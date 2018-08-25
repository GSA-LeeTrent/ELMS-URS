using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using lmsextreg.Data;
using lmsextreg.Models;

namespace lmsextreg.Pages.Approvals
{
    [Authorize(Roles = "APPROVER")]
    public class EditModel: PageModel
    {
         private readonly ApplicationDbContext _dbContext;
         private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(lmsextreg.Data.ApplicationDbContext dbCntx, UserManager<ApplicationUser> usrMgr)
        {
            _dbContext = dbCntx;
            _userManager = usrMgr;
        }         

        [BindProperty]
        public ProgramEnrollment ProgramEnrollment { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ////////////////////////////////////////////////////////////
            // Step #1:
            // Check to see if records exists
            ////////////////////////////////////////////////////////////
            ProgramEnrollment = await _dbContext.ProgramEnrollments
                              .Where(pe => pe.ProgramEnrollmentID == id) 
                              .SingleOrDefaultAsync();     

            ////////////////////////////////////////////////////////////
            // Return "Not Found" if record doesn't exist
            ////////////////////////////////////////////////////////////
            if (ProgramEnrollment == null)
            {
                return NotFound();
            }                                     

            ////////////////////////////////////////////////////////////
            // Step #2:
            // Now that record exists, make sure that the logged-in user
            // is authorized to edit (approver/deny) enrollment
            // applications for this particular LMS Program.
            ////////////////////////////////////////////////////////////
            var sql = " SELECT * FROM public.\"ProgramEnrollment\" "
                    + "  WHERE  \"ProgramEnrollmentID\" = {0} "
                    + "    AND  \"LMSProgramID\" " 
                    + "     IN "
                    + "      ( "
                    + "        SELECT \"LMSProgramID\" "
                    + "          FROM public.\"ProgramApprover\" "
                    + "         WHERE \"ApproverUserId\" = {1} "
                    + "      ) ";

            ProgramEnrollment = null;
            ProgramEnrollment = await _dbContext.ProgramEnrollments
                                .FromSql(sql, id, _userManager.GetUserId(User))
                                .Include(pe => pe.LMSProgram)
                                .Include(pe => pe.Student)
                                    .ThenInclude(s => s.SubAgency)
                                    .ThenInclude(sa => sa.Agency)
                                .Include(pe => pe.EnrollmentStatus)
                                .SingleOrDefaultAsync();

            /////////////////////////////////////////////////////////////
            // We already know that record exists from Step #1 so if we
            // get a "Not Found" in Step #2, we know it's because the 
            // logged-in user is not authorized to edit (approve/deny)
            // enrollment applications for this LMS Program.
            /////////////////////////////////////////////////////////////
            if (ProgramEnrollment == null)
            {
                return Unauthorized();
            }
            
            return Page();                              
        }

        public async Task<IActionResult> OnPostAsync(int id, string status)
        {
            Console.WriteLine("Approvals.Edit.OnPost(): BEGIN");
            Console.WriteLine("id: " + id);
            Console.WriteLine("status: " + status);

            ////////////////////////////////////////////////////////////
            // Step #1:
            // Check to see if records exists
            ////////////////////////////////////////////////////////////
            ProgramEnrollment = await _dbContext.ProgramEnrollments
                              .Where(pe => pe.ProgramEnrollmentID == id) 
                              .SingleOrDefaultAsync();     

            ////////////////////////////////////////////////////////////
            // Return "Not Found" if record doesn't exist
            ////////////////////////////////////////////////////////////
            if (ProgramEnrollment == null)
            {
                Console.WriteLine("ProgramEnrollment NOT FOUND in Step# 1");
                return NotFound();
            } 

            ////////////////////////////////////////////////////////////
            // Step #2:
            // Now that record exists, make sure that the logged-in user
            // is authorized to edit (approver/deny) enrollment
            // applications for this particular LMS Program.
            ////////////////////////////////////////////////////////////
            var sql = " SELECT * FROM public.\"ProgramEnrollment\" "
                    + "  WHERE  \"ProgramEnrollmentID\" = {0} "
                    + "    AND  \"LMSProgramID\" " 
                    + "     IN "
                    + "      ( "
                    + "        SELECT \"LMSProgramID\" "
                    + "          FROM public.\"ProgramApprover\" "
                    + "         WHERE \"ApproverUserId\" = {1} "
                    + "      ) ";

            ProgramEnrollment = null;
            ProgramEnrollment = await _dbContext.ProgramEnrollments
                                .FromSql(sql, id, _userManager.GetUserId(User))
                                .SingleOrDefaultAsync();

            /////////////////////////////////////////////////////////////
            // We already know that record exists from Step #1 so if we
            // get a "Not Found" in Step #2, we know it's because the 
            // logged-in user is not authorized to edit (approve/deny)
            // enrollment applications for this LMS Program.
            /////////////////////////////////////////////////////////////
            if (ProgramEnrollment == null)
            {
                return Unauthorized();
            }            

            /////////////////////////////////////////////////////////////////
            // Update ProgramEnrollment Record
            /////////////////////////////////////////////////////////////////
            ProgramEnrollment.StatusCode = status;
            ProgramEnrollment.ApproverUserId = _userManager.GetUserId(User);
            _dbContext.ProgramEnrollments.Update(ProgramEnrollment);
            await _dbContext.SaveChangesAsync();

            /////////////////////////////////////////////////////////////////
            // Redirect to Approval Index Page
            /////////////////////////////////////////////////////////////////
            return RedirectToPage("./Index");          
        }
    }
}