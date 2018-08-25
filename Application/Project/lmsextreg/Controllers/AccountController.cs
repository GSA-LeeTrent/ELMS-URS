using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using lmsextreg.Data;
using lmsextreg.Constants;
using lmsextreg.Services;

namespace lmsextreg.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IEventLogService _eventLogService;

        public AccountController
        (
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IEventLogService eventLogSvc
        )
        {
            _signInManager = signInManager;
            _logger = logger;
            _eventLogService = eventLogSvc;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = _signInManager.UserManager.GetUserAsync(User).Result;
            if (user != null)
            {
                ///////////////////////////////////////////////////////////////////
                // Log the 'LOGOUT' event
                ///////////////////////////////////////////////////////////////////
                _eventLogService.LogEvent(EventTypeCodeConstants.LOGOUT, user);   
            }            

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToPage("/Index");
        }
    }
}
