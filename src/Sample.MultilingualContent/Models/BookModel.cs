using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.MultilingualContent.Models
{
    public record BookDetail(string LanguageCode, string Title, string Author);

    public record BookModel(string Id, IEnumerable<BookDetail> Details);

    public record BookSaveRequestModel(string Id, string CriteriaLanguageCode, IEnumerable<BookDetail> Details, bool UseTranslation, bool IsHtmlContent, bool IsTranslationEachLanguage);
}
