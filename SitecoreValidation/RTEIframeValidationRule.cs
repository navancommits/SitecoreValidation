using Sitecore.Data.Validators;
using System.Text.RegularExpressions;

namespace SitecoreValidation.Validators
{
    public class RTEIframeValidationRule : StandardValidator
    {
        public override string Name => "RTEIframeValidationRule";

        protected override ValidatorResult Evaluate()
        {
            string fieldValue = GetControlValidationValue();

            if (string.IsNullOrWhiteSpace(fieldValue)) return ValidatorResult.Valid;

            if (Regex.IsMatch(fieldValue, "<iframe", RegexOptions.IgnoreCase))
            {
                Text = "Usage of <iframe> is not allowed in this field.";
                return GetFailedResult(ValidatorResult.CriticalError);
            }

            return ValidatorResult.Valid;
        }

        protected override ValidatorResult GetMaxValidatorResult() => ValidatorResult.CriticalError;
    }
}