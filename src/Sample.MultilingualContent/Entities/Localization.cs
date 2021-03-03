namespace Sample.MultilingualContent.Entities
{
    public class Localization : EntityBase
    {
        public string LanguageId { get; set; }
        public string Value { get; set; }

        public virtual LocalizationSet LocalizationSet { get; set; }

        public virtual Language Language { get; set; }
        
    }
}
