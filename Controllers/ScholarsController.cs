using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholarTrack.Data;
using ScholarTrack.Models;
using ScholarTrack.ViewModels;

namespace ScholarTrack.Controllers;

public class ScholarsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ScholarsController(ApplicationDbContext db)
    {
        _db = db;
    }

    private const int PageSize = 10;

    // READ: list scholars with search, filters, sorting and paging
    public async Task<IActionResult> Index(
        string? search, string? program, ScholarStatus? status, AllowanceStatus? allowance,
        string? sort, int page = 1)
    {
        search = search?.Trim();
        var sortKey = sort != null && ScholarListViewModel.SortOptions.ContainsKey(sort) ? sort : "name";

        IQueryable<Scholar> query = _db.Scholars.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            // Match part of a name, or an exact ID number.
            query = int.TryParse(search, out var idNumber)
                ? query.Where(s => s.FullName.Contains(search) || s.Id == idNumber)
                : query.Where(s => s.FullName.Contains(search));
        }
        if (!string.IsNullOrEmpty(program)) query = query.Where(s => s.Program == program);
        if (status.HasValue) query = query.Where(s => s.Status == status.Value);
        if (allowance.HasValue) query = query.Where(s => s.Allowance == allowance.Value);

        query = sortKey switch
        {
            "name_desc" => query.OrderByDescending(s => s.FullName),
            "year" => query.OrderBy(s => s.YearLevel).ThenBy(s => s.FullName),
            "newest" => query.OrderByDescending(s => s.Id),
            _ => query.OrderBy(s => s.FullName)
        };

        var totalCount = await query.CountAsync();
        var pageCount = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        page = Math.Clamp(page, 1, pageCount);

        var model = new ScholarListViewModel
        {
            Items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
            Search = search,
            Program = program,
            Status = status,
            Allowance = allowance,
            Sort = sortKey,
            Page = page,
            PageSize = PageSize,
            PageCount = pageCount,
            TotalCount = totalCount,
            OverallCount = await _db.Scholars.CountAsync()
        };
        return View(model);
    }

    // CREATE
    public IActionResult Create()
    {
        return View(new Scholar { Program = ScholarOptions.Programs[0] });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FullName,Program,YearLevel,Status,Allowance")] Scholar scholar)
    {
        ValidateProgram(scholar);
        if (!ModelState.IsValid) return View(scholar);

        _db.Scholars.Add(scholar);
        await _db.SaveChangesAsync();
        TempData["Message"] = $"{scholar.FullName} was added.";
        return RedirectToAction(nameof(Index));
    }

    // UPDATE: edit all fields
    public async Task<IActionResult> Edit(int id)
    {
        var scholar = await _db.Scholars.FindAsync(id);
        if (scholar == null) return NotFound();
        return View(scholar);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Program,YearLevel,Status,Allowance")] Scholar scholar)
    {
        if (id != scholar.Id) return NotFound();

        ValidateProgram(scholar);
        if (!ModelState.IsValid) return View(scholar);

        var existing = await _db.Scholars.FindAsync(id);
        if (existing == null) return NotFound();

        existing.FullName = scholar.FullName;
        existing.Program = scholar.Program;
        existing.YearLevel = scholar.YearLevel;
        existing.Status = scholar.Status;
        existing.Allowance = scholar.Allowance;
        await _db.SaveChangesAsync();

        TempData["Message"] = $"{existing.FullName} was updated.";
        return RedirectToAction(nameof(Index));
    }

    // UPDATE: change status from the list
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, ScholarStatus status, string? returnUrl)
    {
        var scholar = await _db.Scholars.FindAsync(id);
        if (scholar == null) return NotFound();

        scholar.Status = status;
        await _db.SaveChangesAsync();
        return RedirectBack(returnUrl);
    }

    // UPDATE: release or set the allowance back to pending
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAllowance(int id, string? returnUrl)
    {
        var scholar = await _db.Scholars.FindAsync(id);
        if (scholar == null) return NotFound();

        scholar.Allowance = scholar.Allowance == AllowanceStatus.Released
            ? AllowanceStatus.Pending
            : AllowanceStatus.Released;
        await _db.SaveChangesAsync();

        TempData["Message"] = $"{scholar.FullName}'s allowance is now {scholar.Allowance}.";
        return RedirectBack(returnUrl);
    }

    // DELETE: confirmation page, then the actual delete
    public async Task<IActionResult> Delete(int id)
    {
        var scholar = await _db.Scholars.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (scholar == null) return NotFound();
        return View(scholar);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var scholar = await _db.Scholars.FindAsync(id);
        if (scholar != null)
        {
            _db.Scholars.Remove(scholar);
            await _db.SaveChangesAsync();
            TempData["Message"] = $"{scholar.FullName} was deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    // Go back to the list with the same search and filters, but only to a local URL.
    private IActionResult RedirectBack(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
    }

    private void ValidateProgram(Scholar scholar)
    {
        if (!string.IsNullOrEmpty(scholar.Program) && !ScholarOptions.Programs.Contains(scholar.Program))
        {
            ModelState.AddModelError(nameof(Scholar.Program), "Choose a program from the list.");
        }
    }
}
