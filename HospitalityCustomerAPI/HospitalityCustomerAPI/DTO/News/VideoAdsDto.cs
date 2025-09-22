namespace HospitalityCustomerAPI.DTO.News
{
    public sealed class NewsItemDto
    {
        public Guid Ma { get; set; }
        public Guid? MaLoai { get; set; }
        public string TenLoai { get; set; } = "";
        public string TieuDe { get; set; } = "";
        public string MoTaNgan { get; set; } = "";
        public string? HinhAnh { get; set; }
        public string? Link { get; set; }
        public DateTime? NgayTao { get; set; }
    }

    public sealed class VideoAdsDto
    {
        public Guid Ma { get; set; }
        public string Ten { get; set; } = "";
        public string? Link { get; set; }
        public string? LinkWeb { get; set; }
        public string? Thumbnail { get; set; }
        public bool Status { get; set; }
        public DateTime? NgayTao { get; set; }


    }

    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Total { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }

    public class ResponseModel<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        public T? Data { get; init; }

        public static ResponseModel<T> Ok(T data, string msg = "") => new() { Success = true, Data = data, Message = msg };
        public static ResponseModel<T> Fail(string msg) => new() { Success = false, Message = msg };
    }
}
