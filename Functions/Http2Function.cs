using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace _11LabsFunctionApp.Functions
{
    public class Http2Function
    {
        private readonly ILogger<Http2Function> _logger;
        private readonly ITtsService _ttsService;
        private readonly IBlobStorageService _blobStorageService;

        public Http2Function(ILogger<Http2Function> logger, ITtsService ttsService, IBlobStorageService blobStorageService)
        {
            _logger = logger;
            _ttsService = ttsService;
            _blobStorageService = blobStorageService;
        }

        [Function("Http2Function")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<TextToSpeechRequest>(requestBody);

            if (string.IsNullOrEmpty(data?.Text))
            {
                return new BadRequestObjectResult("Please pass a text in the request body.");
            }

            try
            {
                var audioBytes = await _ttsService.GetTextToSpeechAsync(data.Text);

                // Save to blob storage using the decoupled service
                string blobUrl = await _blobStorageService.UploadAudioAsync(audioBytes, "audio/mpeg");

                // Return the blob URL
                return new OkObjectResult(blobUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TTS request");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
