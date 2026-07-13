using FluentValidation;
using Products.Application.Dtos;
using Products.Application.Mapping;

namespace Products.Application.Validation;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(64)
            .Matches("^[A-Za-z0-9][A-Za-z0-9-_]*$")
            .WithMessage("'Sku' may only contain letters, numbers, hyphens and underscores.");

        RuleFor(x => x.Price)
            .GreaterThan(0m);

        RuleFor(x => x.Colour)
            .NotEmpty()
            .Must(colour => ProductColourParser.TryParse(colour, out _))
            .WithMessage($"'Colour' must be one of: {ProductColourParser.ValidValues}.");

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}
