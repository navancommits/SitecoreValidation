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
                                string originalValue = field.Value;
                                string cleanedValue = StripIframeTags(originalValue);

                                if (cleanedValue != originalValue)
                                {
                                    field.Value = cleanedValue;
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

            private string StripIframeTags(string input)
            {
                // Remove actual iframe tags
                string cleaned = Regex.Replace(input, @"<iframe[\s\S]*?</iframe>", string.Empty, RegexOptions.IgnoreCase);

                // Decode HTML-encoded content
                string decoded = System.Net.WebUtility.HtmlDecode(input);
                string decodedCleaned = Regex.Replace(decoded, @"<iframe[\s\S]*?</iframe>", string.Empty, RegexOptions.IgnoreCase);

                // If encoded version had iframes removed, return re-encoded string
                if (decoded != decodedCleaned)
                {
                    return System.Net.WebUtility.HtmlEncode(decodedCleaned);
                }

                return cleaned;
            }
        }
}