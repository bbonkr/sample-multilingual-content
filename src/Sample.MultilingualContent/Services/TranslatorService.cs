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
        Task<TranslationResultModel[]> TranslateAsync(TranslationRequestModel model);
    }

    public class TranslatorService : ITranslatorService
    {
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

        public async Task<TranslationResultModel[]> TranslateAsync(TranslationRequestModel model)
        {
            ValidateAzureTranslateConnectionOptions();

            var requestBody = model.Inputs.ToJson();

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = getRequestUri(model.TranslateToLanguages);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, CONTENT_TYPE_VALUE);
                    request.Headers.Add(OCP_APIM_SUBSCRIPTION_KEY, azureTranslatorConnectionOptions.SubscriptionKey);
                    request.Headers.Add(OCP_APIM_SUBSCRIPTION_REGION, azureTranslatorConnectionOptions.Region);
                    //request.Headers.Add(CONTENT_TYPE_KEY, CONTENT_TYPE_VALUE);

                    var response = await client.SendAsync(request);//.ConfigureAwait(false);

                    var result = await response.Content.ReadAsStringAsync();

                    var resultModel = JsonSerializer.Deserialize<TranslationResultModel[]>(result, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    });

                    return resultModel;
                }
            }
        }

        private Uri getRequestUri(string[] translateToLanguages)
        {
            var endpoint = azureTranslatorConnectionOptions.Endpoint;
            if (endpoint.EndsWith("/"))
            {
                endpoint = endpoint.Substring(0, endpoint.Length - 1);
            }

            var requestUri = new Uri($"{endpoint}{TRANSLATOR_ROUTE}&to={String.Join("&to=", translateToLanguages)}");

            logger.LogInformation($"Request uri: {requestUri.ToString()}");

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
                throw new OptionsValidationException(AzureTranslatorConnectionOptions.Name, typeof(AzureTranslatorConnectionOptions), errorMessage.ToArray());
            }
        }

        private readonly AzureTranslatorConnectionOptions azureTranslatorConnectionOptions;
        private readonly ILogger logger;
    }
}
