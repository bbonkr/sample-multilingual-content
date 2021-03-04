using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.MultilingualContent.Entities
{
    public class Book : EntityBase
    {
        public virtual IList<BookLocalization> Localizations { get; set; }
    }

    public class BookLocalization : EntityBase
    {
        public string LanguageId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public virtual Book Book { get; set; }
        public virtual Language Language { get; set; }
    }
}
