namespace PersonalFinance.Shared.Constraints;
public static class ValueObjectConstraints
{
    public static class Money
    {
        public const int Precision = 18;
        public const int Scale = 2;
    }
}
