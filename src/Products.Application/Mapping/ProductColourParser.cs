using Products.Domain.Enums;

namespace Products.Application.Mapping;

public static class ProductColourParser
{
    public static readonly string ValidValues = string.Join(", ", Enum.GetNames<ProductColour>());

    public static bool TryParse(string? value, out ProductColour colour)
        => Enum.TryParse(value, ignoreCase: true, out colour) && Enum.IsDefined(colour);
}
