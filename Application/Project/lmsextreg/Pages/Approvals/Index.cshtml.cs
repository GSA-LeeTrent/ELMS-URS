using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using lmsextreg.Data;
using lmsextreg.Models;
using lmsextreg.Constants;

namespace lmsextreg.Pages.Approvals
{
    [Authorize(Roles = "APPROVER")]
    public class IndexModel: PageModel
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(lmsextreg.Data.ApplicationDbContext dbCntx, UserManager<ApplicationUser> usrMgr)
        {
            _dbContext = dbCntx;
            _userManager = usrMgr;
        }
        public IList<ProgramEnrollment> ProgramEnrollment { get;set; }
        public ApplicationUser LoggedInUser {get;set;}

        public string PENDING   = StatusCodeConstants.PENDING;
        public string WITHDRAWN = StatusCodeConstants.WITHDRAWN;
        public string APPROVED  = StatusCodeConstants.APPROVED;
        public string DENIED    = StatusCodeConstants.DENIED;
        public string REVOKED   = StatusCodeConstants.REVOKED;

        public async Task OnGetAsync()
        {
            Console.WriteLine("User is APPROVER: " + User.IsInRole(RoleConstants.APPROVER));

            LoggedInUser = await GetCurrentUserAsync();

            if ( User.IsInRole(RoleConstants.APPROVER))
            {
                var loggedInUserID = _userManager.GetUserId(User);

                ///////////////////////////////////////////////////////////////
                // Make sure that the logged-in user with the role of approver
                // is authorized to approve /deny /revoke enrollment
                // requests for this particular LMS Program.
                //////////////////////////////////////////////////////////////
                var sql = " SELECT * "
                        + " FROM " + MiscConstants.DB_SCHEMA_NAME +  ".\"ProgramEnrollment\" "
                        + " WHERE \"LMSProgramID\" " 
                        + " IN "
                        + " ( "
                        + "   SELECT \"LMSProgramID\" "
                        + "   FROM " + MiscConstants.DB_SCHEMA_NAME +  ".\"ProgramApprover\" "
		                + "   WHERE \"ApproverUserId\" = {0} "
	                    + " ) ";

            // Console.WriteLine("SQL: ");
            // Console.WriteLine(sql);                        

            ProgramEnrollment  = await _dbContext.ProgramEnrollments
                                .FromSql(sql, loggedInUserID)
                                .Include( pe =>  pe.LMSProgram)
                                .Include ( pe => pe.Student).ThenInclude(s => s.Agency)
                                .Include( pe => pe.EnrollmentStatus)
                                .Include( pe => pe.Approver)
                                .OrderBy( pe => pe.LMSProgram.LongName)
                                    .ThenBy(pe => pe.Student.FullName)
                                    .ThenBy(pe => pe.EnrollmentStatus.StatusCode)
                                .ToListAsync();

                Console.WriteLine("ProgramEnrollment.Count: " + ProgramEnrollment.Count);
            }
            

        }
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
    }
}