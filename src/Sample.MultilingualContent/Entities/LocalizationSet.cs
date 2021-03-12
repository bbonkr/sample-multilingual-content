using System.Collections.Generic;

namespace Sample.MultilingualContent.Entities
{
    public class LocalizationSet:EntityBase
    {
        public virtual IList<Localization> Localizations { get; set; }
    }
}
