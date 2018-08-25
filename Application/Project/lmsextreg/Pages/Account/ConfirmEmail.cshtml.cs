using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using lmsextreg.Constants;
using lmsextreg.Data;
using lmsextreg.Services;

namespace lmsextreg.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEventLogService _eventLogService;

        public ConfirmEmailModel
        (
            UserManager<ApplicationUser> userManager,
            IEventLogService eventLogSvc
        )
        {
            _userManager = userManager;
            _eventLogService = eventLogSvc;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Error confirming email for user with ID '{userId}':");
            }
            ///////////////////////////////////////////////////////////////////
            // Log the 'EMAIL_CONFIRMED' event
            ///////////////////////////////////////////////////////////////////
            _eventLogService.LogEvent(EventTypeCodeConstants.EMAIL_CONFIRMED, user);             

            return Page();
        }
    }
}
