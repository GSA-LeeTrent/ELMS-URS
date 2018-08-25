using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using lmsextreg.Data;
using lmsextreg.Constants;

namespace lmsextreg.Authorization
{
    public class CanAccessApproverLinkHandler : AuthorizationHandler<CanAccessApproverLink>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CanAccessApproverLinkHandler(SignInManager<ApplicationUser> signInMgr)
        {
            _signInManager = signInMgr;
        }        

        protected override Task HandleRequirementAsync (AuthorizationHandlerContext authContext,
                                                        CanAccessApproverLink requirement)
        {
            bool isSignedIn         = false;
            bool isTwoFactorEnabled = false;
            bool isApprover         = false;

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
                        isApprover = authContext.User.IsInRole("APPROVER");
                    }
                }
            }
       
            // Console.WriteLine("[CanAccesApproverLinkHandler][HandleRequirementAsync] - (isSignedInn)........: " + isSignedIn);
            // Console.WriteLine("[CanAccesApproverLinkHandler][HandleRequirementAsync] - (isTwoFactorEnabled).: " + isTwoFactorEnabled);
            // Console.WriteLine("[CanAccesApproverLinkHandler][HandleRequirementAsync] - (isApprover).........: " + isApprover);

            if ( isSignedIn 
                    && isTwoFactorEnabled 
                    && isApprover )
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