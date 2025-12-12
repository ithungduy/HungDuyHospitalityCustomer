using System.Globalization;
using System.Reflection;

namespace HospitalityCustomerAPI.Common
{
    public static class Cast
    {
        public static string ToString(object? o)
        {
            return (o ?? "").ToString() ?? "";
        }

        public static byte ToByte(object? o)
        {
            byte result = 0;
            if (!Utility.IsNull(o))
            {
                byte.TryParse(o.ToString(), out result);
            }
            return result;
        }

        public static int ToInt(object? o)
        {
            return (int)ToDouble(o);
        }
        public static long ToLong(object? o)
        {
            return (long)ToDouble(o);
        }
        public static float ToFloat(object? o)
        {
            float result = 0f;
            if (!Utility.IsNull(o))
            {
                float.TryParse(o.ToString(), out result);
            }
            return result;
        }

        public static double ToDouble(object? o)
        {
            double result = 0.0;
            if (!Utility.IsNull(o))
            {
                double.TryParse(o.ToString(), out result);
            }
            return result;
        }

        public static decimal ToDecimal(object? o)
        {
            decimal result = 0m;
            if (!Utility.IsNull(o))
            {
                decimal.TryParse(o.ToString(), out result);
            }
            return result;
        }

        public static bool ToBoolean(object? o)
        {
            bool result = false;
            if (!Utility.IsNull(o))
            {
                bool.TryParse(o.ToString(), out result);
            }
            return result;
        }

        public static DateTime ToDateTime(object? o)
        {
            DateTime result = new DateTime(2000, 1, 1);
            if (!Utility.IsNull(o))
            {
                DateTime.TryParse(o.ToString(), out result);
            }
            return result;
        }

        public static DateTime ToDateTime(string dateTime, string Format)
        {
            DateTime result = new DateTime(2000, 1, 1);
            try
            {
                if (!Utility.IsNull(dateTime))
                {
                    return DateTime.ParseExact(dateTime, Format, new CultureInfo("en-US"), DateTimeStyles.None);
                }
            }
            catch
            {
            }
            return result;
        }

        public static TimeSpan ToTimeSpan(object o)
        {
            TimeSpan result = new TimeSpan(0L);
            if (!Utility.IsNull(o))
            {
                TimeSpan.TryParse(o.ToString(), out result);
            }
            return result;
        }

        public static object Nz(object o, object oReplace)
        {
            object result;
            if (!Utility.IsNull(o))
            {
                result = o;
            }
            else
            {
                result = oReplace;
            }
            return result;
        }

        public static object TryConvert(PropertyInfo pro, object o)
        {
            string text = pro.PropertyType.ToString();
            object result;
            if (text.Contains("Decimal"))
            {
                result = ToDecimal(o);
            }
            else if (text.Contains("Double"))
            {
                result = ToDouble(o);
            }
            else if (text.Contains("Int32"))
            {
                result = ToInt(o);
            }
            else if (text.Contains("Boolean"))
            {
                result = ToBoolean(o);
            }
            else if (text.Contains("DateTime"))
            {
                result = ToDateTime(o);
            }
            else if (text.Contains("String"))
            {
                result = Nz(o, "").ToString() ?? "";
            }
            else
            {
                result = o;
            }
            return result;
        }

        public static short ToShort(object o)
        {
            return (short)ToDouble(o);
        }
    }

    public static class CastExtend
    {
        public static string ToString(this object o)
        {
            return Cast.ToString(o);
        }

        public static byte ToByte(this object o)
        {
            return Cast.ToByte(o);
        }

        public static int ToInt(this object o)
        {
            return Cast.ToInt(o);
        }
        public static long ToLong(this object o)
        {
            return Cast.ToLong(o);
        }
        public static float ToFloat(this object o)
        {
            return Cast.ToFloat(o);
        }

        public static double ToDouble(this object o)
        {
            return Cast.ToDouble(o);
        }

        public static decimal ToDecimal(this object o)
        {
            return Cast.ToDecimal(o);
        }

        public static bool ToBoolean(this object o)
        {
            return Cast.ToBoolean(o);
        }

        public static DateTime ToDateTime(this object o)
        {
            return Cast.ToDateTime(o);
        }
        public static DateTime? ToDateTime2(this object o, DateTime? df = null)
        {
            string tmp = (o + "");
            if (tmp.IsEmpty())
            {
                return df;
            }
            try
            {
                DateTime x;
                if (tmp.Contains("T") || tmp.Contains(" "))//Datetime
                {
                    if (DateTime.TryParseExact(tmp, tmp.Contains("-") ? "yyyy-MM-ddTHH:mm:ss" : "dd/MM/yyyy HH:mm:ss", null, DateTimeStyles.None, out x))
                    {
                        return x;
                    }
                    if (DateTime.TryParseExact(tmp, tmp.Contains("-") ? "yyyy-MM-ddThh:mm:ss" : "dd/MM/yyyy hh:mm:ss", null, DateTimeStyles.None, out x))
                    {
                        return x;
                    }
                    if (DateTime.TryParseExact(tmp, tmp.Contains("-") ? "yyyy-MM-ddTHH:mm" : "dd/MM/yyyyTHH:mm", null, DateTimeStyles.None, out x))
                    {
                        return x;
                    }
                    if (DateTime.TryParseExact(tmp, tmp.Contains("-") ? "yyyy-MM-dd HH:mm" : "dd/MM/yyyy HH:mm", null, DateTimeStyles.None, out x))
                    {
                        return x;
                    }
                    if (DateTime.TryParseExact(tmp, tmp.Contains("-") ? "yyyy-MM-dd hh:mm:ss tt" : "dd/MM/yyyy hh:mm:ss tt", null, DateTimeStyles.None, out x))
                    {
                        return x;
                    }
                    return df;
                }
                else//Date
                {
                    if (tmp.Contains("-") || tmp.Contains("/"))
                    {
                        if (DateTime.TryParseExact(tmp, tmp.Contains("-") ? "yyyy-MM-dd" : "dd/MM/yyyy", null, DateTimeStyles.None, out x))
                        {
                            return x;
                        }
                        return df;
                    }
                    else
                    {
                        if (Cast.ToInt(tmp) > 100000)//yyyyMd hoặc dMyyyy hoặc yyyyMMdd hoặc ddMMyyyy
                        {
                            if (tmp.Length == 8) //yyyyMMdd hoặc ddMMyyyy
                            {
                                //coming soon
                            }
                        }
                        else if (Cast.ToInt(tmp) > 1900)//yyyy
                        {
                            return new DateTime(Cast.ToInt(o + ""), 1, 1);
                        }
                    }
                }
            }
            catch { }
            return df;
        }

        public static bool IsToday(this DateTime? dateTime)
        {
            var now = DateTime.Now;
            return dateTime?.Day == now.Day && dateTime?.Month == now.Month && dateTime?.Year == now.Year;
        }

        public static DateTime ToDateTime(this string dateTime, string Format)
        {
            return Cast.ToDateTime(dateTime, Format);
        }

        public static TimeSpan ToTimeSpan(this object o)
        {
            return Cast.ToTimeSpan(o);
        }

        public static object Nz(this object o, object oReplace)
        {
            return Cast.Nz(o, oReplace);
        }

        public static object TryConvert(this PropertyInfo pro, object o)
        {
            return Cast.TryConvert(pro, o);
        }

        public static short ToShort(object o)
        {
            return Cast.ToShort(o);
        }
    }
}
