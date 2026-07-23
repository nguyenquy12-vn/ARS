using UglyToad.PdfPig;

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
            sb.AppendLine(page.Text);
        }
        return sb.ToString().Trim();
    }
}
