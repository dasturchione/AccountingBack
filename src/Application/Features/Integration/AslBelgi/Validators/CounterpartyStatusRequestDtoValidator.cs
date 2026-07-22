using Application.Features.Integration.AslBelgi.DTOs;
using FluentValidation;

namespace Application.Features.Integration.AslBelgi.Validators;

public sealed class CounterpartyStatusRequestDtoValidator : AbstractValidator<CounterpartyStatusRequestDto>
{
    public CounterpartyStatusRequestDtoValidator()
    {
        RuleFor(x => x.Tin)
            .NotEmpty()
            .Matches("^(?:\\d{9}|\\d{14})$");
    }
}
