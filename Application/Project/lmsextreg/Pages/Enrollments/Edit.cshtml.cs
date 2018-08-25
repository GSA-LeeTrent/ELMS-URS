using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using lmsextreg.Data;
using lmsextreg.Models;

namespace lmsextreg.Pages.Enrollments
{
    [Authorize(Roles = "STUDENT")]
    public class EditModel : PageModel
    {
        private readonly lmsextreg.Data.ApplicationDbContext _context;

        public EditModel(lmsextreg.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ProgramEnrollment ProgramEnrollment { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProgramEnrollment = await _context.ProgramEnrollments
                .Include(p => p.LMSProgram).SingleOrDefaultAsync(m => m.LMSProgramID == id);

            if (ProgramEnrollment == null)
            {
                return NotFound();
            }
           ViewData["LMSProgramID"] = new SelectList(_context.LMSPrograms, "LMSProgramID", "LongName");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ProgramEnrollment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProgramEnrollmentExists(ProgramEnrollment.LMSProgramID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProgramEnrollmentExists(int id)
        {
            return _context.ProgramEnrollments.Any(e => e.LMSProgramID == id);
        }
    }
}
