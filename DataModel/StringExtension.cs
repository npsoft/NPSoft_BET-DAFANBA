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

        public static string GetCodeProvince(this string text)
        {
            return string.IsNullOrEmpty(text) || text.Length < 2 ? "" : text.Substring(0, 2);
        }

        public static string GetCodeDistrict(this string text)
        {
            return string.IsNullOrEmpty(text) || text.Length < 5 ? "" : text.Substring(0, 5);
        }

        public static string GetCodeVillage(this string text)
        {
            return string.IsNullOrEmpty(text) || text.Length < 10 ? "" : text.Substring(0, 10);
        }

        public static bool CheckDateOfBirth(this string type, string year, string month, string date)
        {
            int y = int.Parse(year);
            int m = string.IsNullOrEmpty(month) ? 1 : int.Parse(month);
            int d = string.IsNullOrEmpty(date) ? 1 : int.Parse(date);

            int dis = DateTime.Today.Year - y;
            if (DateTime.Today.Month > m ||
                DateTime.Today.Month == m && DateTime.Today.Day > d)
            {
                dis -= 1;
            }
            switch (type)
            {
                case "0101":
                    return dis < 4;
                case "0102":
                    return dis >= 4 && dis < 16;
                case "0103":
                    return dis >= 16 && dis <= 22;
                case "0201":
                    return dis < 4;
                case "0202":
                    return dis >= 4 && dis < 16;
                case "0203":
                    return dis >= 16;
                case "0401":
                    return dis >= 60 && dis < 80;
                case "0402":
                    return dis >= 80;
                case "0403":
                    return dis >= 80;
                case "0601":
                    return dis < 16;
                default:
                    return true;
            }
        }

        public static string GetGender(this string text)
        {
            return text == "Male" ? "Nam" : (text == "Female" ? "Nữ" : "");
        }
    }
}
