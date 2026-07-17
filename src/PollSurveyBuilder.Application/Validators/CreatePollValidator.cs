using FluentValidation;
using PollSurveyBuilder.Application.DTOs;
using PollSurveyBuilder.Domain.Enums;

namespace PollSurveyBuilder.Application.Validators
{
    public class CreatePollValidator : AbstractValidator<CreatePollDTO>
    {
        public CreatePollValidator()
        {
            RuleFor(x => x.Question)
                .NotEmpty().WithMessage("Question is required.")
                .MaximumLength(300).WithMessage("Question must be 300 characters or fewer.");

            RuleFor(x => x.ExpiresInMinutes)
                .GreaterThan(0).When(x => x.ExpiresInMinutes.HasValue)
                .WithMessage("ExpiresInMinutes must be a positive number of minutes.");

            // SingleChoice is the only type where the caller supplies free-text options.
            When(x => x.Type == PollType.SingleChoice, () =>
            {
                RuleFor(x => x.Options)
                    .Must(o => o.Count >= 2 && o.Count <= 6)
                    .WithMessage("A single-choice poll needs between 2 and 6 options.");

                RuleForEach(x => x.Options)
                    .NotEmpty().WithMessage("Options cannot be blank.")
                    .MaximumLength(120).WithMessage("Each option must be 120 characters or fewer.");
            });
        }
    }
}
