using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Services.Implementations;

// Đọc toàn bộ text từ file PDF bằng thư viện PdfPig (đã tham chiếu sẵn trong Services.csproj).
public static class PdfTextExtractor
{
    public static string Extract(Stream pdfStream)
    {
        using var document = PdfDocument.Open(pdfStream);
        var sb = new System.Text.StringBuilder();
        foreach (var page in document.GetPages())
        {
            // ContentOrderTextExtractor đọc theo đúng thứ tự đọc + tự thêm khoảng trắng/xuống dòng
            // (tốt hơn nhiều so với page.Text vốn hay dính chữ, sai thứ tự).
            string text;
            try
            {
                text = ContentOrderTextExtractor.GetText(page);
            }
            catch
            {
                text = page.Text; // dự phòng nếu trang lỗi layout
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text);
                sb.AppendLine();
            }
        }
        return sb.ToString().Trim();
    }
}

