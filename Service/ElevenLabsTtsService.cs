using Newtonsoft.Json;
using System.Text;

public class ElevenLabsTtsService : ITtsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ElevenLabsTtsService(string apiKey)
    {
        _httpClient = new HttpClient();
        _apiKey = apiKey;
    }

    public async Task<byte[]> GetTextToSpeechAsync(string text)
    {
        var requestUrl = "https://api.elevenlabs.io/v1/text-to-speech/21m00Tcm4TlvDq8ikWAM";
        var requestBody = new
        {
            text = text,
            voice_settings = new
            {
                stability = 0.5,
                similarity_boost = 0.5
            }
        };
        var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Add("xi-api-key", _apiKey);

        var response = await _httpClient.PostAsync(requestUrl, requestContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get TTS from ElevenLabs: {response.StatusCode}");
        }
        return await response.Content.ReadAsByteArrayAsync();
    }
}