using HospitalityCustomerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace HospitalityCustomerAPI.Common
{
    public class APIKeyCheckAttribute : ActionFilterAttribute
    {
        private readonly string _passcode;

        public APIKeyCheckAttribute()
        {
            _passcode = AppConstants.HCA_PASSCODE;
        }

        public APIKeyCheckAttribute(string passcode)
        {
            _passcode = passcode;
        }

        private string GetAPIKey()
        {
            return Utility.GetSHA512(_passcode + DateTime.Now.ToString("yyyyMM"));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string apikey = GetAPIKey();

            if (!(context.HttpContext.Request.Headers["token"] == apikey))
            {
                context.Result = new JsonResult(new ResponseModel("TokenIsNotCorrect", ""));
            }

            base.OnActionExecuting(context);
        }
    }
}
