using FluentValidation;
using PollSurveyBuilder.Application.DTOs;

namespace PollSurveyBuilder.Application.Validators
{
    public class CastVoteValidator : AbstractValidator<CastVoteDTO>
    {
        public CastVoteValidator()
        {
            RuleFor(x => x)
                .Must(x => x.OptionId.HasValue || !string.IsNullOrWhiteSpace(x.TextAnswer))
                .WithMessage("Either OptionId or TextAnswer must be provided.");

            RuleFor(x => x.TextAnswer)
                .MaximumLength(1000).WithMessage("Answer must be 1000 characters or fewer.")
                .When(x => x.TextAnswer != null);
        }
    }
}
