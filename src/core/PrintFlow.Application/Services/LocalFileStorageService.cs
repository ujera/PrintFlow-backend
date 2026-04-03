using Microsoft.Extensions.Logging;
using PrintFlow.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintFlow.Application.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadPath;
        private readonly ILogger<LocalFileStorageService> _logger;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg"
    };

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg"
    };

        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
        {
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(_uploadPath);
            _logger = logger;
        }

        public async Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            var extension = Path.GetExtension(fileName);

            if (!AllowedExtensions.Contains(extension))
                throw new Application.Exceptions.BadRequestException(
                    $"File type '{extension}' is not allowed. Allowed types: PNG, JPEG.");

            if (!AllowedContentTypes.Contains(contentType))
                throw new Application.Exceptions.BadRequestException(
                    $"Content type '{contentType}' is not allowed.");

            if (fileStream.Length > MaxFileSize)
                throw new Application.Exceptions.BadRequestException(
                    $"File size exceeds the maximum limit of {MaxFileSize / 1024 / 1024}MB.");

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(stream);

            var fileUrl = $"/uploads/{uniqueFileName}";

            _logger.LogInformation("File uploaded: {FileName} → {FileUrl} ({Size} bytes)",
                fileName, fileUrl, fileStream.Length);

            return new FileUploadResult
            {
                FileName = fileName,
                FileUrl = fileUrl,
                FileSize = fileStream.Length
            };
        }

        public Task<bool> DeleteAsync(string fileUrl)
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_uploadPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FileUrl}", fileUrl);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<(Stream Stream, string ContentType)?> GetAsync(string fileUrl)
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_uploadPath, fileName);

            if (!File.Exists(filePath))
                return Task.FromResult<(Stream, string)?>(null);

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };

            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Task.FromResult<(Stream, string)?>((stream, contentType));
        }
    }
}
