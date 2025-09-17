using static HospitalityCustomerAPI.Common.Enum;

namespace HospitalityCustomerAPI.Common
{
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
