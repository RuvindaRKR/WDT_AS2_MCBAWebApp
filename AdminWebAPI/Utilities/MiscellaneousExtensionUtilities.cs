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
        Failed = 3
    }
}
