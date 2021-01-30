namespace WDT_AS2.Utilities
{
    public static class MiscellaneousExtensionUtilities
    {
        public static bool HasMoreThanNDecimalPlaces(this decimal value, int n) => decimal.Round(value, n) != value;
        public static bool HasMoreThanTwoDecimalPlaces(this decimal value) => value.HasMoreThanNDecimalPlaces(2);
    }

    public enum Period
    {
        Monthly = 1,
        Quaterly = 2,
        OnceOff = 3,
        Anually = 4
    }

    public enum Status
    {
        Pending = 1,
        Complete = 2,
        Failed = 3,
        Set = 4,
        Blocked = 5
    }

    public enum AccountType
    {
        Checking = 1,
        Saving = 2
    }
    public enum TransactionType
    {
        Deposit = 1,
        Withdraw = 2,
        Transfer = 3,
        ServiceCharge = 4,
        BillPay = 5
    }

    public enum AccountStatus
    {
        Locked = 1,
        UnLocked = 2
    }
}
