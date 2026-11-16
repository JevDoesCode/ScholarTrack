using System.ComponentModel.DataAnnotations;

namespace ScholarTrack.Models;

public enum ScholarStatus
{
    Active,
    Inactive,
    Graduated
}

public enum AllowanceStatus
{
    Pending,
    Released
}

public class Scholar
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter the scholar's full name.")]
    [StringLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a program.")]
    public string Program { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Year level must be from 1 to 5.")]
    [Display(Name = "Year level")]
    public int YearLevel { get; set; } = 1;

    public ScholarStatus Status { get; set; } = ScholarStatus.Active;

    public AllowanceStatus Allowance { get; set; } = AllowanceStatus.Pending;
}
