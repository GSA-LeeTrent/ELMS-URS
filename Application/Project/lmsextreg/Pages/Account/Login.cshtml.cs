using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using lmsextreg.Data;
using lmsextreg.Utils;
using lmsextreg.Constants;
using lmsextreg.Models;
using lmsextreg.Services;

namespace lmsextreg.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEventLogService _eventLogService;

        public LoginModel
        (
            SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger,
            IConfiguration config,
            IEventLogService eventLogSvc
        )
        {
            _signInManager      = signInManager;
            _logger             = logger;
            _configuration      = config;
            _eventLogService    = eventLogSvc;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; } = false;

            [BindProperty]
            [Display(Name = "I agree to these Rules of Behavior")]
            [Range(typeof(bool), "true", "true", ErrorMessage = "Rules of Behavior must be agreed to in order to log in.")]
            public bool RulesOfBehaviorAgreedTo { get; set; }             
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            //Console.WriteLine("[Login][OnGetAsync] - returnUrl: " + returnUrl);

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Make sure that passed-in 'returnUrl' is of a local origin
            //this.ReturnUrl = PageModelUtil.EnsureLocalUrl(this, returnUrl);

            // I'm not a robot
            ViewData["ReCaptchaKey"] = _configuration[MiscConstants.GOOGLE_RECAPTCHA_KEY]; 
            this.ReturnUrl = returnUrl;
            //Console.WriteLine("[Login][OnGetAsync] - this.ReturnUrl: " + this.ReturnUrl);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {          
            //Console.WriteLine("[Login][OnPostAsync] - BEGIN");

            // Make sure that passed-in 'returnUrl' is of a local origin
            //this.ReturnUrl = PageModelUtil.EnsureLocalUrl(this, returnUrl);
            this.ReturnUrl = returnUrl;
            //Console.WriteLine("[Login][OnPostAsync] - returnUrl: " + returnUrl);

            ///////////////////////////////////////////////////////////////////   
            // "I'm not a robot" check ...
            ///////////////////////////////////////////////////////////////////   
            if  ( ! PageModelUtil.ReCaptchaPassed
                    (
                        Request.Form["g-recaptcha-response"],
                        _configuration[MiscConstants.GOOGLE_RECAPTCHA_SECRET],
                        _logger
                    )
                )
            {
                //Console.WriteLine("[Login.OnPostAsync] reCAPTCHA FAILED");
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // RECAPTCHA FAILED - redisplay form
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ModelState.AddModelError(string.Empty, "You failed the CAPTCHA. Are you a robot?");
                ViewData["ReCaptchaKey"] = _configuration[MiscConstants.GOOGLE_RECAPTCHA_KEY];
                return Page();
            }

            //Console.WriteLine("[Login.OnPostAsync] reCAPTCHA PASSED");
            //Console.WriteLine("[Login][OnPostAsync] - ModelState.IsValid: " + ModelState.IsValid);

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                // Console.WriteLine("Input.Email: " + Input.Email);
                // Console.WriteLine("Input.Password: " + Input.Password);
                // Console.WriteLine("Input.RememberMe: " + Input.RememberMe);                

                Input.RememberMe = false;
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                
                //Console.WriteLine("[Login][OnPostAsync] - SignInResult.Succeeded........: " + result.Succeeded);
                //Console.WriteLine("[Login][OnPostAsync] - SignInResult.RequiresTwoFactor: " + result.RequiresTwoFactor);
                //Console.WriteLine("[Login][OnPostAsync] - SignInResult.IsLockedOut......: " + result.IsLockedOut);

                if (result.Succeeded)
                {
                    _logger.LogInformation("[Login][OnPostAsync] - SignInResult succeeded");

                    ApplicationUser user = await _signInManager.UserManager.FindByNameAsync(Input.Email);

                    ///////////////////////////////////////////////////////////////////
                    // Log the 'LOGIN' event
                    ///////////////////////////////////////////////////////////////////
                   _eventLogService.LogEvent(EventTypeCodeConstants.LOGIN, user);                          

                    //Console.WriteLine("[Login][OnPostAsync] - Password Expired: " + (user.DatePasswordExpires <= DateTime.Now) );
                    if (user.DatePasswordExpires <= DateTime.Now)
                    {
                        //Console.WriteLine("[Login][OnPostAsync] - Password expired, redirecting to './Manage/ChangePassword' page");
                        return RedirectToPage("./Manage/ChangePassword");
                    }

                    //Console.WriteLine("[Login][OnPostAsync] - ApplicationUser.TwoFactorEnabled: " + user.TwoFactorEnabled);
                    if ( user.TwoFactorEnabled == false)
                    {
                        //Console.WriteLine("[Login][OnPostAsync] - Two-factor auth NOT enabled, redirecting to './Manage/EnableAuthenticator' page");
                        return RedirectToPage("./Manage/EnableAuthenticator");
                    }
                    //Console.WriteLine("[Login][OnPostAsync] - Two-factor auth IS enabled");

                    return LocalRedirect(Url.GetLocalUrl(returnUrl));
                }
                if (result.RequiresTwoFactor)
                {
                    //Console.WriteLine("[Login][OnPostAsync] - redirecting to './LoginWith2fa' page");
                    Input.RememberMe = false;
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    ViewData["ReCaptchaKey"] = _configuration[MiscConstants.GOOGLE_RECAPTCHA_KEY];
                    return Page();
                }
            }

            ///////////////////////////////////////////////////////////////////////////////
            // If we got this far, something failed, redisplay form
            ///////////////////////////////////////////////////////////////////////////////            
            ViewData["ReCaptchaKey"] = _configuration[MiscConstants.GOOGLE_RECAPTCHA_KEY];
            return Page();
        }
    }
}