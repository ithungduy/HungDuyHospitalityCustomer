using HospitalityCustomerAPI.Common.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HospitalityCustomerAPI.Common
{
    public static class EnumExtend
    {
        public static Guid GetEnumGuid(this Enum e)
        {
            MemberInfo[] memInfo = e.GetType().GetMember(e.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(EnumGuidAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((EnumGuidAttribute)attrs[0]).Guid;
            }
            throw new ArgumentException("Enum " + e.ToString() + " chưa được khởi tạo!");
        }

        public static TEnum ToEnum<TEnum>(this string value, bool ignoreCase = true, TEnum defaultValue = default) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (Enum.TryParse<TEnum>(value, ignoreCase, out var result))
                return result;

            return defaultValue;
        }

        public static string GetDisplayName(this Enum value)
        {
            return value.GetType()
                        .GetMember(value.ToString())[0]
                        .GetCustomAttribute<DisplayAttribute>()?
                        .GetName() ?? value.ToString();
        }
    }

    public enum Status
    {
        Active = 1,
        Inactive = 0
    }
    public static class StatusExtend
    {
        #region Int
        public static bool isActive(this int value)
        {
            return value == Status.Active.toInt();
        }

        public static bool isInactive(this int value)
        {
            return value == Status.Inactive.toInt();
        }
        #endregion
        #region Status
        public static bool isActive(this Status value)
        {
            return value == Status.Active;
        }

        public static bool isInactive(this Status value)
        {
            return value == Status.Inactive;
        }
        #endregion
    }
    public static class ActionStatusExtend
    {
        public static int toInt(this Status toInt)
        {
            return (int)toInt;
        }
        public static int toInt(this ActionStatus toInt)
        {
            return (int)toInt;
        }

        #region Int
        public static bool isSuccess(this int value)
        {
            return value == ActionStatus.Success.toInt();
        }
        public static bool isError(this int value)
        {
            return value == ActionStatus.Error.toInt();
        }
        public static bool isDisable(this int value)
        {
            return value == ActionStatus.Disable.toInt();
        }
        public static bool isExist(this int value)
        {
            return value == ActionStatus.Exist.toInt();
        }
        public static bool isNotExit(this int value)
        {
            return value == ActionStatus.NotExit.toInt();
        }
        public static bool isNotMatch(this int value)
        {
            return value == ActionStatus.NotMatch.toInt();
        }
        public static bool isUsing(this int value)
        {
            return value == ActionStatus.Using.toInt();
        }
        public static bool isTransactionError(this int value)
        {
            return value == ActionStatus.TransactionError.toInt();
        }
        #endregion
        #region ActionStatus
        public static bool isSuccess(this ActionStatus value)
        {
            return value == ActionStatus.Success;
        }
        public static bool isError(this ActionStatus value)
        {
            return value == ActionStatus.Error;
        }
        public static bool isDisable(this ActionStatus value)
        {
            return value == ActionStatus.Disable;
        }
        public static bool isNotExit(this ActionStatus value)
        {
            return value == ActionStatus.NotExit;
        }
        public static bool isNotMatch(this ActionStatus value)
        {
            return value == ActionStatus.NotMatch;
        }
        public static bool isUsing(this ActionStatus value)
        {
            return value == ActionStatus.Using;
        }
        public static bool isTransactionError(this ActionStatus value)
        {
            return value == ActionStatus.TransactionError;
        }
        #endregion
     
    }


}
