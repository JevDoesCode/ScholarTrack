using ScholarTrack.Models;

namespace ScholarTrack.ViewModels;

// Carries the search, filter, sort and paging state between the controller and the Index view.
public class ScholarListViewModel
{
    public static readonly Dictionary<string, string> SortOptions = new()
    {
        ["name"] = "Name (A to Z)",
        ["name_desc"] = "Name (Z to A)",
        ["year"] = "Year level (low to high)",
        ["newest"] = "Newest first"
    };

    public List<Scholar> Items { get; set; } = new();

    public string? Search { get; set; }
    public string? Program { get; set; }
    public ScholarStatus? Status { get; set; }
    public AllowanceStatus? Allowance { get; set; }
    public string Sort { get; set; } = "name";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public int PageCount { get; set; } = 1;
    public int TotalCount { get; set; }    // scholars matching the current search and filters
    public int OverallCount { get; set; }  // all scholars in the database

    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrEmpty(Program) || Status.HasValue || Allowance.HasValue;

    public int FirstItem => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int LastItem => Math.Min(Page * PageSize, TotalCount);

    // Route values for the Previous/Next links, so paging keeps the search and filters.
    public Dictionary<string, string> RouteValues(int page)
    {
        var values = new Dictionary<string, string> { ["page"] = page.ToString() };
        if (!string.IsNullOrWhiteSpace(Search)) values["search"] = Search;
        if (!string.IsNullOrEmpty(Program)) values["program"] = Program;
        if (Status.HasValue) values["status"] = Status.Value.ToString();
        if (Allowance.HasValue) values["allowance"] = Allowance.Value.ToString();
        if (Sort != "name") values["sort"] = Sort;
        return values;
    }
}
