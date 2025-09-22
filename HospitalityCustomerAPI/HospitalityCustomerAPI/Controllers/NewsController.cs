using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.DTO.News;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Controllers
{
    [Route("[controller]")]
    public class NewsController : ApiControllerBase
    {
        private readonly INewsRepository _newsRepository;

        public NewsController(HungDuyHospitalityCustomerContext hungDuyHospitalityCustomerContext, INewsRepository newsRepository) : base(hungDuyHospitalityCustomerContext)
        {
            _newsRepository = newsRepository;
        }

        [HttpPost("GetTinTuc")]
        [APIKeyCheck]
        public ResponseModel GetTinTuc() => _newsRepository.GetTinTuc();

        [HttpPost("GetTinTucTongQuat")]
        [APIKeyCheck]
        public ResponseModel GetTinTucTongQuat() => _newsRepository.GetTinTucTheoLoai(TinTucEnum.TinTuc.GetEnumGuid());

        [HttpPost("GetTinTucNoiBat")]
        [APIKeyCheck]
        public ResponseModel GetTinTucNoiBat() => _newsRepository.GetTinTucTheoLoai(TinTucEnum.NoiBat.GetEnumGuid());

        [HttpPost("GetTinTucTheoLoai")]
        [APIKeyCheck]
        public ResponseModel GetTinTucTheoLoai([FromForm] Guid maLoai) => _newsRepository.GetTinTucTheoLoai(maLoai);

        [HttpPost("GetVideoAds")]
        [APIKeyCheck]
        public ResponseModel GetVideoAds() => _newsRepository.GetVideoAds();

        [HttpPost("GetTinTucNoiBatHome")]
        [APIKeyCheck]
        public ResponseModel GetTinTucNoiBatHome() => _newsRepository.GetTinTucNoiBatHome();

        [HttpGet("/news")]
        [APIKeyCheck]
        public Task<ResponseModel<PagedResult<NewsItemDto>>> GetNews([FromQuery] Guid? maLoai, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
            => _newsRepository.GetNewsAsync(maLoai, page, pageSize, ct);

        [HttpGet("/news/highlighted")]
        [APIKeyCheck]
        public Task<ResponseModel<IReadOnlyList<NewsItemDto>>> GetHighlighted(CancellationToken ct = default)
            => _newsRepository.GetHighlightedAsync(ct);

        [HttpGet("/news/videos")]
        [APIKeyCheck]
        public Task<ResponseModel<PagedResult<VideoAdsDto>>> GetVideos([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
            => _newsRepository.GetVideosAsync(page, pageSize, ct);
    }
}
