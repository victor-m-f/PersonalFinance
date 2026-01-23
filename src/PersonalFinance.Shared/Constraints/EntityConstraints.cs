namespace PersonalFinance.Shared.Constraints;
public static class EntityConstraints
{
    public static class Category
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 60;
        public const int ColorHexLength = 9;
        public const string ColorHexRegex = "^#[0-9A-Fa-f]{8}$";
    }

    public static class Expense
    {
        public const int DescriptionMaxLength = 200;
        public const int DescriptionSearchMaxLength = 256;
    }
}
