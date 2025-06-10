using Sitecore.Data.Validators;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Save;
using Sitecore.Web.UI.Sheer;
using System.Collections.Generic;
using System.Xml;

namespace SitecoreValidation.Pipelines.Save
{
    public class ValidateBeforeSave
    {
        public void Process(SaveArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            foreach (var saveItem in args.Items)
            {
                var item = Sitecore.Context.ContentDatabase.GetItem(saveItem.ID, saveItem.Language, saveItem.Version);
                if (item == null) continue;

                // Combine both item-level and field-level validators
                var validators = ValidatorManager.BuildValidators(ValidatorsMode.Gutter| ValidatorsMode.ValidatorBar| ValidatorsMode.ValidateButton, item);

                foreach (BaseValidator validator in validators)
                {
                    Log.Info($"Validator loaded: {validator.Name} - Result: {validator.Result}", this);
                    validator.Validate(new ValidatorOptions(true));

                    if (validator.Result == ValidatorResult.CriticalError)
                    {
                        SheerResponse.Alert("Validation failed: " + validator.Text);
                        //args.AbortPipeline();
                        //return;
                    }
                }
            }
        }
    }
}