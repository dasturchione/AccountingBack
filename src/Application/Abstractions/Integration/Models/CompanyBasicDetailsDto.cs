using System.Text.Json.Serialization;

namespace Application.Abstractions.Integration.Models;

public class CompanyBasicDetailsDto
{
    [JsonPropertyName("CompanyInn")]      public string CompanyInn      { get; set; } = string.Empty;
    [JsonPropertyName("Pinfl")]           public string Pinfl           { get; set; } = string.Empty;
    [JsonPropertyName("CompanyName")]     public string CompanyName     { get; set; } = string.Empty;
    [JsonPropertyName("CompanyAddress")]  public string CompanyAddress  { get; set; } = string.Empty;
    [JsonPropertyName("RegionCode")]      public string RegionCode      { get; set; } = string.Empty;
    [JsonPropertyName("Region")]          public string Region          { get; set; } = string.Empty;
    [JsonPropertyName("DistrictCode")]    public string DistrictCode    { get; set; } = string.Empty;
    [JsonPropertyName("District")]        public string District        { get; set; } = string.Empty;
    [JsonPropertyName("PhoneNumber")]     public string PhoneNumber     { get; set; } = string.Empty;
    [JsonPropertyName("Email")]           public string Email           { get; set; } = string.Empty;
    [JsonPropertyName("VatCode")]         public string VatCode         { get; set; } = string.Empty;
    [JsonPropertyName("SpecialAccount")]  public string SpecialAccount  { get; set; } = string.Empty;
    [JsonPropertyName("Accounts")]        public List<CompanyAccountDto> Accounts { get; set; } = [];
    [JsonPropertyName("DirectorInn")]     public string DirectorInn     { get; set; } = string.Empty;
    [JsonPropertyName("DirectorPinfl")]   public string DirectorPinfl   { get; set; } = string.Empty;
    [JsonPropertyName("DirectorName")]    public string DirectorName    { get; set; } = string.Empty;
    [JsonPropertyName("Accountant")]      public string Accountant      { get; set; } = string.Empty;
    [JsonPropertyName("Oked")]            public string Oked            { get; set; } = string.Empty;
    [JsonPropertyName("TaxGap")]          public string TaxGap          { get; set; } = string.Empty;
    [JsonPropertyName("TaxPayerTypeName")] public string TaxPayerTypeName { get; set; } = string.Empty;
    [JsonPropertyName("Branches")]        public List<CompanyBranchDto> Branches { get; set; } = [];
}
