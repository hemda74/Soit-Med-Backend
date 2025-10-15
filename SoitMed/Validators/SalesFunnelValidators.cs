using FluentValidation;
using SoitMed.DTO;
using SoitMed.Models.Enums;

namespace SoitMed.Validators
{
    public class CreateActivityRequestValidator : AbstractValidator<CreateActivityRequestDto>
    {
        public CreateActivityRequestValidator()
        {
            RuleFor(x => x.InteractionType)
                .IsInEnum()
                .WithMessage("Invalid interaction type. Valid values are: Visit, FollowUp");

            RuleFor(x => x.ClientType)
                .IsInEnum()
                .WithMessage("Invalid client type. Valid values are: A, B, C, D");

            RuleFor(x => x.Result)
                .IsInEnum()
                .WithMessage("Invalid activity result. Valid values are: Interested, NotInterested");

            RuleFor(x => x.Reason)
                .IsInEnum()
                .When(x => x.Result == ActivityResult.NotInterested)
                .WithMessage("Rejection reason is required when result is NotInterested");

            RuleFor(x => x.Comment)
                .MaximumLength(2000)
                .WithMessage("Comment cannot exceed 2000 characters");

            RuleFor(x => x.DealInfo)
                .NotNull()
                .When(x => x.Result == ActivityResult.Interested && x.OfferInfo == null)
                .WithMessage("Either DealInfo or OfferInfo must be provided when result is Interested");

            RuleFor(x => x.OfferInfo)
                .NotNull()
                .When(x => x.Result == ActivityResult.Interested && x.DealInfo == null)
                .WithMessage("Either DealInfo or OfferInfo must be provided when result is Interested");

            RuleFor(x => x.DealInfo)
                .Null()
                .When(x => x.OfferInfo != null)
                .WithMessage("Cannot provide both DealInfo and OfferInfo");

            RuleFor(x => x.OfferInfo)
                .Null()
                .When(x => x.DealInfo != null)
                .WithMessage("Cannot provide both DealInfo and OfferInfo");
        }
    }

    public class CreateDealValidator : AbstractValidator<CreateDealDto>
    {
        public CreateDealValidator()
        {
            RuleFor(x => x.DealValue)
                .GreaterThan(0)
                .WithMessage("Deal value must be greater than 0");

            RuleFor(x => x.ExpectedCloseDate)
                .GreaterThan(DateTime.Today)
                .When(x => x.ExpectedCloseDate.HasValue)
                .WithMessage("Expected close date must be in the future");
        }
    }

    public class CreateOfferValidator : AbstractValidator<CreateOfferDto>
    {
        public CreateOfferValidator()
        {
            RuleFor(x => x.OfferDetails)
                .NotEmpty()
                .WithMessage("Offer details are required")
                .MaximumLength(2000)
                .WithMessage("Offer details cannot exceed 2000 characters");

            RuleFor(x => x.DocumentUrl)
                .MaximumLength(500)
                .WithMessage("Document URL cannot exceed 500 characters")
                .Must(BeValidUrl)
                .When(x => !string.IsNullOrEmpty(x.DocumentUrl))
                .WithMessage("Document URL must be a valid URL");
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }

    public class UpdateDealValidator : AbstractValidator<UpdateDealDto>
    {
        public UpdateDealValidator()
        {
            RuleFor(x => x.DealValue)
                .GreaterThan(0)
                .When(x => x.DealValue.HasValue)
                .WithMessage("Deal value must be greater than 0");

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage("Invalid deal status");

            RuleFor(x => x.ExpectedCloseDate)
                .GreaterThan(DateTime.Today)
                .When(x => x.ExpectedCloseDate.HasValue)
                .WithMessage("Expected close date must be in the future");
        }
    }

    public class UpdateOfferValidator : AbstractValidator<UpdateOfferDto>
    {
        public UpdateOfferValidator()
        {
            RuleFor(x => x.OfferDetails)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.OfferDetails))
                .WithMessage("Offer details cannot exceed 2000 characters");

            RuleFor(x => x.Status)
                .IsInEnum()
                .When(x => x.Status.HasValue)
                .WithMessage("Invalid offer status");

            RuleFor(x => x.DocumentUrl)
                .MaximumLength(500)
                .WithMessage("Document URL cannot exceed 500 characters")
                .Must(BeValidUrl)
                .When(x => !string.IsNullOrEmpty(x.DocumentUrl))
                .WithMessage("Document URL must be a valid URL");
        }

        private bool BeValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}

