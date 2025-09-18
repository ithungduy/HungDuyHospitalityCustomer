using HospitalityCustomerAPI.DTO.News;
using HospitalityCustomerAPI.Models;

namespace HospitalityCustomerAPI.Repositories.IRepositories
{
    public interface INewsRepository
    {
        ResponseModel GetTinTuc();
        ResponseModel GetTinTucTheoLoai(Guid maLoai);
        ResponseModel GetVideoAds();
        ResponseModel GetTinTucNoiBatHome();

        Task<ResponseModel<PagedResult<NewsItemDto>>> GetNewsAsync(Guid? maLoai, int page, int pageSize, CancellationToken ct);
        Task<ResponseModel<IReadOnlyList<NewsItemDto>>> GetHighlightedAsync(CancellationToken ct);
        Task<ResponseModel<IReadOnlyList<VideoAdsDto>>> GetVideosAsync(CancellationToken ct);
    }
}
