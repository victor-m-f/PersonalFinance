using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalFinance.Domain.ValueObjects;

namespace PersonalFinance.Infrastructure.Data.Converters;
public sealed class CategoryColorConverter : ValueConverter<CategoryColor, string>
{
    public CategoryColorConverter()
        : base(
            color => color.Value,
            value => CategoryColor.Create(value).Value!)
    {
    }
}