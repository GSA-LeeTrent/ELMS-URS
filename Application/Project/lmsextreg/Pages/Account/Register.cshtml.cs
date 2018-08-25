using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using lmsextreg.Data;
using lmsextreg.Services;
using lmsextreg.Models;
using lmsextreg.Constants;
using lmsextreg.Utils;

namespace lmsextreg.Pages.Account
{
    [AllowAnonymous]    
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEventLogService _eventLogService;

        public RegisterModel
            (
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager,
                ILogger<LoginModel> logger,
                IEmailSender emailSender,
                ApplicationDbContext dbContext,
                RoleManager<IdentityRole> roleManager,
                IConfiguration config,
                IEventLogService eventLogSvc
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _dbContext = dbContext;
            _roleManager = roleManager;
            _configuration = config;
            _eventLogService = eventLogSvc;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public SelectList AgencySelectList { get; set; }
        public SelectList SubAgencySelectList { get; set; }     
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 12)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Middle Name")]
            public string MiddleName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Job Title")]
            public string JobTitle { get; set; }   

            [Required]
            [Display(Name = "Agency")]  
            public string AgencyID { get; set; }

            [Required]
            [Display(Name = "SubAgency")]  
            public string SubAgencyID { get; set; }    

            [BindProperty]
            [Display(Name = "I agree to these Rules of Behavior")]
            [Range(typeof(bool), "true", "true", ErrorMessage = "Rules of Behavior must be agreed to in order to register.")]
            public bool RulesOfBehaviorAgreedTo { get; set; }            
        }

         public void OnGet(string returnUrl = null)
        {
            AgencySelectList    = new SelectList(_dbContext.Agencies.OrderBy(a => a.DisplayOrder), "AgencyID", "AgencyName");
            SubAgencySelectList = new SelectList(_dbContext.SubAgencies.OrderBy(sa => sa.DisplayOrder), "SubAgencyID", "SubAgencyName");
            
            var recaptchaKey = _configuration[MiscConstants.GOOGLE_RECAPTCHA_KEY];
            Console.WriteLine("recaptchaKey: " + recaptchaKey);
            
            var recaptchaSecret = _configuration[MiscConstants.GOOGLE_RECAPTCHA_SECRET];
            Console.WriteLine("recaptchaSecret: " + recaptchaSecret);              

            ViewData["ReCaptchaKey"] = _configuration[MiscConstants.GOOGLE_RECAPTCHA_KEY];            
         
            ReturnUrl = PageModelUtil.EnsureLocalUrl(this, returnUrl);
         }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
 			if ( ! ModelState.IsValid )
			{
                Console.WriteLine("Modelstate is INVALID - returning Page()");
				return Page();
			}             
 
            ReturnUrl = PageModelUtil.EnsureLocalUrl(this, returnUrl);

            ///////////////////////////////////////////////////////////////////   
            // "I'm not a robot" check ...
            ///////////////////////////////////////////////////////////////////   
            if  ( ! PageModelUtil.ReCaptchaPassed
                    (
                        Request.Form["g-recaptcha-response"], // that's how you get it from the Request object
                        _configuration[MiscConstants.GOOGLE_RECAPTCHA_SECRET],
                        _logger
                    )
                )
            {
                Console.WriteLine("reCAPTCHA FAILED");
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // RECAPTCHA FAILED - redisplay form
                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                ModelState.AddModelError(string.Empty, "You failed the CAPTCHA. Are you a robot?");
                AgencySelectList    = new SelectList(_dbContext.Agencies.OrderBy(a => a.DisplayOrder), "AgencyID", "AgencyName");
                SubAgencySelectList = new SelectList(_dbContext.SubAgencies.OrderBy(sa => sa.DisplayOrder), "SubAgencyID", "SubAgencyName");
                ViewData["ReCaptchaKey"] = _configuration[MiscConstants.GOOGLE_RECAPTCHA_KEY];
                return Page();
            }

            Console.WriteLine("reCAPTCHA PASSED");

            if (ModelState.IsValid)
            {
               Console.WriteLine("Modelstate is VALID - processing will continue");

                var user = new ApplicationUser
                { 
                    UserName                = Input.Email, 
                    Email                   = Input.Email,
                    FirstName               = Input.FirstName,
                    MiddleName              = Input.MiddleName,
                    LastName                = Input.LastName,
                    JobTitle                = Input.JobTitle,
                    AgencyID                = Input.AgencyID,
                    SubAgencyID             = Input.SubAgencyID,
                    DateRegistered          = DateTime.Now,
                    DateAccountExpires      = DateTime.Now.AddDays(AccountConstants.DAYS_ACCOUNT_EXPIRES),
                    DatePasswordExpires     =  DateTime.Now.AddDays(AccountConstants.DAYS_PASSWORD_EXPIRES),
                    RulesOfBehaviorAgreedTo = Input.RulesOfBehaviorAgreedTo
                };

                ///////////////////////////////////////////////////////////////////
                // Create User
                ///////////////////////////////////////////////////////////////////
                var result = await _userManager.CreateAsync(user, Input.Password);

                ///////////////////////////////////////////////////////////////////
                // Create User Role 
                ///////////////////////////////////////////////////////////////////
                if (result.Succeeded)
                {
                    result = await _userManager.AddToRoleAsync(user, RoleConstants.STUDENT);
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);
                    await _emailSender.SendEmailConfirmationAsync(Input.Email, callbackUrl);

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //return LocalRedirect(Url.GetLocalUrl(returnUrl));

                    ///////////////////////////////////////////////////////////////////
                    // Log the 'USER_REGISTERED' event
                    ///////////////////////////////////////////////////////////////////
                   _eventLogService.LogEvent(EventTypeCodeConstants.USER_REGISTERED, user);

                    /////////////////////////////////////////////////////////////////////
                    // Redirect user to 'RegisterConfirmation' page
                    /////////////////////////////////////////////////////////////////////
                    return RedirectToPage("./RegisterConfirmation");
                }
                
                _logger.LogDebug("[logger] # of errors: " + result.Errors.Count());
                Console.WriteLine("[console] # of errors: " + result.Errors.Count());

                foreach (var error in result.Errors)
                {
                    _logger.LogDebug(error.Description);
                    Console.WriteLine(error.Description);

                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // If we got this far, something failed, redisplay form
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            AgencySelectList    = new SelectList(_dbContext.Agencies.OrderBy(a => a.DisplayOrder), "AgencyID", "AgencyName");
            SubAgencySelectList = new SelectList(_dbContext.SubAgencies.OrderBy(sa => sa.DisplayOrder), "SubAgencyID", "SubAgencyName");
            ViewData["ReCaptchaKey"] = _configuration[MiscConstants.GOOGLE_RECAPTCHA_KEY];
            return Page();
        }

       public JsonResult OnGetSubAgenciesInAgency(string agyID) 
        {
            List<SubAgency> subAgencyList = _dbContext.SubAgencies.Where( sa => sa.AgencyID == agyID ).OrderBy(sa => sa.DisplayOrder).ToList();
            return new JsonResult(new SelectList(subAgencyList, "SubAgencyID", "SubAgencyName"));
        } 
    }
}