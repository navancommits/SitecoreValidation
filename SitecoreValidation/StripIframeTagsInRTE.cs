using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Save;
using System.Text.RegularExpressions;

namespace SitecoreValidation.Pipelines.Save
{
    public class StripIframeTagsInRTE
    {
        public void Process(SaveArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));

            foreach (var saveItem in args.Items)
            {
                var item = Sitecore.Context.ContentDatabase.GetItem(saveItem.ID, saveItem.Language, saveItem.Version);
                if (item == null) continue;

                bool hasChanged = false;

                item.Editing.BeginEdit();
                try
                {
                    foreach (Field field in item.Fields)
                    {
                        if (field.TypeKey == "rich text" && !string.IsNullOrEmpty(field.Value))
                        {
                            string sanitized = Regex.Replace(field.Value, @"<iframe[\s\S]*?</iframe>", string.Empty, RegexOptions.IgnoreCase);
                            if (sanitized != field.Value)
                            {
                                field.Value = sanitized;
                                hasChanged = true;
                            }
                        }
                    }
                }
                finally
                {
                    if (hasChanged)
                        item.Editing.EndEdit();
                    else
                        item.Editing.CancelEdit();
                }
            }
        }
    }
}