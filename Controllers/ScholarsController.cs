using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScholarTrack.Data;
using ScholarTrack.Models;

namespace ScholarTrack.Controllers;

public class ScholarsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ScholarsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // READ: list all scholars
    public async Task<IActionResult> Index()
    {
        var scholars = await _db.Scholars.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
        return View(scholars);
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
    public async Task<IActionResult> ChangeStatus(int id, ScholarStatus status)
    {
        var scholar = await _db.Scholars.FindAsync(id);
        if (scholar == null) return NotFound();

        scholar.Status = status;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // UPDATE: release or set the allowance back to pending
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAllowance(int id)
    {
        var scholar = await _db.Scholars.FindAsync(id);
        if (scholar == null) return NotFound();

        scholar.Allowance = scholar.Allowance == AllowanceStatus.Released
            ? AllowanceStatus.Pending
            : AllowanceStatus.Released;
        await _db.SaveChangesAsync();

        TempData["Message"] = $"{scholar.FullName}'s allowance is now {scholar.Allowance}.";
        return RedirectToAction(nameof(Index));
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

    private void ValidateProgram(Scholar scholar)
    {
        if (!string.IsNullOrEmpty(scholar.Program) && !ScholarOptions.Programs.Contains(scholar.Program))
        {
            ModelState.AddModelError(nameof(Scholar.Program), "Choose a program from the list.");
        }
    }
}
