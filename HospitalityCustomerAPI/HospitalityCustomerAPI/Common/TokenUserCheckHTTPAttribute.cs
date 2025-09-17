using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.Controllers;
using HospitalityCustomerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace HISCustomerAPI.Common
{
    /// <summary>
    /// Filter attribute để kiểm tra và xác thực token trong HTTP Header
    /// Token format: base64EncodedObject.signature
    /// Signature = SHA256(base64EncodedObject + DKKB_PASSCODE + currentYearMonth)
    /// </summary>
    public class TokenUserCheckHTTP : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                // Kiểm tra và ép kiểu controller
                var controller = context.Controller as ControllerBase;
                if (controller == null)
                {
                    context.Result = new JsonResult(new ResponseModel("TokenIsNotCorrect", ""));
                    return;
                }

                // Lấy token từ HTTP Header với key "token"
                string token = controller.Request.Headers["token"].ToString();
                if (string.IsNullOrEmpty(token))
                {
                    context.Result = new JsonResult(new ResponseModel("TokenIsNotCorrect", ""));
                    return;
                }

                // Tách token thành 2 phần: base64 object và signature
                string[] arrToken = token.Split('.');
                string obj = arrToken[0];       // Phần base64 chứa thông tin user
                string sign = arrToken[1];      // Phần signature để xác thực

                // Tính toán signature mong đợi dựa trên base64 object và mã passcode
                string expectedSign = Utility.GetSHA256(obj + AppConstants.HCA_PASSCODE + DateTime.Now.ToString("yyyyMM"));

                // So sánh signature nhận được với signature mong đợi
                if (sign != expectedSign)
                {
                    context.Result = new JsonResult(new ResponseModel("TokenIsNotCorrect", ""));
                    return;
                }

                // Giải mã base64 thành JSON string
                string jsonToken = Utility.Base64Decode(obj);

                // Chuyển đổi JSON thành object UserDKKBToken
                var objToken = JsonConvert.DeserializeObject<UserHCAToken>(jsonToken);

                // Kiểm tra object có được tạo thành công không
                if (objToken == null)
                {
                    context.Result = new JsonResult(new ResponseModel("TokenIsNotCorrect", ""));
                    return;
                }

                // Gán token object vào controller để sử dụng sau này
                var apiController = controller as ApiControllerBase;
                if (apiController != null)
                {
                    apiController.objToken = objToken;
                }
            }
            catch (Exception ex)
            {
                // Xử lý mọi exception và trả về lỗi token không hợp lệ
                context.Result = new JsonResult(new ResponseModel("TokenIsNotCorrect", ""));
            }
            base.OnActionExecuting(context);
        }
    }
}