using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Save;
using System;
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
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Patterns for actual HTML
            string iframePattern = @"<iframe\b[^>]*>(.*?)<\/iframe>|<iframe\b[^>]*/?>";
            string scriptWithIframePattern = @"<script\b[^>]*>.*?<iframe\b[\s\S]*?<\/iframe>.*?<\/script>";
            string objectWithIframePattern = @"<object\b[^>]*>.*?<iframe\b[\s\S]*?<\/iframe>.*?<\/object>";

            // Patterns for HTML-encoded versions
            string encodedIframePattern = @"&lt;iframe\b[^&]*&gt;(.*?)&lt;/iframe&gt;|&lt;iframe\b[^&]*/?&gt;";
            string encodedScriptWithIframePattern = @"&lt;script\b[^&]*&gt;.*?&lt;iframe\b[^&]*&gt;.*?&lt;/iframe&gt;.*?&lt;/script&gt;";
            string encodedObjectWithIframePattern = @"&lt;object\b[^&]*&gt;.*?&lt;iframe\b[^&]*&gt;.*?&lt;/iframe&gt;.*?&lt;/object&gt;";

            // Step 1: Remove direct HTML iframe/script/object
            string cleaned = input;
            cleaned = Regex.Replace(cleaned, iframePattern, string.Empty, RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, scriptWithIframePattern, string.Empty, RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, objectWithIframePattern, string.Empty, RegexOptions.IgnoreCase);

            // Step 2: Decode input to handle encoded versions
            string decoded = System.Net.WebUtility.HtmlDecode(input);

            // Step 3: Remove encoded iframe/script/object
            string decodedCleaned = decoded;
            decodedCleaned = Regex.Replace(decodedCleaned, iframePattern, string.Empty, RegexOptions.IgnoreCase);
            decodedCleaned = Regex.Replace(decodedCleaned, scriptWithIframePattern, string.Empty, RegexOptions.IgnoreCase);
            decodedCleaned = Regex.Replace(decodedCleaned, objectWithIframePattern, string.Empty, RegexOptions.IgnoreCase);
            decodedCleaned = Regex.Replace(decodedCleaned, encodedIframePattern, string.Empty, RegexOptions.IgnoreCase);
            decodedCleaned = Regex.Replace(decodedCleaned, encodedScriptWithIframePattern, string.Empty, RegexOptions.IgnoreCase);
            decodedCleaned = Regex.Replace(decodedCleaned, encodedObjectWithIframePattern, string.Empty, RegexOptions.IgnoreCase);

            // Step 4: Return re-encoded result if decoded version changed
            if (!decoded.Equals(decodedCleaned, StringComparison.OrdinalIgnoreCase))
            {
                return System.Net.WebUtility.HtmlEncode(decodedCleaned);
            }

            return cleaned;
        }
    }
}