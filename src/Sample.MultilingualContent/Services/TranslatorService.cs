using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sample.MultilingualContent.Models;

namespace Sample.MultilingualContent.Services
{

    public interface ITranslatorService
    {
        Task<IEnumerable<TranslationResultModel>> TranslateAsync(TranslationRequestModel model);
    }

    public class TranslatorService : ITranslatorService
    {
        private const string TAG = "[Azure Translator Service]";

        public const string TRANSLATOR_ROUTE = "/translate?api-version=3.0";
        public const string OCP_APIM_SUBSCRIPTION_KEY = "Ocp-Apim-Subscription-Key";
        public const string OCP_APIM_SUBSCRIPTION_REGION = "Ocp-Apim-Subscription-Region";
        public const string CONTENT_TYPE_KEY = "Content-Type";
        public const string CONTENT_TYPE_VALUE = "application/json";


        public TranslatorService(IOptions<AzureTranslatorConnectionOptions> azureTranslatorConnectionOptionsAccessor, ILoggerFactory loggerFactory)
        {
            azureTranslatorConnectionOptions = azureTranslatorConnectionOptionsAccessor.Value;
            logger = loggerFactory.CreateLogger<TranslatorService>();
        }

        public async Task<IEnumerable<TranslationResultModel>> TranslateAsync(TranslationRequestModel model)
        {
            ValidateAzureTranslateConnectionOptions();
            ValidateRequestbody(model, true);

            List<TranslationResultModel> resultSet = null;

            var requestBody = model.Inputs.ToJson();

            using (var client = new HttpClient())
            {
                foreach(var uri in getRequestUri(model))
                {
                    using (var request = new HttpRequestMessage())
                    {
                        request.Method = HttpMethod.Post;
                        request.RequestUri = uri;

                        request.Headers.Add(OCP_APIM_SUBSCRIPTION_KEY, azureTranslatorConnectionOptions.SubscriptionKey);
                        request.Headers.Add(OCP_APIM_SUBSCRIPTION_REGION, azureTranslatorConnectionOptions.Region);

                        request.Content = new StringContent(requestBody, Encoding.UTF8, CONTENT_TYPE_VALUE);

                        var response = await client.SendAsync(request);

                        if (response.Content == null)
                        {
                            throw new Exception($"{TAG} Response content is empty.");
                        }

                        var resultJson = await response.Content.ReadAsStringAsync();

                        var jsonSerializerOptions = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        };

                        if (response.IsSuccessStatusCode)
                        {
                            var resultModel = JsonSerializer.Deserialize<IEnumerable<TranslationResultModel>>(resultJson, jsonSerializerOptions);

                            logger.LogInformation($"${TAG} The request has been processed. => Translated.");

                            if (resultSet == null)
                            {
                                resultSet = new List<TranslationResultModel>(resultModel);
                            }
                            else
                            {
                                var resultModelIndex = 0;
                                resultModel.ToList().ForEach((result) =>
                                {
                                    var list = resultSet[resultModelIndex].Translations.ToList();
                                    result.Translations.ToList().ForEach((translation) =>
                                        {

                                            list.Add(translation);

                                        });

                                    resultSet[resultModelIndex].Translations = list;
                                    resultModelIndex++;
                                });
                            }
                        }
                        else
                        {
                            var resultModel = JsonSerializer.Deserialize<TranslationErrorResultModel>(resultJson, jsonSerializerOptions);

                            logger.LogInformation($"${TAG} The request does not has been processed. => Not  Translated.");

                            throw new SomethingWrongException<TranslationErrorResultModel>(resultModel.Error.Message, resultModel);
                        }
                    }
                }                
            }

            return resultSet;
        }

        private IEnumerable<Uri> getRequestUri(TranslationRequestModel model)
        {
            if (model.IsTranslationEachLanguage)
            {
                foreach(var language in model.ToLanguages)
                {
                    yield return getRequestUri(model, language);
                }
            }
            else
            {
                yield return getRequestUri(model, string.Empty);
            }
        }

