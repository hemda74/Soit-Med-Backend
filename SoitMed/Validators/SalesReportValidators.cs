using FluentValidation;
using SoitMed.DTO;

namespace SoitMed.Validators
{
    public class CreateSalesReportDtoValidator : AbstractValidator<CreateSalesReportDto>
    {
        private readonly string[] _allowedTypes = { "daily", "weekly", "monthly", "custom" };

        public CreateSalesReportDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Body)
                .NotEmpty().WithMessage("Body is required.")
                .MaximumLength(2000).WithMessage("Body cannot exceed 2000 characters.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required.")
                .Must(type => _allowedTypes.Contains(type.ToLower()))
                .WithMessage("Type must be one of: daily, weekly, monthly, custom.");

            RuleFor(x => x.ReportDate)
                .NotEmpty().WithMessage("Report date is required.")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Report date cannot be in the future.");
        }
    }

    public class UpdateSalesReportDtoValidator : AbstractValidator<UpdateSalesReportDto>
    {
        private readonly string[] _allowedTypes = { "daily", "weekly", "monthly", "custom" };

        public UpdateSalesReportDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Body)
                .NotEmpty().WithMessage("Body is required.")
                .MaximumLength(2000).WithMessage("Body cannot exceed 2000 characters.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required.")
                .Must(type => _allowedTypes.Contains(type.ToLower()))
                .WithMessage("Type must be one of: daily, weekly, monthly, custom.");

            RuleFor(x => x.ReportDate)
                .NotEmpty().WithMessage("Report date is required.")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Report date cannot be in the future.");
        }
    }

    public class FilterSalesReportsDtoValidator : AbstractValidator<FilterSalesReportsDto>
    {
        private readonly string[] _allowedTypes = { "daily", "weekly", "monthly", "custom" };

        public FilterSalesReportsDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .MaximumLength(450).WithMessage("Employee ID cannot exceed 450 characters.")
                .When(x => !string.IsNullOrEmpty(x.EmployeeId));

            RuleFor(x => x.Type)
                .Must(type => _allowedTypes.Contains(type!.ToLower()))
                .WithMessage("Type must be one of: daily, weekly, monthly, custom.")
                .When(x => !string.IsNullOrEmpty(x.Type));

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage("Start date must be less than or equal to end date.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");
        }
    }

    public class RateSalesReportDtoValidator : AbstractValidator<RateSalesReportDto>
    {
        public RateSalesReportDtoValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.")
                .When(x => x.Rating.HasValue);

            RuleFor(x => x.Comment)
                .MaximumLength(500)
                .WithMessage("Comment cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Comment));

            // At least one of Rating or Comment must be provided
            RuleFor(x => x)
                .Must(x => x.Rating.HasValue || !string.IsNullOrEmpty(x.Comment))
                .WithMessage("Either rating or comment must be provided.");
        }
    }
}

