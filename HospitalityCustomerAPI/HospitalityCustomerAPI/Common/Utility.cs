using HospitalityCustomerAPI.Models.HCAEntity;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HospitalityCustomerAPI.Common
{
    public class Utility
    {
        private static Random random = new Random();
        public static Guid defaultUID = new Guid();

        public static string GetSHA512(string str)
        {
            return GetSHA(str, SHA512.Create());
        }
        public static string GetSHA256(string str)
        {
            return GetSHA(str, SHA256.Create());
        }
        private static string GetSHA(string str, HashAlgorithm hash)
        {
            try
            {
                string[] value = (from x in hash.ComputeHash(Encoding.UTF8.GetBytes(str))
                                  select x.ToString("X2")).ToArray<string>();
                return string.Join("", value).ToUpper();
            }
            catch
            {
            }
            return "";
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string RandomNumber(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string BoDauTiengViet(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = (s + "").Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public static bool IsNull(object o)
        {
            return o == null || Convert.IsDBNull(o);
        }

       
        public static Guid GetGuid(string str)
        {
            try
            {
                return Guid.Parse(str);
            }
            catch
            {
                return Utility.defaultUID;
            }
        }
        public static string ConvertPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length >= 10 && phoneNumber.Substring(0, 2) == "84")
            {
                return "0" + phoneNumber.Substring(2);
            }
            else
            {
                return phoneNumber;
            }
        }
        public static string GetCodeDiaChi(string ten)
        {
            ten = BoDauTiengViet(ten);
            var Reg = new Regex(@"^[0-9a-zA-Z/-]");
            try
            {
                string[] tmps = ten.Split(' ');
                string code = "";
                for (int i = 0; i < tmps.Length; i++)
                {
                    if (tmps[i].Trim() != "" && Reg.IsMatch(tmps[i].Trim()))
                    {
                        code += tmps[i].Trim()[0];
                    }
                }
                return code;
            }
            catch
            {
                return "";
            }
        }

        public static (DateTime TuNgay, DateTime DenNgay) GetDateRangeOfWeek(int weekNumber, int year)
        {
            // Lấy ngày 1/1 của năm
            DateTime firstDayOfYear = new DateTime(year, 1, 1);

            // Tính số ngày cần lùi/tiến để đến thứ Hai (giống logic JavaScript)
            // JavaScript: day === 0 ? -6 : 1 - day
            int day = (int)firstDayOfYear.DayOfWeek; // 0=CN, 1=T2, ..., 6=T7
            int daysOffset = (day == 0) ? -6 : (1 - day);

            // Ngày bắt đầu của tuần 1 (thứ Hai đầu tiên, có thể thuộc năm trước)
            DateTime firstMonday = firstDayOfYear.AddDays(daysOffset);

            // Tuần cần tìm: (weekNumber - 1) tuần sau tuần 1
            DateTime tuNgay = firstMonday.AddDays((weekNumber - 1) * 7);

            // Ngày kết thúc là Chủ nhật
            DateTime denNgay = tuNgay.AddDays(6);

            return (tuNgay, denNgay);
        }

    }
}
