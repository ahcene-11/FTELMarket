using System.ComponentModel.DataAnnotations;
namespace FTELMarket.Settings;

public class FlakySettings
{
    [Required(AllowEmptyStrings = false)]
    public string BaseAddress { get; set; } = string.Empty;
}