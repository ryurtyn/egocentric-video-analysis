
## Introduction

Egocentric videos, or videos captured from a first-person perspective, provide a unique view that mimics the human visual experience, making it suitable for tutorials and step-by-step guides in a wide range of domains, from cooking and crafting to surgical procedures. With technological advancements in head-mounted video recording devices, such as the Meta Ray-Ban Smart Glasses, capturing egocentric videos has become increasingly accessible for the general public. This accessibility paves the way for innovative applications of such videos.

This project explores the potential of egocentric videos in instructional contexts by developing a tool that can extract detailed procedural information. The tool is designed to identify specific actions and sequences within a video, contextualizing this information in a manner that is both digestible and accessible to users. Utilizing cutting-edge machine learning models and computer vision techniques, the tool processes video content and presents it alongside the original footage, thereby enhancing the learning experience. This prototype aims to transform traditional learning environments into interactive, user-focused platforms. 


## Project Structure

The prototype is built with the following structure in mind: an instructor will record a video while wearing a head-mounted recording device, like the Meta Ray-Ban Smart Glasses. After the instruction session, they will upload the video to the web portal where it will be sent to process. After processing, the video is displayed in a list with previously uploaded videos. A student or learner can then access this list of videos and view the details of one of the videos. The detailed view will show some of the extracted information including a list of procedural tasks, key words, and the video itself, with the ability to scrub through the video to find the moment a specific task is shown. 

This project is built using a suite of Microsoft Azure services. User interactions are facilitated on a Blazor webapp. Background processing is run on an Azure Function. 

![workflow](images/Pasted%20image%2020240514163926.png)
<!-- ![[images/Pasted image 20240514163926.png]] -->

### Blazor WebApp
The Blazor WebApp handles user inputs and interactions. It has three public pages: a home page, upload, and view.

#### Upload Page 
The upload page allows a user to upload a new video. The selected video is uploaded to an Azure storage container in Blob Storage. Each new upload will trigger the background Azure function "ProcessVideoFile". 

#### View Page 
The view page is where a student would navigate to view the processed instructional content. The first view of this page is a list of all uploaded videos. The user can then select a single video and is redirected to view the information for that specific video.

### Azure Function
The Azure Function handles background processing. It watches for new videos to be uploaded to the specified Blob Storage Container. When a file is uploaded, it retrieves that file and generates metadata for the file. This metadata is then saved in a different Blob Storage Container for future retrieval. 

## Technologies Used
This project leverages a robust stack of technologies, frameworks, and services to deliver a comprehensive solution for egocentric video analysis:

- **.NET Blazor**: Utilized for building the interactive web front-end. Blazor allows for the development of dynamic web applications using C# instead of JavaScript.
- **Azure WebApp**: Hosts the Blazor web application, providing web hosting service on the Microsoft Azure cloud platform.
- **Azure Functions**: Used for serverless computing, enabling execution of code in response to events, which efficiently handles backend processes like API requests.
- **Azure Blob Storage**: Used for cloud storage of video files and result data that are processed and analyzed by the application.
- **Azure Video Indexer API**: Integrates advanced media analytics capabilities, facilitating the extraction of insights from video files automatically.
- **Azure Language Service API**: Provides AI-powered language understanding capabilities, which are used to analyze and interpret user interactions and video content.
- **OpenAI API**: Leverages state-of-the-art machine learning models to perform natural language processing and extract information from video content. 

## Getting Started

### Prerequisites

- .NET SDK
- Azure CLI 
- Azure Account 
- Visual Studio 

### Installation

To begin, clone the project from github: https://github.com/ryurtyn/egocentric-video-analysis

#### Creating Azure Cloud Resources 

##### Azure Resource Group
Create a new resource group in the Azure portal. **All other resources will be assigned to this resource group.**

##### Azure Web App
Create a new Azure Web App in the Azure Portal. This will hold all pages for user interaction. 

##### Azure Storage Account
You will need to create two Azure storage accounts. One will hold the input video files, and the other will hold the processed data. 
###### Input Video Files Storage Account: V1 Storage Account
To have full functionality, the storage account for the input videos should be a V1 storage account. This will allow scrubbing the video in Chrome. A V2 storage account will work fine if you use Safari as a web browser. Make sure to enable "anonymous blob access" and to change the access level to "container"

Using the Azure CLI, run: 
- `az storage account create --name [account_name] --resource-group [your-resource-group-name] --location eastus --sku Standard_LRS --kind Storage --allow-blob-public-access true`
- `az storage account blob-service-properties update --account-name [account_name] --default-service-version '2020-10-02'` 

