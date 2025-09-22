
using HISCustomerAPI.Common;
using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.Controllers;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace HRMUngTuyen.Controllers
{
    [Route("[controller]")]
    public class DiaChiController : ApiControllerBase
    {
        private readonly ITinhThanhRepository _tinhThanhRepository;
        private readonly IPhuongXaRepository _phuongXaRepository;
        private readonly IQuocGiaRepository _quocGiaRepository;

        public DiaChiController(HungDuyHospitalityCustomerContext hungDuyHospitalityCustomerContext,
            ITinhThanhRepository tinhThanhRepository,
            IPhuongXaRepository phuongXaRepository,
            IQuocGiaRepository quocGiaRepository) : base(hungDuyHospitalityCustomerContext)
        {
            _tinhThanhRepository = tinhThanhRepository;
            _phuongXaRepository = phuongXaRepository;
            _quocGiaRepository = quocGiaRepository;
        }


        [HttpGet("quocgia")]
        [APIKeyCheck]
        public async Task<ResponseModel> GetQuocGiaList()
        {
            var quocGias = await _quocGiaRepository.GetAllAsync();
            var result = quocGias.OrderBy(q => q.Ten)
                .Select(q => new { MaQuocGia = q.Ma, TenQuocGia = q.Ten });
            return new ResponseModelSuccess("Lấy quốc gia thành công", result);
        }


        [HttpGet("tinhthanh")]
        [APIKeyCheck]
        public async Task<ResponseModel> GetTinhThanhList([FromQuery] string maQuocGia)
        {
            if (string.IsNullOrEmpty(maQuocGia))
            {
                return new ResponseModelSuccess("",new List<object>());
            }
            var tinhThanhs = await _tinhThanhRepository.GetByQuocGiaAsync(maQuocGia);
            var result = tinhThanhs.OrderBy(t => t.Ten).Select(t => new { MaTinhThanh = t.Ma, TenTinhThanh = t.Ten });
            return new ResponseModelSuccess("Lấy tỉnh thành thành công", result);
        }

        [HttpGet("phuongxa")]
        [APIKeyCheck]
        public async Task<ResponseModel> GetPhuongXaListByTinhThanh([FromQuery] string maTinhThanh)
        {
            if (string.IsNullOrEmpty(maTinhThanh))
            {
                return new ResponseModelSuccess("", new List<object>());
            }
            var phuongXas = await _phuongXaRepository.GetByTinhThanhAsync(maTinhThanh);
            var result = phuongXas.OrderBy(p => p.Ten).Select(p => new { MaPhuongXa = p.Ma, TenPhuongXa = p.Ten });
            return new ResponseModelSuccess("Lấy phường xã thành công", result);
        }
    }
}
