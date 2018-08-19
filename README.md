# TextMood

## Automatic Text Sentiment Analysis

With the power of [Twilio Webhooks](https://www.twilio.com/docs/glossary/what-is-a-webhook), we can forward all received text messages to an [Azure Function](https://azure.microsoft.com/services/functions/?WT.mc_id=none-TwilioBlog-bramin) to leverage the power of the [Sentiment Analysis API](https://azure.microsoft.com/services/cognitive-services/text-analytics/?WT.mc_id=none-TwilioBlog-bramin).

The completed solution can be found here: https://github.com/brminnick/TextMood_TwilioBlog

![Architecture Diagram](https://user-images.githubusercontent.com/13558917/43020603-66513e1a-8c15-11e8-928f-878ce536fd86.png)

### POST Request

```json
{
  "documents": [
    {
      "language": "en",
      "id": "251c99d7-1f89-426a-a3ad-c6fa1b34f020",
      "text": "I hope you find time to actually get your reports done today."
    }
  ]
}
```

### Response

```json
{
"sentiment": {
  "documents": [
    {
      "id": "251c99d7-1f89-426a-a3ad-c6fa1b34f020",
      "score": 0.776355504989624
    }
  ]
}
```

### 1. Create a Sentiment Analysis API Key

To use the Sentiment Analysis API, we'll need to first create an API Key using the Azure Portal.

1. Navigate to the [Azure Portal](https://portal.azure.com/?WT.mc_id=none-TwilioBlog-bramin)
    - If you are new to Azure, use [this sign-up link](https://azure.microsoft.com/free/ai/?WT.mc_id=none-TwilioBlog-bramin) to receive a free $200 credit

2. On the Azure Portal, select **+ Create a Resource**
3. In the **New** window, select **AI + Machine Learning**
4. In the **Featured** frame, select **Text Analytics**
5. In the **Create** window, make the following selections
    - **Name**: TextMood
    - **Subscription**: [Select your Azure subscription]
    - **Location**: [Select the location closest to you]
    - **Pricing Tier**: F0 (5K Transactions per 30 days)
        - This is a free tier
    - **Resource Group**: TextMood
6. In the **Create** window, select **Create**
7. On the Azure Portal, select the bell-shaped notification icon
8. Stand by while the **Notifications** window says **Deployment in progress...**
9. Once the deployment has finished, on the **Notifications** window, select **Go to resource**
10. In the TextMood Resource page, select **Keys** and locate **KEY 1**
    - We will use this API Key when we create our Azure Function

11. In the TextMood Resource page, select **Overview** and locate the **Endpoint**
    - We will use this Url when we create our Azure Function

### 2. Create an Azure Function

[Azure Functions](https://azure.microsoft.com/services/functions/?WT.mc_id=none-TwilioBlog-bramin) are a serverless offering in Azure. In these steps, we will use Azure Functions to create a POST API.

1. On a PC, open [Visual Studio](https://visualstudio.microsoft.com/?WT.mc_id=none-TwilioBlog-bramin)
   - I am using [Visual Studio 2017](https://visualstudio.microsoft.com/vs/?WT.mc_id=none-TwilioBlog-bramin)

2. In Visual Studio, select File -> New -> Project

![File New Project](https://user-images.githubusercontent.com/13558917/44055253-a87f15ea-9ef9-11e8-8c68-b3869a842886.png)

3. In the **New Project** window, type **Function** into the search bar
4. In the **New Project** window, select **Azure Functions**
5. In the **New Project** window, change the project **Name** to **TextMood**
6. In the **New Project** window, select **OK**

![New Azure Function](https://user-images.githubusercontent.com/13558917/44055505-6e28bc6a-9efa-11e8-9e10-3ed99272e249.png)

7. In the **New Project - TextMood** window, select the following options
   - **Azure Functions v1 (.NET Framework)**
   - **Http Trigger**
   - **Storage Account**: Storage Emulator
   - **Access Rights**: Function

8. In the **New Project - TextMood** window, select **OK**

![Azure Function Setup](https://user-images.githubusercontent.com/13558917/44056404-fda59be0-9efc-11e8-9461-f2397d7fd33f.png)

9. In the **Solution Explorer**, right-click on the **TextMood** project -> **Unload Project**

![Unload Project](https://user-images.githubusercontent.com/13558917/44056895-841fdc02-9efe-11e8-997f-e118d359f8cc.png)

10. In the **Solution Explorer**, right-click on the **TextMood** project -> **Edit TextMood.csproj**

![Edit CSProj](https://user-images.githubusercontent.com/13558917/44057516-4d3e0a22-9f00-11e8-962f-aba2a7204af0.png)

11. In the **TextModd.csproj** window, update the **PackgeReference**s in the to include `Microsoft.NET.Sdk.Functions`, `Twilio` and `Microsoft.Azure.CognitiveServices.Language.TextAnalytics`
    - Note: `Microsoft.NET.Sdk.Functions` likely already exists. Ensure it's using `Version="1.0.14"`

```csharp
<ItemGroup>
    <PackageReference Include="Microsoft.NET.Sdk.Functions" Version="1.0.14" />
    <PackageReference Include="Twilio" Version="5.14.0" />
    <PackageReference Include="Microsoft.Azure.CognitiveServices.Language.TextAnalytics" Version="2.0.0-preview" />
</ItemGroup>
```

12. Save and Close the **TextModd.csproj** window

13. In the **Solution Explorer**, right-click on the **TextMood** project -> **Reload Project**
    - If a confirmation popup appears, select **OK**

14. In the **Solution Explorer**, double-click on **Function1.cs**
15. Copy the below code into the **Function1.cs** window, overwriting all existing code which was auto-generated by the template

```csharp
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Twilio.TwiML;

namespace TextMood
{
    public static class AnalyzeTextSentiment
    {
        const string _sadFaceEmoji = "☹️";
        const string _neutralFaceEmoji = "\U0001F610";
        const string _happyFaceEmoji = "\U0001F603";
        const string _blankFaceEmoji = "\U0001F636";

        const string _textSentimentAPIKey = "Your API Key";
        const string _cognitiveServicesEndpoint = "Your Cognitive Services API Endpoint";

        readonly static TextAnalyticsClient _textAnalyticsApiClient = new TextAnalyticsClient(new ApiKeyServiceClientCredentials(_textSentimentAPIKey))
        {
            BaseUri = new Uri(_cognitiveServicesEndpoint)
        };

        [FunctionName(nameof(AnalyzeTextSentiment))]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage httpRequest, TraceWriter log)
        {
            log.Info("Text Message Received");

            log.Info("Parsing Request Message");
            var httpRequestBody = await httpRequest.Content.ReadAsStringAsync();

            log.Info("Creating New Text Model");
            var textMessageBody = GetTextMessageBody(httpRequestBody, log);

            log.Info("Retrieving Sentiment Score");
            var sentimentScore = await GetSentiment(textMessageBody) ?? -1;

            var response = $"Text Sentiment: {GetEmoji(sentimentScore)}";

            log.Info($"Sending OK Response: {response}");
            return new HttpResponseMessage { Content = new StringContent(CreateTwilioResponse(response), Encoding.UTF8, "application/xml") };
        }

        static string GetTextMessageBody(string httpRequestBody, TraceWriter log)
        {
            var formValues = httpRequestBody?.Split('&')
                                ?.Select(value => value.Split('='))
                                ?.ToDictionary(pair => Uri.UnescapeDataString(pair[0]).Replace("+", " "),
                                              pair => Uri.UnescapeDataString(pair[1]).Replace("+", " "));

            foreach (var value in formValues)
                log.Info($"Key: {value.Key}, Value: {value.Value}");

            var textMessageKeyValuePair = formValues.Where(x => x.Key?.ToUpper()?.Equals("BODY") ?? false)?.FirstOrDefault();

            return textMessageKeyValuePair?.Value;
        }

        static string CreateTwilioResponse(string message)
        {
            var response = new MessagingResponse().Message(message);

            return response.ToString().Replace("utf-16", "utf-8");
        }

        static async Task<double?> GetSentiment(string text)
        {
            var sentimentDocument = new MultiLanguageBatchInput(new List<MultiLanguageInput> { { new MultiLanguageInput(id: "1", text: text) } });

            var sentimentResults = await _textAnalyticsApiClient.SentimentAsync(sentimentDocument).ConfigureAwait(false);

            if (sentimentResults?.Errors?.Any() ?? false)
            {
                var exceptionList = sentimentResults.Errors.Select(x => new Exception($"Id: {x.Id}, Message: {x.Message}"));
                throw new AggregateException(exceptionList);
            }

            var documentResult = sentimentResults?.Documents?.FirstOrDefault();

            return documentResult?.Score;
        }

        static string GetEmoji(double? sentimentScore)
        {
            switch (sentimentScore)
            {
                case double number when (number >= 0 && number < 0.4):
                    return _sadFaceEmoji;
                case double number when (number >= 0.4 && number <= 0.6):
                    return _neutralFaceEmoji;
                case double number when (number > 0.6):
                    return _happyFaceEmoji;
                case null:
                    return _blankFaceEmoji;
                default:
                    return string.Empty;
            }
        }

        class ApiKeyServiceClientCredentials : ServiceClientCredentials
        {
            readonly string _subscriptionKey;

            public ApiKeyServiceClientCredentials(string subscriptionKey) => _subscriptionKey = subscriptionKey;

            public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request is null)
                    throw new ArgumentNullException(nameof(request));

                request.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);

                return Task.CompletedTask;
            }
        }
    }
}
```

16. In the **Function1.cs** window, for `_textSentimentAPIKey`, replace `Your API Key` with the Key from the TextAnalytics Cognitive Service resource created e6arlier
17. In the **Function1.cs** window, for `_cognitiveServicesEndpoint`, replace `Your Cognitive Services Endpoint` with the Endpoint from the TextAnalytics Cognitive Service resource created earlier

## 3. Publish the Azure Function to the Cloud

1. In the Visual Studio Solution Explorer, right-click the TextMood project
2. In the right-click menu, select **Publish**

![Right Click Publish](https://user-images.githubusercontent.com/13558917/43021578-fe004e56-8c18-11e8-97cc-3a79dd3fe2e5.png)

3. In the Publish window, select **Function App**
4. In the Publish window, select **Create New**
5. In the Public window, select **Publish**

![Publish Window](https://user-images.githubusercontent.com/13558917/43021579-fe1eaa7c-8c18-11e8-8545-4d5cb1a058ae.png)

6. In the **Create App Service** window, enter an App Name
    - **Note**: The app name needs to be unique because it will be used for the Function's url 
    - I recommend using TextMood[LastName]
        - In this example, I am using TextMoodMinnick

7. In the **Create App Service** window, select your subscription
8. In the **Create App Service** window, next to **Resource Group** select **New...**

![Create App Service](https://user-images.githubusercontent.com/13558917/43049737-ab7beb6e-8db1-11e8-8bd0-251194acfee5.png)

9. In the **New resource group name** window, enter TextMood
10. In the **New resource group name**, select **OK**

![Create App Service Group](https://user-images.githubusercontent.com/13558917/43049736-ab6769e6-8db1-11e8-89c9-c18b9d1c2bcf.png)

11. In the **Create App Service** window, next to **Hosting Plan**, select **New...**

![New Hosting Plan](https://user-images.githubusercontent.com/13558917/43049735-ab51eabc-8db1-11e8-94eb-7853331fad33.png)

12. In the **Configure Hosting Plan** window, enter a name for **App Service Plan**
    - I recommend using TextMood[LastName]Plan
        - In this example, I am using TextMoodMinnickPlan

13. In the **Configure Hosting Plan** window, select a **Location**
    - I recommend selecting the location closest to you

14. In the **Configure Hosting Plan** window, for the **Size**, select **Consumption**
15. In the **Configure Hosting Plan** window, select **OK**

![Configure Hosting Plan](https://user-images.githubusercontent.com/13558917/43049734-ab3d2028-8db1-11e8-889f-0cf28cd1a1f0.png)

16. In the **Create App Service** window, next to **Storage Account**, select **New...**

![New Storage Account](https://user-images.githubusercontent.com/13558917/43049733-ab26f24e-8db1-11e8-8421-ea121903e6c2.png)

17. In the **Storage Account** window, enter an **Account Name**
    - I recommend using textmood[LastName]
        - In this example, I am using textmoodminnick
18. In the **Storage Account** window, for the **Account Type**, select **Standard - Locally Redundant**
19. In the **Storage Account** window, select **OK**

![New Storage Account](https://user-images.githubusercontent.com/13558917/43049732-ab104c9c-8db1-11e8-8ea7-182837384c78.png)

20. In the **Create App Service** window, select **Create** 

![Create App Service](https://user-images.githubusercontent.com/13558917/43049731-aad2e488-8db1-11e8-8291-f8bf297ea281.png)

21. Stand by while the Azure Function is created

![Deploying App Service](https://user-images.githubusercontent.com/13558917/43049839-7b3710f8-8db3-11e8-9f79-ce24c77c90c1.png)

22. In the Visual Studio Solution Explorer, right-click the TextMood project
23. In the right-click menu, select **Publish**

![Right Click Publish](https://user-images.githubusercontent.com/13558917/43021578-fe004e56-8c18-11e8-97cc-3a79dd3fe2e5.png)

24. In the **Publish** window, select **Publish**

![Azure Functions Publish](https://user-images.githubusercontent.com/13558917/43050274-eb039328-8dba-11e8-8c2f-eacf72d8c9f0.png)

### 4. Copy Azure Functions Url

1. Navigate to the [Azure Portal](https://portal.azure.com/?WT.mc_id=none-TwilioBlog-bramin)
2. On the Azure Portal, select **Resource Groups**
3. In the **Resource Group** window, select **TextMood**

![Resource Group TextMood](https://user-images.githubusercontent.com/13558917/43050243-4b24c692-8dba-11e8-8ab3-16656077a73f.png)

4. In the TextMood Resource Group window, select **Overview**
5. In the TextMood Overview window, select the Azure Function resource, **TextMood[LastName]**

![Open Functions](https://user-images.githubusercontent.com/13558917/43050244-4b3a2802-8dba-11e8-9a90-4467470ac2fa.png)

6. In the **Azure Functions** window, select **AnalyzeTextSentiment**
7. In the **AnalyzeSentiment** window, select **Get Function Url**
    - We will use this Url when we create our Twilio phone number

![Get Function Url](https://user-images.githubusercontent.com/13558917/43050298-6d28cd46-8dbb-11e8-9442-b1dfa85ada24.png)

### 5. Create Twilio Phone Number

1. Log into your Twilio account and navigate to the [Manage Numbers](https://www.twilio.com/console/phone-numbers/incoming) page. 

2. Click the **+** sign to buy a new number
    - Note you may skip this step if you have an existing Twilio Phone Number

![Buy a new number](https://user-images.githubusercontent.com/13558917/43050486-3d85fd54-8dbe-11e8-82aa-4cd94737a05f.png)

3. On the **Buy a Number** page, select a **Country**
    - For this example, I am using United States

4. On the **Buy a Number** page, tap the **Number** drop down and change it to **Location**
5. On the **Buy a Number** page, enter a location name
    - For this example, I am using San Francisco

6. On the **Buy a Number** page, check **SMS**
7. On the **Buy a Number** page, click **Search**

![Buy a new number page](https://user-images.githubusercontent.com/13558917/43050472-20250a98-8dbe-11e8-88e9-d059dac13cca.png)

8. On the search results page, locate the number you'd like to purchase and select **Buy**

![Buy Number](https://user-images.githubusercontent.com/13558917/43050469-1fe11158-8dbe-11e8-982d-3f608ad066c7.png)

9. In the confirmation window, select **Buy This Number**

![Buy This Number](https://user-images.githubusercontent.com/13558917/43050470-1ff6197c-8dbe-11e8-8364-9cafe5f0b9ca.png)

10. In the purchase confirmation window, select **Setup Number**

![Setup Number](https://user-images.githubusercontent.com/13558917/43050471-200b6ff2-8dbe-11e8-8187-df1a964399f9.png)

11. In the **Messaging** section, next to **A Message Comes In**, select **Webhook**
12. In the **Messaging** section, next to **A Message Comes In**, enter the Azure Function Url that we created in the previous section
13. In the **Messaging** section, next to **A Message Comes In**, select **HTTP POST**
14. In the **Manage Numbers** window, select **Save**

![Messaging](https://user-images.githubusercontent.com/13558917/43158699-2d545b7e-8f35-11e8-8e47-d120024aa948.png)

### 6. Send a Text

1. Open a text messaging app and send a text message to your Twilio phone number

![Happy Text](https://user-images.githubusercontent.com/13558917/43050543-4ca5cbb0-8dbf-11e8-8c37-82752d265432.png)

# Resources

- [Azure Sentiment Analysis](https://azure.microsoft.com/services/cognitive-services/text-analytics/?WT.mc_id=none-TwilioBlog-bramin)
- [Azure Functions](https://azure.microsoft.com/services/functions/?WT.mc_id=none-TwilioBlog-bramin)
- [Twilio Webhooks](https://www.twilio.com/docs/glossary/what-is-a-webhook)
- [Cognitive Services](https://azure.microsoft.com/services/cognitive-services/?WT.mc_id=none-TwilioBlog-bramin)
