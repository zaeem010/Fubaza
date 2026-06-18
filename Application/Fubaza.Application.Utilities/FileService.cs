using Fubaza.Application.Core.Contracts.Services;
using Fubaza.Application.Core.Extensions;
using Fubaza.Application.Core.Settings;
using Fubaza.Application.Dto.Services;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace Fubaza.Application.Utilities
{
	public class FileService : IFileService
	{
		private readonly ILogger<FileService> _logger;
		private readonly IWebHostEnvironment _env;
        private readonly StorageClient _storageClient;
        private readonly CloudStorageSetting _settings;
        public FileService(ILogger<FileService> logger,
            IWebHostEnvironment env,
            IOptions<CloudStorageSetting> settings)
		{
			_logger = logger;
			_env = env;
            _settings = settings.Value;
            var json = _settings.CredentialJson ?? throw new Exception("CredentialJson is required.");

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var credential = GoogleCredential.FromStream(stream);

            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> GetTemplateContent(string template, Dictionary<string, string> placeholders)
        {
            var content = string.Empty;

            try
            {
                var path = Path.Combine(_env.WebRootPath, "email-templates", template);
                _logger.LogInformation("Attempting to read email template from path: {TemplatePath}", path);

                if (File.Exists(path))
                {
                    content = await File.ReadAllTextAsync(path);
                    _logger.LogInformation("Successfully read content from template: {Template}", template);

                    if (string.IsNullOrWhiteSpace(content))
                    {
                        _logger.LogWarning("Template content is empty for: {Template}", template);
                    }
                    else
                    {
                        _logger.LogInformation("Template content before placeholder replacement:\n{RawContent}", content);
                    }

                    foreach (var (key, value) in placeholders)
                    {
                        _logger.LogInformation("Replacing placeholder: {Placeholder} with: {Value}", key, value);
                        content = content.Replace(key, value);
                    }

                    _logger.LogInformation("Final email content after replacements:\n{ProcessedContent}", content);
                }
                else
                {
                    _logger.LogWarning("Template file does not exist at path: {TemplatePath}", path);
                }
            }
            catch (Exception ex)
            {
                content = string.Empty;
                _logger.LogError(ex, "Failed to read or process email template: {Template}", template);
            }

            return content;
        }

        public async Task<string> UploadAsync(UploadRequest request)
        {
            try
            {
                // Validate request
                if (request.FileContent == null && (request.IsBase64 && string.IsNullOrEmpty(request.Base64)))
                    throw new Exception("Upload request contains no data.");

                byte[] fileData = request.IsBase64
                    ? Convert.FromBase64String(request.Base64!)
                    : request.FileContent!;

                if (fileData.Length == 0)
                    throw new Exception("Decoded file content is empty.");

                // Paths
                string folder = request.UploadType.ToDescriptionString();
                string folderPath = $"attachments/{folder}".Replace("\\", "/");

                string originalName = string.IsNullOrWhiteSpace(request.FileName)
                    ? "file"
                    : request.FileName!.Trim('"');

                string fileName = $"{Guid.NewGuid()}_{originalName}";
                string objectName = $"{folderPath}/{fileName}";

                using var memoryStream = new MemoryStream(fileData);

                // Build object metadata including Content-Type + CacheControl
                var gcsObject = new Google.Apis.Storage.v1.Data.Object
                {
                    Bucket = _settings.BucketName,
                    Name = objectName,
                    ContentType = GetContentType(fileName),
                    CacheControl = "public, max-age=3600"
                };

                // Upload to GCS
                await _storageClient.UploadObjectAsync(
                    gcsObject,
                    memoryStream
                );

                // Make object publicly readable
                await _storageClient.UpdateObjectAsync(
                    new Google.Apis.Storage.v1.Data.Object
                    {
                        Bucket = _settings.BucketName,
                        Name = objectName
                    },
                    new UpdateObjectOptions
                    {
                        PredefinedAcl = PredefinedObjectAcl.PublicRead
                    }
                );

                // Public URL
                return $"https://storage.googleapis.com/{_settings.BucketName}/{objectName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file to Google Cloud.");
                return string.Empty;
            }
        }





        private string GetContentType(string fileName)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            return contentType ?? "application/octet-stream";
        }

        //public Task<string> UploadAsync(UploadRequest request)
        //{
        //    return UploadAsync(request, out _);
        //}

        //public Task<string> UploadAsync(UploadRequest request, out string file_directory_path)
        //{
        //    file_directory_path = string.Empty;

        //    if ((request.IsBase64 && string.IsNullOrEmpty(request.Base64)) && request.FileContent == null)
        //    {
        //        _logger.LogWarning("Upload request has no content: IsBase64={IsBase64}, Base64 is empty, and FileContent is null.", request.IsBase64);
        //        return Task.FromResult(string.Empty);
        //    }

        //    byte[] file_data = request.IsBase64 ? Convert.FromBase64String(request.Base64) : request.FileContent;
        //    if (file_data == null)
        //    {
        //        _logger.LogWarning("File data is null after conversion.");
        //        return Task.FromResult(string.Empty);
        //    }

        //    using var stream_data = new MemoryStream(file_data);
        //    if (stream_data.Length == 0)
        //    {
        //        _logger.LogWarning("Stream length is 0. No file saved.");
        //        return Task.FromResult(string.Empty);
        //    }

        //    string folder = request.UploadType.ToDescriptionString();
        //    string folder_name = Path.Combine("Attachments", folder).Replace('\\', '/');  // Linux-safe
        //    string path_to_save = Path.Combine(Directory.GetCurrentDirectory(), folder_name);

        //    _logger.LogInformation("Saving file to directory: {DirectoryPath}", path_to_save);

        //    if (!Directory.Exists(path_to_save))
        //    {
        //        Directory.CreateDirectory(path_to_save);
        //        _logger.LogInformation("Created directory at: {DirectoryPath}", path_to_save);
        //    }

        //    file_directory_path = path_to_save;

        //    string file_name = request.FileName?.Trim('"');
        //    if (string.IsNullOrEmpty(file_name))
        //    {
        //        file_name = Guid.NewGuid().ToString();
        //        _logger.LogInformation("No filename provided. Generated unique filename: {GeneratedFileName}", file_name);
        //    }

        //    string full_path = Path.Combine(path_to_save, file_name);
        //    string db_path = Path.Combine(folder_name, file_name).Replace('\\', '/');  // Linux-safe path for DB

        //    if (File.Exists(full_path))
        //    {
        //        _logger.LogWarning("File already exists. Finding next available filename.");
        //        db_path = GetNextAvailableFilename(db_path);
        //        full_path = GetNextAvailableFilename(full_path);
        //        _logger.LogInformation("Next available file paths: full_path={FullPath}, db_path={DbPath}", full_path, db_path);
        //    }

        //    try
        //    {
        //        using var file_stream = new FileStream(full_path, FileMode.Create);
        //        stream_data.CopyTo(file_stream);
        //        _logger.LogInformation("File written successfully at path: {FullPath}", full_path);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to write file at path: {FullPath}", full_path);
        //        return Task.FromResult(string.Empty);
        //    }

        //    return Task.FromResult(db_path);
        //}

        //private static string number_pattern = " ({0})";

        //private static string GetNextAvailableFilename(string path)
        //{
        //    if (!File.Exists(path))
        //        return path;

        //    if (Path.HasExtension(path))
        //        return FindNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), number_pattern));

        //    return FindNextFilename(path + number_pattern);
        //}

        //private static string FindNextFilename(string pattern)
        //{
        //    string test_path = string.Format(pattern, 1);
        //    if (!File.Exists(test_path))
        //        return test_path;

        //    int min = 1, max = 5;

        //    while (File.Exists(string.Format(pattern, max)))
        //    {
        //        min = max;
        //        max *= 2;
        //    }

        //    while (max != min + 1)
        //    {
        //        int pivot = (max + min) / 2;
        //        if (File.Exists(string.Format(pattern, pivot)))
        //            min = pivot;
        //        else
        //            max = pivot;
        //    }

        //    return string.Format(pattern, max);
        //}

    }
}
