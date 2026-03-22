using FluentValidation;

namespace OVO.Wardrobe;

public class UpdateGarmentDtoValidator : AbstractValidator<UpdateGarmentDto>
{
    public UpdateGarmentDtoValidator()
    {
        RuleFor(x => x.SubCategory).MaximumLength(128).When(x => x.SubCategory != null);
        RuleFor(x => x.Color).MaximumLength(64).When(x => x.Color != null);
        RuleFor(x => x.Pattern).MaximumLength(64).When(x => x.Pattern != null);
        RuleFor(x => x.Seasons).MaximumLength(256).When(x => x.Seasons != null);
        RuleFor(x => x.Size).MaximumLength(32).When(x => x.Size != null);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}
