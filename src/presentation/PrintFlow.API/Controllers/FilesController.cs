using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintFlow.Application.DTOs.Common;
using PrintFlow.Application.Interfaces.Services;

namespace PrintFlow.API.Controllers;

/// <summary>
/// File upload for custom product designs (PNG, JPEG)
/// </summary>
[Authorize]
[Route("api/files")]
[Produces("application/json")]
public class FilesController : BaseApiController
{
    private readonly IFileStorageService _fileStorageService;

    public FilesController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Upload a custom design file
    /// </summary>
    /// <remarks>
    /// Accepts PNG and JPEG files up to 10MB.
    /// Returns a fileUrl that can be used when adding items to cart.
    /// 
    /// Example flow:
    /// 1. Upload file → get fileUrl
    /// 2. Add to cart with uploadFileUrl = fileUrl
    /// </remarks>
    /// <param name="file">Image file (PNG or JPEG, max 10MB)</param>
    /// <returns>Upload result with file URL</returns>
    /// <response code="200">File uploaded successfully</response>
    /// <response code="400">Invalid file type, too large, or no file provided</response>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResult<FileUploadResult>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResult<object>.Fail("No file provided."));

        using var stream = file.OpenReadStream();
        var result = await _fileStorageService.UploadAsync(stream, file.FileName, file.ContentType);

        return Ok(ApiResult<FileUploadResult>.Ok(result, "File uploaded."));
    }

    /// <summary>
    /// Upload multiple custom design files
    /// </summary>
    /// <remarks>
    /// Accepts up to 5 files per request. Each file must be PNG or JPEG, max 10MB.
    /// </remarks>
    /// <param name="files">Image files (PNG or JPEG)</param>
    /// <returns>Upload results with file URLs</returns>
    /// <response code="200">Files uploaded successfully</response>
    /// <response code="400">Invalid files or too many files</response>
    [HttpPost("upload-multiple")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResult<List<FileUploadResult>>), 200)]
    [ProducesResponseType(400)]
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

    /// <summary>
    /// Delete an uploaded file
    /// </summary>
    /// <param name="fileName">File name (from the fileUrl path)</param>
    /// <returns>Success confirmation</returns>
    /// <response code="200">File deleted</response>
    /// <response code="404">File not found</response>
    [HttpDelete("{fileName}")]
    [ProducesResponseType(typeof(ApiResult<bool>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(string fileName)
    {
        var deleted = await _fileStorageService.DeleteAsync($"/uploads/{fileName}");

        if (!deleted)
            return NotFound(ApiResult<object>.Fail("File not found."));

        return Ok(ApiResult<bool>.Ok(true, "File deleted."));
    }
}