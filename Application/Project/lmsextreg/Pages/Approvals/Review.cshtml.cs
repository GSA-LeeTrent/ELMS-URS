using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using lmsextreg.Constants;
using lmsextreg.Data;
using lmsextreg.Models;
using lmsextreg.Services;

namespace lmsextreg.Pages.Approvals
{
    [Authorize(Roles = "APPROVER")]
    public class ReviewModel: PageModel
    {
         private readonly ApplicationDbContext _dbContext;
         private readonly UserManager<ApplicationUser> _userManager;
         private readonly IEmailSender _emailSender;
         private readonly IEventLogService _eventLogService;

        public ReviewModel  
        (
            lmsextreg.Data.ApplicationDbContext dbCntx, 
            UserManager<ApplicationUser> usrMgr,
            IEmailSender emailSndr,
            IEventLogService eventLogSvc
        )
        {
            _dbContext = dbCntx;
            _userManager = usrMgr;
            _emailSender = emailSndr;
            _eventLogService = eventLogSvc;
        }         

        public class InputModel
        {
            [Display(Name = "Remarks")]  
            public string Remarks { get; set; }
        }   


        [BindProperty]
        public InputModel Input { get; set; }
        public ProgramEnrollment ProgramEnrollment { get; set; }
        public IList<EnrollmentHistory> EnrollmentHistory { get;set; }
        public bool ShowReviewForm { get; set; }
        public bool ShowApproveButton {get; set; }
        public bool ShowDenyButton {get; set; }
        public bool ShowRevokeButton {get; set; }

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
            var sql = " SELECT * "
                    + "   FROM " + MiscConstants.DB_SCHEMA_NAME + ".\"ProgramEnrollment\" "
                    + "  WHERE  \"ProgramEnrollmentID\" = {0} "
                    + "    AND  \"LMSProgramID\" " 
                    + "     IN "
                    + "      ( "
                    + "        SELECT \"LMSProgramID\" "
                    + "          FROM " + MiscConstants.DB_SCHEMA_NAME + ".\"ProgramApprover\" "
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

            EnrollmentHistory = await _dbContext.EnrollmentHistories
                                    .Where(eh => eh.ProgramEnrollmentID == ProgramEnrollment.ProgramEnrollmentID)
                                    .Include(eh => eh.Actor)
                                    .Include(eh => eh.StatusTransition)
                                    .OrderBy(eh => eh.EnrollmentHistoryID)
                                    .ToListAsync();
                                    
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

            if ( ProgramEnrollment.StatusCode == StatusCodeConstants.PENDING )
            {
                ShowReviewForm      = true;
                ShowApproveButton   = true;
                ShowDenyButton      = true;
                ShowRevokeButton    = false;
            }
            if ( ProgramEnrollment.StatusCode == StatusCodeConstants.APPROVED )
            {
                ShowReviewForm      = true;
                ShowApproveButton   = false;
                ShowDenyButton      = false;
                ShowRevokeButton    = true;
            }   
            if ( ProgramEnrollment.StatusCode == StatusCodeConstants.DENIED )
            {
                ShowReviewForm      = true;
                ShowApproveButton   = true;
                ShowDenyButton      = false;
                ShowRevokeButton    = false;
            }   
            if ( ProgramEnrollment.StatusCode == StatusCodeConstants.REVOKED )
            {
                ShowReviewForm      = false;
                ShowApproveButton   = false;
                ShowDenyButton      = false;
                ShowRevokeButton    = false;
            }                                    

            return Page();                              
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            return await this.OnPostAsync
            (
                id, 
                StatusCodeConstants.APPROVED, 
                TransitionCodeConstants.PENDING_TO_APPROVED
            );
        }

        public async Task<IActionResult> OnPostDenyAsync(int id)
        {
            ///////////////////////////////////////////////////////////////
            // Remarks are required when Enrollment Request is DENIED
            ///////////////////////////////////////////////////////////////
            if ( String.IsNullOrEmpty(Input.Remarks))
            {
                ModelState.AddModelError("ApproverRemarks", "Remarks are required when enrollment request has been denied");

                ////////////////////////////////////////////////////////////////////////////////////////////////
                // Rebuild page 
                ////////////////////////////////////////////////////////////////////////////////////////////////
                ProgramEnrollment = await this.retrieveProgramEnrollment(this.createAuthorizationSQL(), id);
                EnrollmentHistory = await this.retrieveEnrollmentHistory(ProgramEnrollment.ProgramEnrollmentID); 
                ShowReviewForm      = true;
                ShowApproveButton   = true;
                ShowDenyButton      = true;
                ShowRevokeButton    = false;
                return Page();
            }

            return await this.OnPostAsync
            (
                id, 
                StatusCodeConstants.DENIED, 
                TransitionCodeConstants.PENDING_TO_DENIED
            );
        }
        public async Task<IActionResult> OnPostRevokeAsync(int id)
        {
            return await this.OnPostAsync
            (
                id, 
                StatusCodeConstants.REVOKED, 
                TransitionCodeConstants.APPROVED_TO_REVOKED
            );
        }

