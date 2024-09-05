using System;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Newtonsoft.Json;


namespace _11LabsFunctionApp.Functions;

public class QFunction
{
    private readonly ILogger<QFunction> _logger;
    private readonly ITtsService _ttsService;
    private readonly IBlobStorageService _blobStorageService;

    public QFunction(ILogger<QFunction> logger, ITtsService ttsService, IBlobStorageService blobStorageService)
    {
        _logger = logger;
        _blobStorageService = blobStorageService;
        _ttsService = ttsService;
    }

    [Function(nameof(QFunction))]
    public async Task RunAsync([QueueTrigger("myqueue-items", Connection = "ConnectionString")] QueueMessage message)
    {
        _logger.LogInformation($"C# Queue trigger function received message: {message.MessageText}");

        try
        {
            // Deserialize message text into the request object
            var data = JsonConvert.DeserializeObject<TextToSpeechRequest>(message.MessageText);

            if (string.IsNullOrEmpty(data?.Text))
            {
                _logger.LogWarning("The message does not contain valid text.");
                return; // Exit the function if the text is invalid
            }

            // Get audio bytes from TTS service
            var audioBytes = await _ttsService.GetTextToSpeechAsync(data.Text);

            // Upload to blob storage
            string blobUrl = await _blobStorageService.UploadAudioAsync(audioBytes, "audio/mpeg");

            _logger.LogInformation($"Audio file uploaded successfully. Blob URL: {blobUrl}");
        }
        catch (JsonSerializationException jsonEx)
        {
            _logger.LogError(jsonEx, "Error deserializing queue message.");
            // Consider handling deserialization errors specifically
        }
        catch (ServiceException serviceEx) // Replace with actual exceptions thrown by TTS and Blob services
        {
            _logger.LogError(serviceEx, "Error with TTS or Blob storage service.");
            // Consider implementing retry logic or sending to a dead-letter queue
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing queue message.");
            // General exception handling
        }
    }
}
