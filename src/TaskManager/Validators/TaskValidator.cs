using FluentValidation;

namespace TaskManager.Validators
{
  public class TaskValidator : AbstractValidator<Models.Task>
  {
    public TaskValidator()
    {
      RuleFor(x => x.Title)
        .NotEmpty()
        .MaximumLength(100)
        .WithMessage("Title cannot exceed 100 characters.");
      RuleFor(x => x.Description)
        .NotEmpty()
        .MaximumLength(500)
        .WithMessage("Description cannot exceed 500 characters.");
    }
  }
}