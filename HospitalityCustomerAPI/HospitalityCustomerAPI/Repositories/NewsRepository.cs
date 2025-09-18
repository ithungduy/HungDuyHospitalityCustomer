using HospitalityCustomerAPI.DTO.News;
using HospitalityCustomerAPI.Models;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Repositories
{
    public class NewsRepository : INewsRepository
    {
        private readonly HungDuyHospitalityCustomerContext _context;

        public NewsRepository(HungDuyHospitalityCustomerContext context)
        {
            _context = context;
        }

        public ResponseModel GetTinTuc()
        {
            var result = (from x in _context.Set<NwsTinTuc>()
                          join loaitt in _context.Set<NwsLoaiTinTuc>()
                          on x.MaLoai equals loaitt.Ma
                          where (x.Deleted != true) && (loaitt.Deleted != true)
                                && (x.Status == true) && (loaitt.Status == true)
                          select new
                          {
                              ma = x.Ma,
                              tenLoai = loaitt.Ten + "",
                              ten = x.Title + "",
                              loai = x.MaLoai,
                              mota = x.MoTaNgan + "",
                              url = x.Link + "",
                              ngaytao  =x.CreatedDate
                          })
                          .OrderByDescending(item => item.ngaytao)
                          .ToList();

            return new ResponseModelSuccess("", result);
        }

        public ResponseModel GetTinTucTheoLoai(Guid maLoai)
        {
            var result = (from x in _context.Set<NwsTinTuc>()
                          join loaitt in _context.Set<NwsLoaiTinTuc>()
                          on x.MaLoai equals loaitt.Ma
                          where (x.Deleted != true) && (loaitt.Deleted != true)
                                && (x.Status == true) && (loaitt.Status == true)
                                && x.MaLoai == maLoai
                          select new
                          {
                              ma = x.Ma,
                              hinhAnh = x.HinhAnh + "",
                              tenLoai = loaitt.Ten + "",
                              ten = x.Title + "",
                              loai = x.MaLoai,
                              mota = x.MoTaNgan + "",
                              url = x.Link + "",
                              ngaytao = x.CreatedDate,
                          })
                          .OrderByDescending(item => item.ngaytao)
                          .ToList();

            return new ResponseModelSuccess("", result);
        }

        public ResponseModel GetVideoAds()
        {
            var result = _context.NwsVideoAds
                .Where(t => !(t.Deleted ?? false) && (t.Status ?? false))
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new
                {
                    ma = x.Ma + "",
                    ten = x.Ten + "",
                    link = x.Link + "",
                    linkweb = x.LinkWeb + "",
                    thumbnail = x.Thumbnail + "",
                    status = x.Status + "",
                }).ToList();

            return new ResponseModelSuccess("", result);
        }

        public ResponseModel GetTinTucNoiBatHome()
        {
            var result = (from x in _context.Set<NwsTinTuc>()
                          join loaitt in _context.Set<NwsLoaiTinTuc>()
                          on x.MaLoai equals loaitt.Ma
                          where (x.Deleted != true) && (loaitt.Deleted != true)
                                && (x.Status == true) && (loaitt.Status == true)
                                && (x.TinNoiBat == true)
                          select new
                          {
                              ma = x.Ma,
                              hinhAnh = x.HinhAnh + "",
                              tenLoai = loaitt.Ten + "",
                              ten = x.Title + "",
                              loai = x.MaLoai,
                              mota = x.MoTaNgan + "",
                              url = x.Link + "",
                              ngaytao = x.CreatedDate
                          })
                          .OrderByDescending(item => item.ngaytao)
                          .ToList();

            return new ResponseModelSuccess("", result);
        }

        public async Task<ResponseModel<PagedResult<NewsItemDto>>> GetNewsAsync(Guid? maLoai, int page, int pageSize, CancellationToken ct)
        {
            var q = from x in _context.Set<NwsTinTuc>().AsNoTracking()
                    join loai in _context.Set<NwsLoaiTinTuc>().AsNoTracking()
                        on x.MaLoai equals loai.Ma
                    where (x.Deleted != true) && (loai.Deleted != true)
                          && (x.Status == true) && (loai.Status == true)
                    select new NewsItemDto
                    {
                        Ma = x.Ma,
                        MaLoai = x.MaLoai,
                        TenLoai = loai.Ten ?? "",
                        TieuDe = x.Title ?? "",
                        MoTaNgan = x.MoTaNgan ?? "",
                        HinhAnh = x.HinhAnh,
                        Link = x.Link,
                        NgayTao = x.CreatedDate,
                    };

            if (maLoai.HasValue) q = q.Where(t => t.MaLoai == maLoai.Value);

            var total = await q.CountAsync(ct);
            var items = await q.OrderByDescending(t => t.NgayTao)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(ct);

            var payload = new PagedResult<NewsItemDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
            return ResponseModel<PagedResult<NewsItemDto>>.Ok(payload);
        }

        public async Task<ResponseModel<IReadOnlyList<NewsItemDto>>> GetHighlightedAsync(CancellationToken ct)
        {
            var data = await (from x in _context.Set<NwsTinTuc>().AsNoTracking()
                              join loai in _context.Set<NwsLoaiTinTuc>().AsNoTracking()
                                  on x.MaLoai equals loai.Ma
                              where (x.Deleted != true) && (loai.Deleted != true)
                                    && (x.Status == true) && (loai.Status == true)
                                    && (x.TinNoiBat == true)
                              orderby x.CreatedDate descending
                              select new NewsItemDto
                              {
                                  Ma = x.Ma,
                                  MaLoai = x.MaLoai,
                                  TenLoai = loai.Ten ?? "",
                                  TieuDe = x.Title ?? "",
                                  MoTaNgan = x.MoTaNgan ?? "",
                                  HinhAnh = x.HinhAnh,
                                  Link = x.Link
                              })
                             .Take(5)
                             .ToListAsync(ct);

            return ResponseModel<IReadOnlyList<NewsItemDto>>.Ok(data);
        }

        public async Task<ResponseModel<IReadOnlyList<VideoAdsDto>>> GetVideosAsync(CancellationToken ct)
        {
            var data = await _context.NwsVideoAds.AsNoTracking()
                .Where(t => !(t.Deleted ?? false) && (t.Status ?? false))
                .OrderByDescending(x => x.CreatedDate)
                .Take(5)
                .Select(x => new VideoAdsDto
                {
                    Ma = x.Ma,
                    Ten = x.Ten ?? "",
                    Link = x.Link,
                    LinkWeb = x.LinkWeb,
                    Thumbnail = x.Thumbnail,
                    Status = x.Status ?? false
                })
                .ToListAsync(ct);

            return ResponseModel<IReadOnlyList<VideoAdsDto>>.Ok(data);
        }
    }
}
