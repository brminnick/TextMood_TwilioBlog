# TextMood

Ever wondered about the mood of that text message you just received? Is it happy or angry? 

In this repo, we show how to leverage [Microsoft Cognitive Services](https://azure.microsoft.com/services/cognitive-services/?WT.mc_id=none-github-bramin), [Azure Functions](https://azure.microsoft.com/services/functions/?WT.mc_id=none-github-bramin), and [Twilio Web Hooks](https://www.twilio.com/docs/glossary/what-is-a-webhook)to automatically determine the sentiment of each text message.

## Architecture

![Architecture](https://user-images.githubusercontent.com/13558917/43020603-66513e1a-8c15-11e8-928f-878ce536fd86.png)

## Text Sentiment Analysis

[Azure's Sentiment Analysis](https://azure.microsoft.com/services/cognitive-services/text-analytics/?WT.mc_id=none-TwilioBlog-bramin) service powered by [Cognitive Services](https://azure.microsoft.com/services/cognitive-services/?WT.mc_id=none-TwilioBlog-bramin) is a REST API that allows me to upload text as a POST Request, and it uses artificial intelligence to give me a sentiment score between 0 and 1; scores close to 0 indicate negative sentiment, while scores close to 1 indicate positive sentiment.

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

Let's do this automatically for all text messages!

## Automatic Text Sentiment Analysis

With the power of [Twilio Webhooks](https://www.twilio.com/docs/glossary/what-is-a-webhook), we can forward all received text messages to an [Azure Function](https://azure.microsoft.com/services/functions/?WT.mc_id=none-TwilioBlog-bramin) to leverage the power of the [Sentiment Analysis API](https://azure.microsoft.com/services/cognitive-services/text-analytics/?WT.mc_id=none-TwilioBlog-bramin).

First, we'll need a Twilio phone number.

### 1. Create a Sentiment Analysis API Key

To use the Sentiment Analysis API, we'll need to first create an API Key using the Azure Portal.

1. Navigate to the [Azure Portal](https://ms.portal.azure.com/?WT.mc_id=none-TwilioBlog-bramin)
    - If you are new to Azure, use [this sign-up link](https://azure.microsoft.com/free/ai/?WT.mc_id=none-TwilioBlog-bramin) to receive a Free $200 credit

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

1. Clone or download the [TextMood_TwilioBlog](https://github.com/brminnick/TextMood_TwilioBlog/tree/master)
    - To clone the repo using git, open a cmd prompt and enter the following command:
        - `git clone https://github.com/brminnick/TextMood_TwilioBlog.git`
    -To download the repo use the following steps:
        - Click the link above to navigate to the GitHub repo in your browser
        - Click Clone or Download
        - Click Download Zip

![Download Zip](https://user-images.githubusercontent.com/13558917/43021037-13dfed6e-8c17-11e8-946e-baf30502104b.png)

2. After downloading/cloning the repo, locate `TextMood.sln`
3. Launch `TextMood.sln` in Visual Studio 
4. In the Visual Studio Solution Explorer, double-click on Constants\CognitiveServicesConstants.cs
5. In **CognitiveServicesConstants.cs**, for `TextSentimentAPIKey`, replace `Your API Key` with the Key from the TextAnalytics Cognitive Service resource created earlier
6. In **CognitiveServicesConstants.cs**, for `_cognitiveServicesEndpoint`, replace `Your Cognitive Services Endpoint` with the Enpoint from the TextAnalytics Cognitive Service resource created earlier

7. In the Visual Studio Solution Explorer, right-click the TextMood project
8. In the right-click menu, select **Publish**

![Right Click Publish](https://user-images.githubusercontent.com/13558917/43021578-fe004e56-8c18-11e8-97cc-3a79dd3fe2e5.png)

9. In the Publish window, select **Function App**
10. In the Publish window, select **Create New**
11. In the Public window, select **Publish**

![Publish Window](https://user-images.githubusercontent.com/13558917/43021579-fe1eaa7c-8c18-11e8-8545-4d5cb1a058ae.png)

12. In the **Create App Service** window, enter an App Name
    - **Note**: The app name needs to be unique because it will be used for the Function's url 
    - I recommend using TextMood[LastName]
        - In this example, I am using TextMoodMinnick

13. In the **Create App Service** window, select your subscription
14. In the **Create App Service** window, next to **Resource Group** select **New...**

![Create App Service](https://user-images.githubusercontent.com/13558917/43049737-ab7beb6e-8db1-11e8-8bd0-251194acfee5.png)

15. In the **New resource group name** window, enter TextMood
16. In the **New resource group name**, select **OK**

![Create App Service Group](https://user-images.githubusercontent.com/13558917/43049736-ab6769e6-8db1-11e8-89c9-c18b9d1c2bcf.png)

17. In the **Create App Service** window, next to **Hosting Plan**, select **New...**

![New Hosting Plan](https://user-images.githubusercontent.com/13558917/43049735-ab51eabc-8db1-11e8-94eb-7853331fad33.png)

18. In the **Configure Hosting Plan** window, enter a name for **App Service Plan**
    - I recommend using TextMood[LastName]Plan
        - In this example, I am using TextMoodMinnickPlan

19. In the **Configure Hosting Plan** window, select a **Location**
    - I recommend selecting the location closest to you

20. In the **Configure Hosting Plan** window, for the **Size**, select **Consumption**
21. In the **Configure Hosting Plan** window, select **OK**

![Configure Hosting Plan](https://user-images.githubusercontent.com/13558917/43049734-ab3d2028-8db1-11e8-889f-0cf28cd1a1f0.png)

22. In the **Create App Service** window, next to **Storage Account**, select **New...**

![New Storage Account](https://user-images.githubusercontent.com/13558917/43049733-ab26f24e-8db1-11e8-8421-ea121903e6c2.png)

23. In the **Storage Account** window, enter an **Account Name**
    - I recommend using textmood[LastName]
        - In this example, I am using textmoodminnick
24. In the **Storage Account** window, for the **Account Type**, select **Standard - Locally Redundant**
25. In the **Storage Account** window, select **OK**

![New Storage Account](https://user-images.githubusercontent.com/13558917/43049732-ab104c9c-8db1-11e8-8ea7-182837384c78.png)

26. In the **Create App Service** window, select **Create** 

![Create App Service](https://user-images.githubusercontent.com/13558917/43049731-aad2e488-8db1-11e8-8291-f8bf297ea281.png)

27. Stand by while the Azure Function is created

![Deploying App Service](https://user-images.githubusercontent.com/13558917/43049839-7b3710f8-8db3-11e8-9f79-ce24c77c90c1.png)

28. In the Visual Studio Solution Explorer, right-click the TextMood project
29. In the right-click menu, select **Publish**

![Right Click Publish](https://user-images.githubusercontent.com/13558917/43021578-fe004e56-8c18-11e8-97cc-3a79dd3fe2e5.png)

30. In the **Publish** window, select **Publish**

![Azure Functions Publish](https://user-images.githubusercontent.com/13558917/43050274-eb039328-8dba-11e8-8c2f-eacf72d8c9f0.png)

### 3. Copy Azure Functions Url

1. Navigate to the [Azure Portal](https://ms.portal.azure.com/?WT.mc_id=none-TwilioBlog-bramin)
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

### 4. Create Twilio Phone Number

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

### 5. Send a Text

1. Open a text messaging app and send a text message to your Twilio phone number

![Happy Text](https://user-images.githubusercontent.com/13558917/43050543-4ca5cbb0-8dbf-11e8-8c37-82752d265432.png)

# Resources

- [Azure Sentiment Analysis](https://azure.microsoft.com/services/cognitive-services/text-analytics/?WT.mc_id=none-TwilioBlog-bramin)
- [Azure Functions](https://azure.microsoft.com/services/functions/?WT.mc_id=none-TwilioBlog-bramin)
- [Twilio Webhooks](https://www.twilio.com/docs/glossary/what-is-a-webhook)
- [Cognitive Services](https://azure.microsoft.com/services/cognitive-services/?WT.mc_id=none-TwilioBlog-bramin)
