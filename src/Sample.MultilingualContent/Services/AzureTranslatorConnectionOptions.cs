namespace Sample.MultilingualContent.Services
{
    public class AzureTranslatorConnectionOptions
    {
        public static string Name = "Translator";

        public string Endpoint { get; set; }

        public string SubscriptionKey { get; set; }

        public string Region { get; set; }
    }
}
