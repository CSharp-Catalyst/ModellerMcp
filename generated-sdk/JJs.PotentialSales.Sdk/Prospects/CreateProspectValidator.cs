using FluentValidation;

namespace JJs.PotentialSales.Sdk.Prospects;

/// <summary>
/// Validator for creating new prospects
/// </summary>
public class CreateProspectValidator : AbstractValidator<CreateProspectRequest>
{
    public CreateProspectValidator()
    {
        RuleFor(x => x.PotentialSaleNumber)
            .NotEmpty()
            .WithMessage("Potential sale number is required")
            .Length(10, 10)
            .WithMessage("Potential sale number must be exactly 10 characters")
            .Matches(@"^[A-Z0-9]{10}$")
            .WithMessage("Potential sale number must contain only uppercase letters and numbers");

        RuleFor(x => x.SiteNumber)
            .NotEmpty()
            .WithMessage("Site number is required")
            .MaximumLength(50)
            .WithMessage("Site number cannot exceed 50 characters");

        RuleFor(x => x.Assignee)
            .NotEmpty()
            .WithMessage("Assignee is required")
            .MaximumLength(100)
            .WithMessage("Assignee cannot exceed 100 characters");

        RuleFor(x => x.ProspectTypeId)
            .NotEmpty()
            .WithMessage("Prospect type ID is required");

        RuleFor(x => x.SourceId)
            .NotEmpty()
            .WithMessage("Source ID is required");

        RuleFor(x => x.CustomerStatus)
            .IsInEnum()
            .WithMessage("Customer status must be a valid value");

        RuleFor(x => x.CustomerNumber)
            .MaximumLength(50)
            .WithMessage("Customer number cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.CustomerNumber));

        RuleFor(x => x.TradingName)
            .NotEmpty()
            .WithMessage("Trading name is required")
            .MaximumLength(512)
            .WithMessage("Trading name cannot exceed 512 characters");

        RuleFor(x => x.ProspectStatus)
            .IsInEnum()
            .WithMessage("Prospect status must be a valid value");

        RuleFor(x => x.Interest)
            .IsInEnum()
            .WithMessage("Interest must be a valid value");

        RuleFor(x => x.SalesFollowUpDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Sales follow-up date must be in the future")
            .When(x => x.SalesFollowUpDate.HasValue);

        RuleFor(x => x.SalesFollowUpDescription)
            .MaximumLength(200)
            .WithMessage("Sales follow-up description cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.SalesFollowUpDescription));

        RuleFor(x => x.QuoteProvidedDescription)
            .MaximumLength(200)
            .WithMessage("Quote provided description cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.QuoteProvidedDescription));

        RuleFor(x => x.AddressLine)
            .NotEmpty()
            .WithMessage("Address line is required")
            .MaximumLength(500)
            .WithMessage("Address line cannot exceed 500 characters");

        RuleFor(x => x.ContactFirstName)
            .MaximumLength(100)
            .WithMessage("Contact first name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactFirstName));

        RuleFor(x => x.ContactLastName)
            .MaximumLength(100)
            .WithMessage("Contact last name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactLastName));

        RuleFor(x => x.ContactPhone)
            .MaximumLength(20)
            .WithMessage("Contact phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));

        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .WithMessage("Contact email must be a valid email address")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.Description)
            .MaximumLength(8000)
            .WithMessage("Description cannot exceed 8000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
