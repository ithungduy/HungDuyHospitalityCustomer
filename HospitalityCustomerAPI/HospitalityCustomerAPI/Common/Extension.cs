using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace HospitalityCustomerAPI.Common
{
    public static class Exten
    {      

        public static bool IsEmpty(this string? str)
        {
            return str == null || str == "";
        }

        public static Guid? GetGuidNull(this string? str)
        {
            try
            {
                return str.IsEmpty() ? null : Guid.Parse(str);
            }
            catch
            {
                return Utility.defaultUID;
            }
        }
        public static Guid GetGuid(this string? str)
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
        public static bool IsNullOrEmpty(this Guid? str)
        {
            try
            {
                return str == null || str == Utility.defaultUID;
            }
            catch
            {
                return true;
            }
        }
        public static bool IsEmpty(this Guid str)
        {
            try
            {
                return str == Utility.defaultUID;
            }
            catch
            {
                return true;
            }
        }
        public static bool IsNotEmpty(this Guid? str)
        {
            return !str.IsNullOrEmpty();
        }
        public static bool IsNotEmpty(this Guid str)
        {
            return !str.IsEmpty();
        }
        public static Guid DefaultIfNull(this Guid? str)
        {
            return str ?? Utility.defaultUID;
        }
        public static DateTime ParseDate(this string? str)
        {
            try
            {
                return DateTime.ParseExact(str, str.Contains("-") ? "yyyy-MM-dd" : "dd/MM/yyyy", null);
            }
            catch
            {
                return DateTime.Now;
            }
        }
        public delegate T CallBack<T>();
        public delegate void CallBackData<T>(List<T> b);
        public static T? DeepCopy<T>(this T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
        public static string SerializeChild<T>(this T value)
        {
            return Serialize(value, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
        }
        public static string Serialize<T>(this T value, XmlSerializerNamespaces? namespaces = null)
        {
            if (value == null) return string.Empty;
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var stringWriter = new Utf8StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = namespaces != null }))
                {
                    xmlSerializer.Serialize(xmlWriter, value, namespaces);
                    return stringWriter.ToString();
                }
            }
        }
      

        private static MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(10));
        private static List<string> _key = new List<string>();
        public static T GetValueObject<T>(this IMemoryCache _memoryCache, string key, CallBack<T> callBack, MemoryCacheEntryOptions? option = null)
        {
            if (!_memoryCache.TryGetValue(key, out T? obj))
            {
                obj = callBack();
                _memoryCache.Set(key, obj, option ?? cacheEntryOptions);
                if (!_key.Any(t => t == key))
                {
                    _key.Add(key);
                }
            }
            return obj ?? Activator.CreateInstance<T>();
        }


        public static T GetValue<T>(this IMemoryCache _memoryCache, string key, CallBack<T> callBack, MemoryCacheEntryOptions? option = null)
        {
            return callBack();
            if (!_memoryCache.TryGetValue(key, out T? obj))
            {
                obj = callBack();
                _memoryCache.Set(key, obj, option ?? cacheEntryOptions);
                if (!_key.Any(t => t == key))
                {
                    _key.Add(key);
                }
            }
            return obj.DeepCopy();
        }
        public delegate Task<T> CallBackAsync<T>();

        public static async Task<T> GetValueAsync<T>(this IMemoryCache _memoryCache, string key, CallBackAsync<T> callBack, MemoryCacheEntryOptions? option = null)
        {
            return await callBack();
            if (!_memoryCache.TryGetValue(key, out T? obj))
            {
                obj = await callBack();
                _memoryCache.Set(key, obj, option ?? cacheEntryOptions);
                if (!_key.Any(t => t == key))
                {
                    _key.Add(key);
                }
            }
            return obj.DeepCopy();
        }
        public static void Clear(this IMemoryCache _memoryCache, Func<string, bool>? predicate)
        {
            List<string> keyRemove = new List<string>();
            foreach (var k in predicate != null ? _key.Where(predicate) : _key)
            {
                try
                {
                    keyRemove.Add(k);
                    _memoryCache.Remove(k);
                }
                catch { }
            }
            foreach (var key in keyRemove)
            {
                _key.Remove(key);
            }
        }
    }
}
