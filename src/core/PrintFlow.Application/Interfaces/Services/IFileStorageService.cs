namespace PrintFlow.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteAsync(string fileUrl);
    Task<(Stream Stream, string ContentType)?> GetAsync(string fileUrl);
}

public class FileUploadResult
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
}