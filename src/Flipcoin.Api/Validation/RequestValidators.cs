using Flipcoin.Api.Contracts.Auth;
using Flipcoin.Api.Contracts.Game;
using Flipcoin.Api.Contracts.Transfers;
using FluentValidation;

namespace Flipcoin.Api.Validation;

// Validators for the incoming HTTP request contracts. They run at the API
// boundary (see ValidationFilter); the domain still enforces its own invariants.

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.ToAddress).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m);
    }
}

public class PlayGameRequestValidator : AbstractValidator<PlayGameRequest>
{
    public PlayGameRequestValidator()
    {
        RuleFor(x => x.Choice).IsInEnum();
        // Stake is optional (practice), but if supplied it must be positive.
        When(x => x.Stake.HasValue, () =>
            RuleFor(x => x.Stake!.Value).GreaterThan(0m));
    }
}
