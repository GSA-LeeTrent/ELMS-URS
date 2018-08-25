using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using lmsextreg.Data;
using lmsextreg.Models;
using lmsextreg.Constants;

namespace lmsextreg.Pages.Enrollments
{
    [Authorize(Roles = "STUDENT")]
    public class IndexModel : PageModel
    {
        private readonly lmsextreg.Data.ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(lmsextreg.Data.ApplicationDbContext context, UserManager<ApplicationUser> usrMgr)
        {
            _context = context;
            _userManager = usrMgr;
        }

        public string PENDING   = StatusCodeConstants.PENDING;
        public string APPROVED  = StatusCodeConstants.APPROVED;
        public string WITHDRAWN = StatusCodeConstants.WITHDRAWN;
        public string REVOKED   = StatusCodeConstants.REVOKED;
        public string DENIED    = StatusCodeConstants.DENIED;
        public IList<ProgramEnrollment> ProgramEnrollment { get;set; }

        public ApplicationUser LoggedInUser {get;set;}
        public bool ProgramsAreAvailable {get; set; }

        public async Task OnGetAsync()
        {
            LoggedInUser = await GetCurrentUserAsync();

            ProgramEnrollment = await _context.ProgramEnrollments
                .Where(p => p.StudentUserId == LoggedInUser.Id)
                .Include(p => p.LMSProgram)
                .Include ( pe => pe.Student).ThenInclude(s => s.Agency)
                .Include(p => p.EnrollmentStatus)
                .ToListAsync();
           
            var userID = _userManager.GetUserId(User);

            //////////////////////////////////////////////////////////////////////////
            // Select the remaining programs that student has net as yet enrolled in
            // This is used to manage the user interface to make sure that student
            // can't enroll in the same program more than once.
            /////////////////////////////////////////////////////////////////////////
            var sql = " SELECT * "
                    + " FROM " + MiscConstants.DB_SCHEMA_NAME + ".\"LMSProgram\" "
                    + " WHERE \"LMSProgramID\" "
                    + " NOT IN "
                    + " ( SELECT \"LMSProgramID\" "
                    + "   FROM " + MiscConstants.DB_SCHEMA_NAME + ".\"ProgramEnrollment\" " 
                    + "   WHERE \"StudentUserId\" = {0} "
                    + " )";
            var resultSet =  _context.LMSPrograms.FromSql(sql, userID).AsNoTracking();
            ProgramsAreAvailable = (resultSet.Count() > 0);
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
    }
}