        public async Task<IActionResult> OnPostAsync(int programEnrollmentID, string statusCode, string statusTransitionCode)
        {
            Console.WriteLine("Approvals.Review.OnPostAsync(): BEGIN");
            Console.WriteLine("id: " + programEnrollmentID);
            Console.WriteLine("Remarks: " + Input.Remarks);

            ////////////////////////////////////////////////////////////
            // Step #1:
            // Check to see if records exists
            ////////////////////////////////////////////////////////////
            var lvProgramEnrollment = await _dbContext.ProgramEnrollments
                                        .Where(pe => pe.ProgramEnrollmentID == programEnrollmentID) 
                                        .AsNoTracking()
                                        .SingleOrDefaultAsync();     

            ////////////////////////////////////////////////////////////
            // Return "Not Found" if record doesn't exist
            ////////////////////////////////////////////////////////////
            if (lvProgramEnrollment == null)
            {
                Console.WriteLine("ProgramEnrollment NOT FOUND in Step #1");
                return NotFound();
            } 

            ////////////////////////////////////////////////////////////
            // Step #2:
            // Now that record exists, make sure that the logged-in user
            // is authorized to edit (approver/deny) enrollment
            // applications for this particular LMS Program.
            ////////////////////////////////////////////////////////////
            var sql = " SELECT * "
                    + "   FROM " + MiscConstants.DB_SCHEMA_NAME + ".\"ProgramEnrollment\" "
                    + "  WHERE  \"ProgramEnrollmentID\" = {0} "
                    + "    AND  \"LMSProgramID\" " 
                    + "     IN "
                    + "      ( "
                    + "        SELECT \"LMSProgramID\" "
                    + "          FROM " + MiscConstants.DB_SCHEMA_NAME + ".\"ProgramApprover\" "
                    + "         WHERE \"ApproverUserId\" = {1} "
                    + "      ) ";

            lvProgramEnrollment = null;
            lvProgramEnrollment = await _dbContext.ProgramEnrollments
                                .FromSql(sql, programEnrollmentID, _userManager.GetUserId(User))
                                .Include(pe => pe.EnrollmentHistory)
                                .SingleOrDefaultAsync();

            /////////////////////////////////////////////////////////////
            // We already know that record exists from Step #1 so if we
            // get a "Not Found" in Step #2, we know it's because the 
            // logged-in user is not authorized to edit (approve/deny)
            // enrollment applications for this LMS Program.
            /////////////////////////////////////////////////////////////
            if (lvProgramEnrollment == null)
            {
                return Unauthorized();
            }            

            ////////////////////////////////////////////////////////////
            // Retrieve the correct StatusTransition
            ////////////////////////////////////////////////////////////            
            var lvStatusTranstion = await _dbContext.StatusTransitions
                                    .Where(st => st.TransitionCode == statusTransitionCode)
                                    .AsNoTracking()
                                    .SingleOrDefaultAsync();
            
            ////////////////////////////////////////////////////////////////
            // Create EnrollmentHistory using the correct StatusTranistion
            ////////////////////////////////////////////////////////////////            
            var lvEnrollmentHistory = new EnrollmentHistory()
            {
                    StatusTransitionID = lvStatusTranstion.StatusTransitionID,
                    ActorUserId = _userManager.GetUserId(User),
                    ActorRemarks = Input.Remarks,
                    DateCreated = DateTime.Now
            };

            ////////////////////////////////////////////////////////////
            // Instantiate EnrollmentHistory, if necessary
            ////////////////////////////////////////////////////////////
            if ( lvProgramEnrollment.EnrollmentHistory == null) 
            {
                lvProgramEnrollment.EnrollmentHistory = new List<EnrollmentHistory>();
            }

            ///////////////////////////////////////////////////////////////////
            // Add newly created EnrollmentHistory with StatusTransition  
            // to ProgramEnrollment's EnrollmentHistory Collection
            ///////////////////////////////////////////////////////////////////            
            lvProgramEnrollment.EnrollmentHistory.Add(lvEnrollmentHistory);

            /////////////////////////////////////////////////////////////////
            // Update ProgramEnrollment Record with
            //  1. EnrollmentStatus of "APPROVED"
            //  2. ApproverUserId (logged-in user)
            //  3. EnrollmentHistory (PENDING TO APPROVED)
            /////////////////////////////////////////////////////////////////
            lvProgramEnrollment.StatusCode = statusCode;
            lvProgramEnrollment.ApproverUserId = _userManager.GetUserId(User);
            _dbContext.ProgramEnrollments.Update(lvProgramEnrollment);
            await _dbContext.SaveChangesAsync();

            //////////////////////////////////////////////////////////////////////////////
            // Retrieve 'User as Approver' for event logging
            //////////////////////////////////////////////////////////////////////////////
            ApplicationUser approver = await GetCurrentUserAsync();
            
            //////////////////////////////////////////////////////////////////
            // Determine EventType StatusCode
            //////////////////////////////////////////////////////////////////
            string eventTypeCode = null;
            if ( statusCode.Equals(StatusCodeConstants.APPROVED) )
            {
                eventTypeCode = EventTypeCodeConstants.ENROLLMENT_APPROVED;
            }
            if ( statusCode.Equals(StatusCodeConstants.DENIED) )
            {
                eventTypeCode = EventTypeCodeConstants.ENROLLMENT_DENIED;
            }     
            if ( statusCode.Equals(StatusCodeConstants.REVOKED) )
            {
                eventTypeCode = EventTypeCodeConstants.ENROLLMENT_REVOKED;
            }                             

            /////////////////////////////////////////////////////////////////////////////////////////////
            // Log the ENROLLMENT <APPROVED || DENIED || REVOKED> event
            /////////////////////////////////////////////////////////////////////////////////////////////
            _eventLogService.LogEvent(eventTypeCode, approver, lvProgramEnrollment.ProgramEnrollmentID); 

            /////////////////////////////////////////////////////////////////////
            // Send email notification to student, advising him/her of
            // program enrollment status
            /////////////////////////////////////////////////////////////////////
            lvProgramEnrollment = null;
            lvProgramEnrollment = await _dbContext.ProgramEnrollments
                                .Where      ( pe => pe.ProgramEnrollmentID == programEnrollmentID)
                                .Include    ( pe => pe.LMSProgram)
                                .Include    ( pe => pe.Student)
                                .Include    ( pe => pe.EnrollmentStatus)
                                .AsNoTracking()
                                .SingleOrDefaultAsync(); 

            ApplicationUser student =  lvProgramEnrollment.Student;
            string email = student.Email;
            string subject  = lvProgramEnrollment.LMSProgram.LongName 
                            + " Enrollment Status";
            string message  = "Your request to enroll in the " 
                            + lvProgramEnrollment.LMSProgram.LongName 
                            + " has been " 
                            + lvProgramEnrollment.EnrollmentStatus.StatusLabel
                            + ".";
            if  ( StatusCodeConstants.DENIED.Equals(lvProgramEnrollment.EnrollmentStatus.StatusCode))          
            {
                string referrer = Request.Headers["Referer"];
                // TODO: Switch to a more professional way of doing this!
                int end = referrer.IndexOf("Approvals"); // TEMPORARY!!
                
                string baseUrl = referrer.Substring(0, end);
                Console.WriteLine("baseUrl: " );
                Console.WriteLine(baseUrl);       
           
                var studentLoginPath = baseUrl + "Account/Login";
                Console.WriteLine("studentLoginPath: " );
                Console.WriteLine(studentLoginPath);
                message += " <a href='" + studentLoginPath + "'>Log-in</a> "
                        + "to the training registration system for more information regarding the denial.";
            }      
            await _emailSender.SendEmailAsync(email, subject, message);

            /////////////////////////////////////////////////////////////////
            // Redirect to Approval Index Page
            /////////////////////////////////////////////////////////////////
            return RedirectToPage("./Index");          
        }

