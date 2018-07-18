using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace TextMood
{
    public static class AnalyzeTextSentiment
    {
        [FunctionName(nameof(AnalyzeTextSentiment))]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage httpRequest, TraceWriter log)
        {
            log.Info("Text Message Received");

            log.Info("Parsing Request Message");
            var httpRequestBody = httpRequest.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            log.Info("Creating New Text Model");
            var textMessageBody = TwilioServices.GetTextMessageBody(httpRequestBody, log);

            log.Info("Retrieving Sentiment Score");
            var sentimentScore = await TextAnalysisServices.GetSentiment(textMessageBody) ?? -1;

            var response = $"Text Sentiment: {EmojiService.GetEmoji(sentimentScore)}";

            log.Info($"Sending OK Response: {response}");
            return new HttpResponseMessage { Content = new StringContent(TwilioServices.CreateTwilioResponse(response), Encoding.UTF8, "application/xml") };
        }
    }
}
