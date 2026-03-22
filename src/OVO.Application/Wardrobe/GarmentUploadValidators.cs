using FluentValidation;

namespace OVO.Wardrobe;

public class GarmentUploadPresignInputDtoValidator : AbstractValidator<GarmentUploadPresignInputDto>
{
    public GarmentUploadPresignInputDtoValidator()
    {
        RuleFor(x => x.FileExtension)
            .NotEmpty()
            .Must(WardrobeObjectKeyHelper.IsAllowedImageExtension)
            .WithMessage("Desteklenen uzantılar: .jpg, .jpeg, .png, .webp.");

        RuleFor(x => x.ExpirySeconds)
            .InclusiveBetween(60, 60 * 60 * 24 * 7);
    }
}

public class CreateGarmentAfterClientUploadDtoValidator : AbstractValidator<CreateGarmentAfterClientUploadDto>
{
    public CreateGarmentAfterClientUploadDtoValidator()
    {
        RuleFor(x => x.GarmentId)
            .NotEmpty();

        RuleFor(x => x.ObjectKey)
            .NotEmpty()
            .MaximumLength(1024);

        RuleFor(x => x.SubCategory).MaximumLength(128).When(x => x.SubCategory != null);
        RuleFor(x => x.Color).MaximumLength(64).When(x => x.Color != null);
        RuleFor(x => x.Pattern).MaximumLength(64).When(x => x.Pattern != null);
        RuleFor(x => x.Seasons).MaximumLength(256).When(x => x.Seasons != null);
        RuleFor(x => x.Size).MaximumLength(32).When(x => x.Size != null);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}

public class CreateGarmentMultipartMetadataDtoValidator : AbstractValidator<CreateGarmentMultipartMetadataDto>
{
    public CreateGarmentMultipartMetadataDtoValidator()
    {
        RuleFor(x => x.SubCategory).MaximumLength(128).When(x => x.SubCategory != null);
        RuleFor(x => x.Color).MaximumLength(64).When(x => x.Color != null);
        RuleFor(x => x.Pattern).MaximumLength(64).When(x => x.Pattern != null);
        RuleFor(x => x.Seasons).MaximumLength(256).When(x => x.Seasons != null);
        RuleFor(x => x.Size).MaximumLength(32).When(x => x.Size != null);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}
