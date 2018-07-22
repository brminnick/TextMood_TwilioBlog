using System;

namespace TextMood
{
    static class CognitiveServicesConstants
    {
#error Missing API Key. Create an API Key at https://aka.ms/cognitive-services-text-analytics-api
        public const string TextSentimentAPIKey = "Your API Key";

#error Missing Endpoint for the Cognitive Servce.You can locate the Azure Region on the Overview page of the TextAnalytics resource created in the Azure Portal.
        const string _cognitiveServicesEndpoint = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0";

        public static Uri BaseUri => new Uri(_cognitiveServicesEndpoint);
    }
}
