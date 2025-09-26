using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace HospitalityCustomerAPI.Common
{
    public static class SMSController
    {
        // Endpoint theo phong cách HungDuyHospitality
        private static readonly string smsURLPublic = "http://123.31.36.151:8888/smsbn/api";

        // Chuẩn tên hàm giống HungDuyHospitality
        public static async Task<bool> sendOTP(string phone, string otp, string time)
            => await sendSMSVNPT(phone, "1377251", otp, time);

        public static async Task<bool> sendThongBaoNo(string phone, string khachhang, string nganhhang, string sotienno, string sotienthu)
        {
            // Định dạng .0 như bản gốc (và sửa check đúng biến)
            var noFmt = sotienno.Contains(".") ? sotienno : (sotienno + ".0");
            var thuFmt = sotienthu.Contains(".") ? sotienthu : (sotienthu + ".0");
            return await sendSMSVNPT(phone, "1380291", khachhang, nganhhang, noFmt, thuFmt);
        }

        // Bản trả về raw JSON để test/debug nội dung gửi (giống HungDuyHospitality)
        public static async Task<string> sendThongBaoNoTest(string phone, string khachhang, string nganhhang, string sotienno, string sotienthu)
        {
            var noFmt = sotienno.Contains(".") ? sotienno : (sotienno + ".0");
            var thuFmt = sotienthu.Contains(".") ? sotienthu : (sotienthu + ".0");
            return await sms_response(phone, "1380291", khachhang, nganhhang, noFmt, thuFmt);
        }

        // ======================= Core helpers (giống HungDuyHospitality) =======================

        private static async Task<bool> sendSMSVNPT(string phone, string TEMPLATEID, params string[] arg)
        {
            phone = NormalizePhone(phone);
            if (phone.Length != 11) return false;

            JObject obj = JObject.Parse(await sms_response(phone, TEMPLATEID, arg));
            return (string?)obj["RPLY"]?["ERROR"] == "0";
        }

        private static async Task<string> sms_response(string phone, string TEMPLATEID, params string[] arg)
        {
            return await sms_responses(new List<string> { phone }, TEMPLATEID, arg);
        }

        private static async Task<string> sms_responses(List<string> phones, string TEMPLATEID, params string[] arg)
        {
            // Payload đồng bộ cấu trúc với HungDuyHospitality (LABELID/CONTRACT/USER...)
            var payload = new
            {
                RQST = new
                {
                    name = "send_sms_list",
                    REQID = Utility.RandomString(5),
                    LABELID = "205840",
                    CONTRACTTYPEID = "1",
                    CONTRACTID = "16025",
                    TEMPLATEID,
                    // SỬA: dùng index để tạo NUM, tránh IndexOf gây sai khi tham số trùng nhau
                    PARAMS = arg.Select((t, i) => new
                    {
                        NUM = (i + 1).ToString(),
                        CONTENT = Utility.BoDauTiengViet(t)
                    }),
                    SCHEDULETIME = "",
                    MOBILELIST = phones.Count == 1 ? phones[0] : string.Join(",", phones),
                    ISTELCOSUB = "0",
                    AGENTID = "181",
                    APIUSER = "Ctyhungduy",
                    APIPASS = "Vnpt@123",
                    USERNAME = "Ctyhungduy",
                    // DATACODING = "8"
                }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"),
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var client = new HttpClient { BaseAddress = new Uri(smsURLPublic) };
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "";
            phone = phone.Trim().Replace(" ", "");
            if (phone.StartsWith("+84")) phone = phone.Substring(1);
            else if (phone.StartsWith("0")) phone = "84" + phone.Substring(1);
            return phone;
        }
    }
}
