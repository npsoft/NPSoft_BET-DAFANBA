using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PhotoBookmart.DataLayer
{
    /// <summary>
    /// Some useful extension
    /// </summary>
    public static class StringExtension
    {
        static string ItemSeperatorString = ",";

        /// <summary>
        /// Copyright ServiceStack V3
        /// https://github.com/ServiceStack/ServiceStack/blob/v3/src/ServiceStack.Common/StringExtensions.cs
        /// </summary>
        /// <param name="items"></param>
        public static string Join(this List<string> items)
        {
            return String.Join(ItemSeperatorString, items.ToArray());
        }

        public static string Join(this List<string> items, string delimeter)
        {
            return String.Join(delimeter, items.ToArray());
        }

        /// <summary>
        /// Return SEO URL base on input 
        /// Copyright Immanuel Dang (trungdt@absoft.vn)
        /// </summary>
        public static string ToSeoUrl(this string url)
        {
            url = url.RemoveDiacritics();

            // make the url lowercase
            string encodedUrl = (url ?? "").ToLower();

            encodedUrl = encodedUrl.Replace("®", "r");

            // replace & with and
            encodedUrl = Regex.Replace(encodedUrl, @"\&+", "and");

            // remove characters
            encodedUrl = encodedUrl.Replace("'", "");

            // remove invalid characters
            encodedUrl = Regex.Replace(encodedUrl, @"[^a-z0-9]", "_");

            // remove duplicates
            encodedUrl = Regex.Replace(encodedUrl, @"-+", "-");

            // trim leading & trailing characters
            encodedUrl = encodedUrl.Trim('-');

            return encodedUrl;
        }

        /// <summary>
        /// Remove all Diacritics (non ASCII char)
        /// Copyright Immanuel Dang (trungdt@absoft.vn)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String RemoveDiacritics(this string s)
        {
            String normalizedString = s.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                Char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Generate random string 
        /// Copyright Immanuel Dang (trungdt@absoft.vn)
        /// </summary>
        public static string GenerateRandomText(this string text, int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            if (!(length > 0))
            {
                length = 3; // 3 by default
            }

            var result = new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }
    }
}
