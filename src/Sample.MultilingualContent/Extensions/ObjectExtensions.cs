using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Sample.MultilingualContent
{
    public static class ObjectExtensions
    {
        public static string ToJson<T>(this T obj, JsonSerializerOptions options = null)
        {
            var actualOptions = options ?? new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All, UnicodeRanges.Cyrillic),
            };

            return JsonSerializer.Serialize<T>(obj, actualOptions);
        }
    }
}
