using FluentValidation;
using SoitMed.DTO;

namespace SoitMed.Validators
{
    public class CreateWeeklyPlanDtoValidator : AbstractValidator<CreateWeeklyPlanDto>
    {
        public CreateWeeklyPlanDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.WeekStartDate)
                .NotEqual(default(DateTime)).WithMessage("Week start date is required.");

            RuleFor(x => x.WeekEndDate)
                .NotEqual(default(DateTime)).WithMessage("Week end date is required.")
                .GreaterThan(x => x.WeekStartDate)
                .WithMessage("Week end date must be after week start date.");

            RuleForEach(x => x.Tasks)
                .SetValidator(new CreateWeeklyPlanTaskDtoValidator())
                .When(x => x.Tasks != null && x.Tasks.Any());
        }
    }

    public class CreateWeeklyPlanTaskDtoValidator : AbstractValidator<CreateWeeklyPlanTaskDto>
    {
        public CreateWeeklyPlanTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Task title is required.")
                .MaximumLength(300).WithMessage("Task title cannot exceed 300 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Task description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
        }
    }

    public class AddTaskToWeeklyPlanDtoValidator : AbstractValidator<AddTaskToWeeklyPlanDto>
    {
        public AddTaskToWeeklyPlanDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Task title is required.")
                .MaximumLength(300).WithMessage("Task title cannot exceed 300 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Task description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
        }
    }

    public class UpdateWeeklyPlanDtoValidator : AbstractValidator<UpdateWeeklyPlanDto>
    {
        public UpdateWeeklyPlanDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));
        }
    }

    public class UpdateWeeklyPlanTaskDtoValidator : AbstractValidator<UpdateWeeklyPlanTaskDto>
    {
        public UpdateWeeklyPlanTaskDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Task title is required.")
                .MaximumLength(300).WithMessage("Task title cannot exceed 300 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Task description cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
        }
    }

    public class CreateDailyProgressDtoValidator : AbstractValidator<CreateDailyProgressDto>
    {
        public CreateDailyProgressDtoValidator()
        {
            RuleFor(x => x.ProgressDate)
                .NotEmpty().WithMessage("Progress date is required.")
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Progress date cannot be in the future.");

            RuleFor(x => x.Notes)
                .NotEmpty().WithMessage("Progress notes are required.")
                .MaximumLength(2000).WithMessage("Progress notes cannot exceed 2000 characters.");

            RuleFor(x => x.TasksWorkedOn)
                .Must(tasks => tasks == null || tasks.All(id => id > 0))
                .WithMessage("All task IDs must be greater than 0.")
                .When(x => x.TasksWorkedOn != null && x.TasksWorkedOn.Any());
        }
    }

    public class UpdateDailyProgressDtoValidator : AbstractValidator<UpdateDailyProgressDto>
    {
        public UpdateDailyProgressDtoValidator()
        {
            RuleFor(x => x.Notes)
                .NotEmpty().WithMessage("Progress notes are required.")
                .MaximumLength(2000).WithMessage("Progress notes cannot exceed 2000 characters.");

            RuleFor(x => x.TasksWorkedOn)
                .Must(tasks => tasks == null || tasks.All(id => id > 0))
                .WithMessage("All task IDs must be greater than 0.")
                .When(x => x.TasksWorkedOn != null && x.TasksWorkedOn.Any());
        }
    }

    public class ReviewWeeklyPlanDtoValidator : AbstractValidator<ReviewWeeklyPlanDto>
    {
        public ReviewWeeklyPlanDtoValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.")
                .When(x => x.Rating.HasValue);

            RuleFor(x => x.ManagerComment)
                .MaximumLength(1000)
                .WithMessage("Manager comment cannot exceed 1000 characters.")
                .When(x => !string.IsNullOrEmpty(x.ManagerComment));

            // At least one of Rating or ManagerComment must be provided
            RuleFor(x => x)
                .Must(x => x.Rating.HasValue || !string.IsNullOrEmpty(x.ManagerComment))
                .WithMessage("Either rating or manager comment must be provided.");
        }
    }

    public class FilterWeeklyPlansDtoValidator : AbstractValidator<FilterWeeklyPlansDto>
    {
        public FilterWeeklyPlansDtoValidator()
        {
            RuleFor(x => x.EmployeeId)
                .MaximumLength(450).WithMessage("Employee ID cannot exceed 450 characters.")
                .When(x => !string.IsNullOrEmpty(x.EmployeeId));

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage("Start date must be less than or equal to end date.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

            RuleFor(x => x.MinRating)
                .InclusiveBetween(1, 5)
                .WithMessage("Min rating must be between 1 and 5.")
                .When(x => x.MinRating.HasValue);

            RuleFor(x => x.MaxRating)
                .InclusiveBetween(1, 5)
                .WithMessage("Max rating must be between 1 and 5.")
                .When(x => x.MaxRating.HasValue);

            RuleFor(x => x)
                .Must(x => !x.MinRating.HasValue || !x.MaxRating.HasValue || x.MinRating.Value <= x.MaxRating.Value)
                .WithMessage("Min rating must be less than or equal to max rating.")
                .When(x => x.MinRating.HasValue && x.MaxRating.HasValue);

            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");
        }
    }
}






