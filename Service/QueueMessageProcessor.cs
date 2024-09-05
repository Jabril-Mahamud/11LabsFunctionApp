using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class QueueMessageProcessor : IQueueMessageProcessor
{
    private readonly ILogger<QueueMessageProcessor> _logger;
    private readonly ITtsService _ttsService;
    private readonly IBlobStorageService _blobStorageService;

    public QueueMessageProcessor(
        ILogger<QueueMessageProcessor> logger,
        ITtsService ttsService,
        IBlobStorageService blobStorageService)
    {
        _logger = logger;
        _ttsService = ttsService;
        _blobStorageService = blobStorageService;
    }

    public async Task ProcessMessageAsync(QueueMessage message)
    {
        _logger.LogInformation($"Processing message: {message.MessageText}");

        try
        {
            // Deserialize message text into the request object
            var data = JsonConvert.DeserializeObject<TextToSpeechRequest>(message.MessageText);

            if (string.IsNullOrEmpty(data?.Text))
            {
                _logger.LogWarning("Message does not contain valid text.");
                return; // Exit if the message does not have valid text
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
            // Handle deserialization errors
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing queue message.");
            // Handle general errors
        }
    }
}