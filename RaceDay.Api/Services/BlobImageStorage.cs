// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 3
// Institution: The Independent Institute of Education (IIE)
// Date:        2 November 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// References:  Microsoft Learn, n.d. Quickstart: Azure Blob Storage client library for .NET. [online] Available at:
//              https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet [Accessed 2 November 2026].

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

// Saves images in an Azure Blob Storage container.
// The connection string is a SECRET: it comes from configuration (user-secrets on your laptop,
// an environment variable in Docker) and must never be written in code or committed to GitHub.
public class BlobImageStorage : IImageStorage
{
    private readonly BlobContainerClient _container;

    public BlobImageStorage(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "AzureBlobStorage:ConnectionString is not set. Store it with: " +
                "dotnet user-secrets set \"AzureBlobStorage:ConnectionString\" \"<your connection string>\" --project RaceDay.Api");
        }

        var containerName = configuration["AzureBlobStorage:ContainerName"] ?? "raceday-images";
        _container = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadAsync(Stream content, string folder, string extension, string contentType,
        CancellationToken cancellationToken = default)
    {
        // Does nothing if the container already exists
        await _container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        // A random name, so nobody can guess or overwrite another image. We never use the uploaded file's own name.
        var blobName = $"{folder}/{Guid.NewGuid():N}{extension}";
        var blob = _container.GetBlobClient(blobName);

        // Tell the browser what kind of file this is, so it shows the image instead of downloading it
        var options = new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } };
        await blob.UploadAsync(content, options, cancellationToken);

        return blob.Uri.ToString();
    }
}
