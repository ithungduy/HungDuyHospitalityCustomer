using Newtonsoft.Json;

namespace HospitalityCustomerAPI.Models
{
    public class ResponseModel
    {
        // ? Status dùng để phân biệt response thành công hay thất bại
        public string? Status { get; set; }

        // ? Message chứa thông báo mô tả kết quả trả về
        public string? Messenge { get; set; }

        // ? Data chứa dữ liệu trả về dưới dạng chuỗi JSON
        public string? Data { get; set; }

        public ResponseModel() { }

        // * Constructor đầy đủ nhận vào status, message và data
        // @param status: Trạng thái của response ("OK" hoặc "Error") 
        // @param messenge: Thông báo mô tả
        // @param data: Dữ liệu trả về (sẽ được convert sang JSON)
        public ResponseModel(string status, string messenge, Object data)
        {
            Status = status;
            Messenge = messenge;
            // ! Data được tự động convert sang JSON string
            Data = JsonConvert.SerializeObject(data);
        }

        // * Constructor chỉ nhận status và message
        public ResponseModel(string status, string messenge)
        {
            Status = status;
            Messenge = messenge;
            Data = null;
        }
    }
    public class ResponseModelSuccess : ResponseModel
    {
        public ResponseModelSuccess()
        {
        }

        // * Constructor tự động set Status = "OK"
        // @param messenge: Thông báo thành công
        // @param data: Dữ liệu trả về (optional)
        public ResponseModelSuccess(string messenge, Object? data = null)
        {
            Status = "OK";
            Messenge = messenge;
            Data = JsonConvert.SerializeObject(data);
        }
    }

    // * Class con dùng cho các response lỗi
    public class ResponseModelError : ResponseModel
    {
        public ResponseModelError()
        {
        }

        // * Constructor tự động set Status = "Error" 
        // @param messenge: Thông báo lỗi
        // @param data: Dữ liệu bổ sung về lỗi (optional)
        public ResponseModelError(string messenge, Object? data = null)
        {
            Status = "Error";
            Messenge = messenge;
            Data = JsonConvert.SerializeObject(data);
        }
    }
}
