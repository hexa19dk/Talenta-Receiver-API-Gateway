using FluentValidation;

namespace TalentaReceiver.Validators
{
    public class MTPesertaTempValidator : AbstractValidator<Models.MT_Peserta_Temp>
    {
        public MTPesertaTempValidator()
        {
            RuleFor(c => c.KodeJabatan).NotNull().NotEmpty();
            RuleFor(c => c.KodePrsh).NotNull().NotEmpty();
            RuleFor(c => c.KodePool).NotNull().NotEmpty().MaximumLength(2).WithMessage("The length of 'Kode Pool'/'value_code' must be 2 characters or fewer \n"); 
            RuleFor(c => c.NIP).NotNull().NotEmpty().MaximumLength(8).WithMessage("The length of 'NIP'/'employee_id' must be 8 characters or fewer \n");
        }
    }
}
