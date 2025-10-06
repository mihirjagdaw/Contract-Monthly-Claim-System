using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using ST10449392_CLDV6212_POE.Models;

namespace ST10449392_CLDV6212_POE.Services
{
    public class AzureFileShareService
    {
        private readonly string _connectionString;
        private readonly string _fileShareName;

        public AzureFileShareService(string connectionString, string fileShareName)
        {
            _connectionString = connectionString ?? 
                                throw new ArgumentNullException(nameof(connectionString));
            _fileShareName = fileShareName ?? 
                             throw new ArgumentNullException(nameof(fileShareName));
        }

        //public async Task UploadFileAsync(string directoryName, string fileName, Stream fileStream)
        //{
        //    // Allowed file extensions
        //    string[] allowedExtensions = { ".pdf", ".docx", ".txt" };
        //    string extension = Path.GetExtension(fileName).ToLower();

        //    // Validate extension
        //    if (!allowedExtensions.Contains(extension))
        //    {
        //        throw new InvalidOperationException(
        //            $"Invalid file type '{extension}'. Only PDF, DOCX, and TXT files are allowed.");
        //    }

        //    // Proceed with upload
        //    try
        //    {
        //        var serviceClient = new ShareServiceClient(_connectionString);
        //        var shareClient = serviceClient.GetShareClient(_fileShareName);

        //        var directoryClient = shareClient.GetDirectoryClient(directoryName);
        //        await directoryClient.CreateIfNotExistsAsync();

        //        var fileClient = directoryClient.GetFileClient(fileName);
        //        await fileClient.CreateAsync(fileStream.Length);
        //        await fileClient.UploadRangeAsync(new HttpRange(0, fileStream.Length), fileStream);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error uploading file to Azure File Share: " + ex.Message, ex);
        //    }
        //}


        //public async Task<Stream> DownloadFileAsync(string directoryName, string fileName)
        //{
        //    try
        //    {
        //        var serviceClient = new ShareServiceClient(_connectionString);
        //        var shareClient = serviceClient.GetShareClient(_fileShareName);
        //        var directoryClient = shareClient.GetDirectoryClient(directoryName);
        //        var fileClient = directoryClient.GetFileClient(fileName);
        //        var downloadInfo = await fileClient.DownloadAsync();
        //        return downloadInfo.Value.Content;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error downloading file from Azure File Share: " + ex.Message, ex);
        //    }
        //}

        //public async Task<List<FileModel>> ListFilesAsync(string directoryName)
        //{
        //    var fileModels = new List<FileModel>();
        //    try
        //    {
        //        var serviceClient = new ShareServiceClient(_connectionString);
        //        var shareClient = serviceClient.GetShareClient(_fileShareName);

        //        var directoryClient = shareClient.GetDirectoryClient(directoryName);
        //        await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync())
        //        {
        //            if (!item.IsDirectory)
        //            {
        //                var fileClient = directoryClient.GetFileClient(item.Name);
        //                var properties = await fileClient.GetPropertiesAsync();
        //                fileModels.Add(new FileModel
        //                {
        //                    Name = item.Name,
        //                    Size = properties.Value.ContentLength,
        //                    LastModified = properties.Value.LastModified
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error listing files in Azure File Share: " + ex.Message, ex);
        //    }

        //    return fileModels;
        //}

        public async Task UploadFileAsync(string directoryName, string fileName, Stream fileStream)
        {
            var serviceClient = new ShareServiceClient(_connectionString);
            var shareClient = serviceClient.GetShareClient(_fileShareName);

            ShareDirectoryClient directoryClient;
            if (string.IsNullOrEmpty(directoryName))
            {
                directoryClient = shareClient.GetRootDirectoryClient();
            }
            else
            {
                directoryClient = shareClient.GetDirectoryClient(directoryName);
                await directoryClient.CreateIfNotExistsAsync();
            }

            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadRangeAsync(new HttpRange(0, fileStream.Length), fileStream);
        }


        public async Task<Stream> DownloadFileAsync(string directoryName, string fileName)
        {
            var serviceClient = new ShareServiceClient(_connectionString);
            var shareClient = serviceClient.GetShareClient(_fileShareName);

            ShareDirectoryClient directoryClient = string.IsNullOrEmpty(directoryName)
                ? shareClient.GetRootDirectoryClient()
                : shareClient.GetDirectoryClient(directoryName);

            var fileClient = directoryClient.GetFileClient(fileName);
            var downloadInfo = await fileClient.DownloadAsync();
            return downloadInfo.Value.Content;
        }


        public async Task<List<FileModel>> ListFilesAsync(string directoryName)
        {
            var fileModels = new List<FileModel>();

            var serviceClient = new ShareServiceClient(_connectionString);
            var shareClient = serviceClient.GetShareClient(_fileShareName);

            ShareDirectoryClient directoryClient = string.IsNullOrEmpty(directoryName)
                ? shareClient.GetRootDirectoryClient()
                : shareClient.GetDirectoryClient(directoryName);

            await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    var fileClient = directoryClient.GetFileClient(item.Name);
                    var properties = await fileClient.GetPropertiesAsync();
                    fileModels.Add(new FileModel
                    {
                        Name = item.Name,
                        Size = properties.Value.ContentLength,
                        LastModified = properties.Value.LastModified
                    });
                }
            }

            return fileModels;
        }

    }
}
