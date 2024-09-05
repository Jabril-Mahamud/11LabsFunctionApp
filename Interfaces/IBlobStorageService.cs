public interface IBlobStorageService
{
    Task<string> UploadAudioAsync(byte[] audioData, string contentType);
}