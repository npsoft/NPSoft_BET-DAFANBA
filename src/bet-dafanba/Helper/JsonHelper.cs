using System;
using System.Text.RegularExpressions;

namespace SpiralEdge.Helper
{
    public class JsonHelper
    {
        public static dynamic GetElBySelector(dynamic json, string selector, bool clone = true)
        {
            dynamic el = json;

            MatchCollection mc = new Regex(@"([^\.\[\]]+(?=\.|\[\d\]|$))|(\[\d+\])", RegexOptions.Compiled).Matches(selector);
            foreach (Match m in mc)
            {
                Match match = new Regex(@"^\[(\d+)\]$", RegexOptions.Compiled).Match(m.Value);
                if (match.Success)
                {
                    el = el[int.Parse(match.Groups[1].Value)];
                }
                else
                {
                    el = el[m.Value];
                }
            }

            if (clone && (
                el.GetType() == typeof(Newtonsoft.Json.Linq.JObject) ||
                el.GetType() == typeof(Newtonsoft.Json.Linq.JArray)))
            {
                el = Newtonsoft.Json.JsonConvert.DeserializeObject(string.Format("{0}", el));
            }

            return el;
        }

        public JsonHelper() { }
    }
}
