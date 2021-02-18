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
            SeedLanguages(dbContext);
        } 

        private static void SeedLanguages(AppDbContext dbContext)
        {
            if (!dbContext.Languages.Any())
            {
                dbContext.Languages.AddRange(
                    CreateLanguage("ko-KR", "한국어"),
                    CreateLanguage("en-US", "English"),
                    CreateLanguage("ru-RU", "русский"),
                    CreateLanguage("ja-JP", "日本語"),
                    CreateLanguage("zh-CN", "中国語"),
                    CreateLanguage("es-ES", "española")
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
