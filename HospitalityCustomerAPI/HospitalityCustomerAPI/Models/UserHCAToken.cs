using Newtonsoft.Json.Linq;

namespace HospitalityCustomerAPI.Models
{
    public class UserHCAToken
    {
        /// <summary>
        /// ID của người dùng
        /// </summary>
        public Guid userid { get; set; }

        /// <summary>
        /// Mã định danh của người dùng
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// ID phụ dạng Guid, có thể null
        /// </summary>
        public Guid? idAs { get; set; }

        /// <summary>
        /// Constructor mặc định, cần thiết cho JSON deserialization
        /// </summary>
        public UserHCAToken()
        {
            id = string.Empty;
        }

        /// <summary>
        /// Constructor với các tham số cơ bản
        /// </summary>
        /// <param name="userid">ID của người dùng</param>
        /// <param name="id">Mã định danh</param>
        public UserHCAToken(Guid userid, string id)
        {
            this.userid = userid;
            this.id = id;
        }

        /// <summary>
        /// Constructor từ JObject, được sử dụng khi parse từ JSON động
        /// </summary>
        /// <param name="obj">JObject chứa dữ liệu user</param>
        public UserHCAToken(JObject obj)
        {
            userid = (Guid)(obj["userid"] ?? 0);
            id = obj["id"] is not null ? (string)obj["id"]! : "";
        }
    }
}
