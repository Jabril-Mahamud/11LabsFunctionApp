public interface ITtsService
{
    Task<byte[]> GetTextToSpeechAsync(string text);
}