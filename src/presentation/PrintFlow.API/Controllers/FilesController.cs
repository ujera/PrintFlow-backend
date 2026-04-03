using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

[Authorize]
[Route("api/files")]
public class FilesController : BaseApiController
{
    private readonly IFileStorageService _fileStorageService;

    public FilesController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Upload a custom design file (PNG, JPEG). Max 10MB.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResult<object>.Fail("No file provided."));

        using var stream = file.OpenReadStream();
        var result = await _fileStorageService.UploadAsync(stream, file.FileName, file.ContentType);

        return Ok(ApiResult<FileUploadResult>.Ok(result, "File uploaded."));
    }

    /// <summary>
    /// Upload multiple custom design files. Max 10MB each.
    /// </summary>
    [HttpPost("upload-multiple")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadMultiple(List<IFormFile> files)
    {
        if (files is null || files.Count == 0)
            return BadRequest(ApiResult<object>.Fail("No files provided."));

        if (files.Count > 5)
            return BadRequest(ApiResult<object>.Fail("Maximum 5 files per upload."));

        var results = new List<FileUploadResult>();

        foreach (var file in files)
        {
            using var stream = file.OpenReadStream();
            var result = await _fileStorageService.UploadAsync(stream, file.FileName, file.ContentType);
            results.Add(result);
        }

        return Ok(ApiResult<List<FileUploadResult>>.Ok(results, $"{results.Count} files uploaded."));
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> Delete(string fileName)
    {
        var deleted = await _fileStorageService.DeleteAsync($"/uploads/{fileName}");

        if (!deleted)
            return NotFound(ApiResult<object>.Fail("File not found."));

        return Ok(ApiResult<bool>.Ok(true, "File deleted."));
    }
}