        private string createAuthorizationSQL() 
        {
            return    " SELECT * "
                    + "   FROM " + MiscConstants.DB_SCHEMA_NAME + ".\"ProgramEnrollment\" "
                    + "  WHERE  \"ProgramEnrollmentID\" = {0} "
                    + "    AND  \"LMSProgramID\" " 
                    + "     IN "
                    + "      ( "
                    + "        SELECT \"LMSProgramID\" "
                    + "          FROM " + MiscConstants.DB_SCHEMA_NAME + ".\"ProgramApprover\" "
                    + "         WHERE \"ApproverUserId\" = {1} "
                    + "      ) ";
            
        }
        private async Task<ProgramEnrollment> retrieveProgramEnrollment(string sql, int programEnrollmentID)   
        {
            return await _dbContext.ProgramEnrollments
                            .FromSql(sql, programEnrollmentID, _userManager.GetUserId(User))
                            .Include(pe => pe.LMSProgram)
                            .Include(pe => pe.Student)
                                .ThenInclude(s => s.SubAgency)
                                .ThenInclude(sa => sa.Agency)
                            .Include(pe => pe.EnrollmentStatus)
                            .SingleOrDefaultAsync();
        }     

       private async Task<List<EnrollmentHistory>> retrieveEnrollmentHistory(int programEnrollmentID)   
        {
            return await _dbContext.EnrollmentHistories
                                    .Where(eh => eh.ProgramEnrollmentID == programEnrollmentID)
                                    .Include(eh => eh.Actor)
                                    .Include(eh => eh.StatusTransition)
                                    .OrderBy(eh => eh.EnrollmentHistoryID)
                                    .ToListAsync();
        }
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);                  
    }
}