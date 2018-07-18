using System;

namespace TextMood
{
    static class CognitiveServicesConstants
    {
#error Missing API Key. Create an API Key at https://aka.ms/cognitive-services-text-analytics-api
        public const string TextSentimentAPIKey = "Your API Key";

#error Missing Azure Region for BaseUri, e.g. "westus". You can locate the Azure Region on the page of the TextAnalytics resource created in the Azure Portal.
        const string _azureRegion = "Text Analytics Azure Region";

        public static Uri BaseUri => new Uri($"https://{_azureRegion}.api.cognitive.microsoft.com/text/analytics/v2.0");
    }
}