Reference Link: [Create a storage account](https://learn.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-cli#code-try-11)

You should then make a container inside this storage account. This will be used in configurations as <your_input_video_file_container_name>

###### Result Data Files Storage Account
This storage account can be created in the Azure portal as usual. Make sure to enable "anonymous blob access" and to change the access level to "container".

You should then make a container inside this storage account. This will be used in configurations as <your_data_file_container_name>

##### Azure Function App
Create an Azure Function. Link it to the Input Video Files storage account from the previous step. 

##### Azure Video Indexer
Create an Azure Video Indexer Resource. 

##### Azure Language Service 
Create an Azure Language Service Resource.

##### Open AI Account
Ensure you have an OpenAI account with API access. Make sure the account has available credit. 


## Configuration

Web App configuration file: appsettings.json
 
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureBlobStorageInputVideoFiles": {
    "ConnectionString": "<your_input_video_file_storage_connection_string>",
    "ContainerName": "<your_input_video_file_container_name>"
  },
  "AzureBlobStorageDataFiles": {
    "ConnectionString":  "<your_data_file_connection_string>",
    "ContainerName":  "<your_data_file_container_name>"
  },
  "VideoIndexer": {
    "AccessToken": "<your_video_indexer_access_token>",
    "AccountId": "<your_video_indexer_account_id>",
    "Location": "<your_video_indexer_account_location>",
    "SubscriptionKey": "<your_video_indexer_subscription_key>"
  },
  "LanguageAI": {
    "SubscriptionKey": "<your_language_ai_subscription_key>"
  },
  "OpenAI": {
    "APIKey": "<your_openAI_api_key>"
  }
}
```

Azure Function configuration file: local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "VideoIndexerApiKey": "<your_video_indexer_access_token>",
    "VideoIndexerAccountId": "<your_video_indexer_account_id>",
    "VideoIndexerSubscriptionId": "<your_video_indexer_subscription_id>",
    "VideoIndexerAccountName": "<your_video_indexer_account_name>",
    "VideoIndexerResourceGroup": "<your_resource_group>",
    "VideoIndexerLocation": "<your_video_indexer_account_location>",
    "BearerAuthKey": "<your_video_indexer_bearer_auth_key>",
    "VideoIndexerSubscriptionKey": "<your_video_indexer_subscription_key>",
    "AzureBlobStorageInputVideoFilesConnectionString": "<your_input_video_file_storage_connection_string>",
    "AzureBLobStorageInputVideoFilesContainerName": "<your_input_video_file_storage_container_name>",
    "AzureBlobStorageDataFilesConnectionString": "<your_data_file_connection_string>",
    "AzureBlobStorageDataFilesContainerName": "<your_data_file_container_name>"
  },
  "Host": {
    "LocalHttpPort": 7071,
    "CORS": "*",
    "CORSCredentials": false
  }
}
```

### Video Indexer Configurations

#### Video Indexer Access Token 
There are two types of video indexer accounts. In this case, the prototype was built with an ARM video indexer account. As a result, the process for getting an access token is a bit more complicated. Additionally, the access tokens expire once an hour, and must be added in the configs upon expiry. 

To generate an access token, go to [Generate - Access Token](https://learn.microsoft.com/en-us/rest/api/videoindexer/generate/access-token?view=rest-videoindexer-2024-01-01&tabs=HTTP#code-try-0)

At this link, press "try it" and fill in necessary video indexer information. Make sure the body includes 
```
{
	permissionType: "Contributor",
	scope: "Account"
}
```

The Request Preview will show this structure: 
```
POST https://management.azure.com/subscriptions/2a2133fc-9811-4822-bf15-8c3c74f5973c/resourceGroups/hello-rusy-resource-group/providers/Microsoft.VideoIndexer/accounts/videoIndexerTester/generateAccessToken?api-version=2024-01-01
Authorization: Bearer <auth key>
Content-type: application/json
```

The <your_video_indexer_bearer_auth_key> configuration must contain the entirety of the Authorization heading including the word Bearer. 

The <your_video_indexer_access_token> should contain the resulting output access token that is outputted by the API call. 

These tokens will be valid for 1 hour. 

See more information here: [How can i access API Key of the video indexer? - Microsoft Q&A](https://learn.microsoft.com/en-us/answers/questions/855740/how-can-i-access-api-key-of-the-video-indexer) 

#### Video Indexer Subscription Key 
The video indexer subscription key can be found at the following link: [Home - Microsoft Azure API Management - developer portal](https://api-portal.videoindexer.ai/)

Navigate to the Profile tab and view the "Primary Key". This is <your_video_indexer_subscription_key>

#### Video Indexer Subscription Id
The video indexer subscription id can be found in the Azure portal in the resource overview section. 

## API References
Video Indexer ARM API: [Generate - Access Token - REST API (Azure Azure Video Indexer) | Microsoft Learn](https://learn.microsoft.com/en-us/rest/api/videoindexer/generate/access-token?view=rest-videoindexer-2024-01-01&tabs=HTTP#code-try-0)

Video Indexer API: [APIs: Details - Microsoft Azure API Management - developer portal](https://api-portal.videoindexer.ai/api-details#api=Operations&operation=Get-Account-Access-Token)

Language Service API: [Summarize text with the conversation summarization API - Azure AI services | Microsoft Learn](https://learn.microsoft.com/en-us/azure/ai-services/language-service/summarization/how-to/conversation-summarization#get-chapter-titles)

OpenAI API: [platform.openai.com/docs/api-reference](https://platform.openai.com/docs/api-reference) 

## Deployment
### Local Testing 
The webapp can be run locally through the visual studio interface. To run the Azure Function locally, run `azurite --silent --location .\azurite --debug .\azurite\debug.log` from within the azure function directory. 

### Cloud Deployment 
Both the web app and the azure function can be deployed through the visual studio interface. 

## Acknowledgment
This project was an independent research project in collaboration with the [Cornell Tech XR Collaboratory](https://xrcollaboratory.tech.cornell.edu/)

## FAQs

## Contact Information
Ruslana Yurtyn 
rny4@cornell.edu 