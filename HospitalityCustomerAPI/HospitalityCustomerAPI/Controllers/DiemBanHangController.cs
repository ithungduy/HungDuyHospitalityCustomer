using HISCustomerAPI.Common;
using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.DTO.DiemBanHang;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace HospitalityCustomerAPI.Controllers
{
    [Route("[controller]")]
    public class DiemBanHangController : ApiControllerBase
    {
        private readonly IDiemBanHangPOSRepository _diemBanHangPOSRepository;

        public DiemBanHangController(HungDuyHospitalityCustomerContext hungDuyHospitalityCustomerContext, 
            IDiemBanHangPOSRepository diemBanHangPOSRepository) : base(hungDuyHospitalityCustomerContext)
        {
            _diemBanHangPOSRepository = diemBanHangPOSRepository;
        }

        [HttpPost("GetDiemBanHangByID")]
        [TokenUserCheckHTTP]
        public ResponseModel GetDiemBanHangByID([FromForm] Guid MaDiemBanHang)
        {
            var result = _diemBanHangPOSRepository.GetTenById(MaDiemBanHang);

            if (result == null)
                return new ResponseModelError("Không tìm thấy điểm bán hàng");

            return new ResponseModelSuccess("", result);
        }
    }
}
