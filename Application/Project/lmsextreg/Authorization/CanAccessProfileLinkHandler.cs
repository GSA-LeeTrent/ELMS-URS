using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using lmsextreg.Data;
using lmsextreg.Constants;

namespace lmsextreg.Authorization
{
    public class CanAccessProfileLinkHandler : AuthorizationHandler<CanAccessProfileLink>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CanAccessProfileLinkHandler(SignInManager<ApplicationUser> signInMgr)
        {
            _signInManager = signInMgr;
        }        

        protected override Task HandleRequirementAsync (AuthorizationHandlerContext authContext,
                                                        CanAccessProfileLink requirement)
        {
            bool isSignedIn             = false;
            bool isTwoFactorEnabled     = false;
            bool isStudentOrApprover    = false;

            if ( _signInManager != null )
            {
                isSignedIn = _signInManager.IsSignedIn(authContext.User);
            }

            if ( isSignedIn )
            {
                string appUserId = _signInManager.UserManager.GetUserId(authContext.User);

                if ( String.IsNullOrEmpty(appUserId) == false )
                {
                    ApplicationUser appUser = GetUserAsync(appUserId).Result;

                    if ( appUser != null) 
                    {
                        isTwoFactorEnabled = appUser.TwoFactorEnabled;
                    }

                    if ( authContext != null && authContext.User != null)
                    {
                        bool isStudent      = authContext.User.IsInRole("STUDENT");
                        bool isApprover     = authContext.User.IsInRole("APPROVER");
                        isStudentOrApprover = (isStudent || isApprover);
                    }
                }
            }
       
            // Console.WriteLine("[CanAccessProfileLinkHandler][HandleRequirementAsync] - (isSignedInn)........: " + isSignedIn);
            // Console.WriteLine("[CanAccessProfileLinkHandler][HandleRequirementAsync] - (isTwoFactorEnabled).: " + isTwoFactorEnabled);
            // Console.WriteLine("[CanAccessProfileLinkHandler][HandleRequirementAsync] - (isStudentOrApprover.: " + isStudentOrApprover);

            if ( isSignedIn 
                    && isTwoFactorEnabled 
                    && isStudentOrApprover )
            {
                authContext.Succeed(requirement);
            }            

            return Task.CompletedTask;
        }

        private  Task<ApplicationUser> GetUserAsync(string userId)
        {
            return _signInManager.UserManager.FindByIdAsync(userId);
        }
    }
}