        private Uri getRequestUri(TranslationRequestModel model, string languageCode = "")
        {
            var endpoint = azureTranslatorConnectionOptions.Endpoint;
            if (endpoint.EndsWith("/"))
            {
                endpoint = endpoint.Substring(0, endpoint.Length - 1);
            }

            var url = $"{endpoint}{TRANSLATOR_ROUTE}";

            if (string.IsNullOrWhiteSpace(languageCode))
            {
                url = $"{url}&to={String.Join("&to=", model.ToLanguages)}";
            }
            else
            {
                url = $"{url}&to={languageCode}";
            }

            if (!String.IsNullOrWhiteSpace(model.FromLanguage))
            {
                url = $"{url}&from={model.FromLanguage}";
            }

            if (!String.IsNullOrWhiteSpace(model.TextType) && !model.TextType.Equals(TextTypes.Plain, StringComparison.OrdinalIgnoreCase))
            {
                url = $"{url}&textType={model.TextType}";
            }

            if (!String.IsNullOrWhiteSpace(model.Category) && !model.Category.Equals(Categories.General, StringComparison.OrdinalIgnoreCase))
            {
                url = $"{url}&category={model.Category}";
            }

            if (!String.IsNullOrWhiteSpace(model.ProfanityAction) && !model.ProfanityAction.Equals(ProfanityActions.NoAction, StringComparison.OrdinalIgnoreCase))
            {
                url = $"{url}&profanityAction={model.ProfanityAction}";

                if (!String.IsNullOrWhiteSpace(model.ProfanityMarker) && !model.ProfanityMarker.Equals(ProfanityMarkers.Asterisk, StringComparison.OrdinalIgnoreCase))
                {
                    url = $"{url}&profanityMarker={model.ProfanityMarker}";
                }
            }

            var requestUri = new Uri(url);

            //logger.LogInformation($"{TAG} Request uri: {requestUri.ToString()}");

            return requestUri;
        }

        private void ValidateAzureTranslateConnectionOptions()
        {
            var errorMessage = new List<string>();

            if (string.IsNullOrWhiteSpace(azureTranslatorConnectionOptions.Endpoint))
            {
                errorMessage.Add($"{nameof(AzureTranslatorConnectionOptions.Endpoint)} is required");
            }

            if (string.IsNullOrWhiteSpace(azureTranslatorConnectionOptions.Region))
            {
                errorMessage.Add($"{nameof(AzureTranslatorConnectionOptions.Region)} is required");
            }

            if (string.IsNullOrWhiteSpace(azureTranslatorConnectionOptions.SubscriptionKey))
            {
                errorMessage.Add($"{nameof(AzureTranslatorConnectionOptions.SubscriptionKey)} is required");
            }

            if (errorMessage.Count > 0)
            {
                logger.LogInformation($"{TAG} {nameof(AzureTranslatorConnectionOptions)} is invalid.");
                throw new OptionsValidationException(AzureTranslatorConnectionOptions.Name, typeof(AzureTranslatorConnectionOptions), errorMessage.ToArray());
            }
        }

        private void ValidateRequestbody(TranslationRequestModel model, bool each = false)
        {
            var errorMessage = new List<string>();

            var inputsCount = model.Inputs.Count();

            if (inputsCount == 0)
            {
                errorMessage.Add("Text to translate is required.");
            }

            if (inputsCount > 100)
            {
                errorMessage.Add("The array can have at most 100 elements.");
            }

            foreach (var input in model.Inputs)
            {
                // https://docs.microsoft.com/en-us/azure/cognitive-services/translator/request-limits#character-and-array-limits-per-request
                // Max 10,000 characters. 
                // Request to translate (+1) and Response to be translated ( + count of to translate languages)
                var contentLength = input.Text.Length * ((each ? 1 : model.ToLanguages.Count()) + 1);
                
                logger.LogInformation($"{TAG} Calculated characters={contentLength}");

                if (contentLength > 10000)
                {
                    errorMessage.Add("The entire text included in the request cannot exceed 10,000 characters including spaces.");
                    break;
                }
            }

            if (errorMessage.Count > 0)
            {
                logger.LogInformation($"{TAG} Request body is invalid.");
                throw new InvalidRequestException<IEnumerable<string>>("Request body is invalid.", errorMessage.ToArray());
            }
        }


        private readonly AzureTranslatorConnectionOptions azureTranslatorConnectionOptions;
        private readonly ILogger logger;
    }
}
