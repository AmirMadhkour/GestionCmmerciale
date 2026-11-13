using FluentValidation;
using GestionCommerciale.Application.DTOs.Clients;

namespace GestionCommerciale.Application.Validators.Clients;

public class CreateClientValidator : AbstractValidator<CreateClientDto>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Le nom ne doit pas dépasser 100 caractères.");

        RuleFor(x => x.FirstNameOrCompany)
            .NotEmpty().WithMessage("Le prénom ou la raison sociale est obligatoire.")
            .MaximumLength(150).WithMessage("La raison sociale ne doit pas dépasser 150 caractères.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est obligatoire.")
            .EmailAddress().WithMessage("Le format de l'email est invalide.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Le numéro de téléphone est obligatoire.")
            .Matches(@"^\+?[0-9\s]{8,15}$").WithMessage("Le format du téléphone est invalide.");
    }
}