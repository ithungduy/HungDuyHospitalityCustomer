using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace HospitalityCustomerAPI.Controllers
{
    public class ApiControllerBase : ControllerBase
    {
        protected readonly HungDuyHospitalityCustomerContext _hungDuyHospitalityCustomerContext;

        public UserHCAToken? objToken { get; set; }

        protected static readonly ResponseModel ResponseNotExist = new ResponseModelError("Đối tượng không tồn tại");
        protected static readonly ResponseModel ResponseHaveError = new ResponseModelError("Đã xảy ra lỗi");
        protected static readonly ResponseModel ResponseAddFailure = new ResponseModelError("Thêm thất bại");
        protected static readonly ResponseModel ResponseUpdateSuccessfully = new ResponseModelSuccess("Cập nhật thành công");
        protected static readonly ResponseModel ResponseUpdateFailure = new ResponseModelError("Cập nhật thất bại");
        protected static readonly ResponseModel ResponseSuccessfully = new ResponseModelSuccess("Thành công");
        protected static readonly ResponseModel ResponseDateFormatIsIncorrect = new ResponseModelError("Định dạng ngày không đúng");
        protected static readonly ResponseModel ResponseAddSuccessfully = new ResponseModelSuccess("Thêm thành công");
        protected static readonly ResponseModel ResponseObjectExists = new ResponseModelError("Đối tượng đã tồn tại");
        protected static readonly ResponseModel ResponseRegisterSuccessfully = new ResponseModelSuccess("Đăng kí thành công");

        public ApiControllerBase(HungDuyHospitalityCustomerContext hungDuyHospitalityCustomerContext)
        {
            _hungDuyHospitalityCustomerContext = hungDuyHospitalityCustomerContext;
        }
        
        protected void AttachCountryCodeForPhoneNumber(in string phoneNumber, out string phoneNumberAttach)
        {
            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.StartsWith("84"))
            {
                phoneNumberAttach = "0" + phoneNumber.Substring(2);
            }
            else
            {
                phoneNumberAttach = phoneNumber ?? "";
            }
        }
    }
}
