using FluentValidation;

namespace OVO.FileStorage;

public class GetPresignedPutUrlInputValidator : AbstractValidator<GetPresignedPutUrlInput>
{
    public GetPresignedPutUrlInputValidator()
    {
        RuleFor(x => x.ObjectKey)
            .NotEmpty()
            .MaximumLength(1024)
            .Must(StorageKeyRules.IsSafeObjectKey)
            .WithMessage("ObjectKey geçersiz veya güvenli değil.");

        RuleFor(x => x.ExpirySeconds)
            .InclusiveBetween(60, 60 * 60 * 24 * 7);
    }
}

public class GetPresignedGetUrlInputValidator : AbstractValidator<GetPresignedGetUrlInput>
{
    public GetPresignedGetUrlInputValidator()
    {
        RuleFor(x => x.ObjectKey)
            .NotEmpty()
            .MaximumLength(1024)
            .Must(StorageKeyRules.IsSafeObjectKey)
            .WithMessage("ObjectKey geçersiz veya güvenli değil.");

        RuleFor(x => x.ExpirySeconds)
            .InclusiveBetween(60, 60 * 60 * 24 * 7);
    }
}
