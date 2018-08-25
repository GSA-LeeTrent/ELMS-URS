using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using lmsextreg.Data;
using lmsextreg.Constants;

namespace lmsextreg.Authorization
{
    public class CanAccessStudentLinkHandler : AuthorizationHandler<CanAccessStudentLink>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CanAccessStudentLinkHandler(SignInManager<ApplicationUser> signInMgr)
        {
            _signInManager = signInMgr;
        }        

        protected override Task HandleRequirementAsync (AuthorizationHandlerContext authContext,
                                                        CanAccessStudentLink requirement)
        {
            bool isSignedIn         = false;
            bool isTwoFactorEnabled = false;
            bool isStudent          = false;

            isSignedIn = _signInManager.IsSignedIn(authContext.User);

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
                        isStudent = authContext.User.IsInRole("STUDENT");
                    }
                }
            }
       
            // Console.WriteLine("[CanAccessStudentLinkHandler][HandleRequirementAsync] - (isSignedInn)........: " + isSignedIn);
            // Console.WriteLine("[CanAccessStudentLinkHandler][HandleRequirementAsync] - (isTwoFactorEnabled).: " + isTwoFactorEnabled);
            // Console.WriteLine("[CanAccessStudentLinkHandler][HandleRequirementAsync] - (isStudent)..........: " + isStudent);

            if ( isSignedIn 
                    && isTwoFactorEnabled 
                    && isStudent )
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