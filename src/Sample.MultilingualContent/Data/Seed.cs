using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Sample.MultilingualContent.Entities;

namespace Sample.MultilingualContent.Data
{
    public class Seeder
    {
        public static void Seed(AppDbContext dbContext)
        {
            SeedLanguages(dbContext, true);
        } 

        private static void SeedLanguages(AppDbContext dbContext, bool reset = false)
        {
            if (!dbContext.Languages.Any())
            {
                dbContext.Languages.AddRange(
                    CreateLanguage("ko", "한국어"),
                    CreateLanguage("en", "English"),
                    CreateLanguage("ru", "русский"),
                    CreateLanguage("ja", "日本語"),
                    CreateLanguage("zh-Hans", "简体中文"),
                    CreateLanguage("zh-Hant", "中国传统的"),
                    CreateLanguage("es", "española")
                    );

                dbContext.SaveChanges();
            }
        }

        private static Language CreateLanguage(string code, string name, string description = "")
        {
            return new Language
            {
                Code = code,
                Name = name,
                Description = description,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
