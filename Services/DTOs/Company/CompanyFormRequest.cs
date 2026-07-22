namespace Services.DTOs.Company;

public class CompanyFormRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? LogoPath { get; set; }
    public string? CompanySize { get; set; }
    public string? Overview { get; set; }
    public string? Website { get; set; }
}
