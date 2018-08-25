using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace lmsextreg.Utils
{
    public static class PageModelUtil
    {
        public static string EnsureLocalUrl(PageModel pageModel, string passedInUrl)
        {
            /*************************************************************************************************
                The IsLocalUrl method protects users from being inadvertently redirected to a malicious site.
                You can log the details of the URL that was provided when a non-local URL is supplied in a
                situation where you expected a local URL. Logging redirect URLs may help in diagnosing
                redirection attacks.
            *************************************************************************************************/

            // Console.WriteLine("[PageModelUtil.EnsureLocalUrl] passedInUrl: '" + passedInUrl + "'" );
            // Console.WriteLine("[PageModelUtil.EnsureLocalUrl] passedInUrl IS NULL: " + (passedInUrl == null) );        

            string returnUrl = null;

            if  ( passedInUrl != null )
            {
                if ( pageModel.Url.IsLocalUrl(passedInUrl) )
                {
                    Console.WriteLine("[PageModelUtil.EnsureLocalUrl] " + passedInUrl + " IS local");
                    returnUrl = passedInUrl;
                } 
                else
                {
                    Console.WriteLine("[PageModelUtil.EnsureLocalUrl] " +  passedInUrl + " IS NOT local - returning NULL");
                    returnUrl = null;
                }
            }
 
            return returnUrl;                  
        }

        public static bool ReCaptchaPassed(string gRecaptchaResponse, string secret, ILogger logger)
        {
            return true;

            // HttpClient httpClient = new HttpClient();
            // var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={gRecaptchaResponse}").Result;
            
            // Console.WriteLine("[PageModelUtil.ReCaptchaPassed] res.StatusCode: " + res.StatusCode);
            
            // if (res.StatusCode != HttpStatusCode.OK)
            // {
            //     logger.LogError("Error while sending request to ReCaptcha");
            //     return false;
            // }
            
            // string JSONres = res.Content.ReadAsStringAsync().Result;
            // dynamic JSONdata = JObject.Parse(JSONres);

            // Console.WriteLine("[PageModelUtil.ReCaptchaPassed] JSONres: " + JSONres);
            // Console.WriteLine("[PageModelUtil.ReCaptchaPassed] JSONdata: " + JSONdata);

            // if (JSONdata.success != "true")
            // {
            //     return false;
            // }

            // return true;
        }

    }
}