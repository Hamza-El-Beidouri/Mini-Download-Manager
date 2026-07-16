using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MiniDownloadManager.Models;

namespace MiniDownloadManager.Services;

public class ResourceInspector
{
    private static readonly HttpClient Client = new();

    private ResourceInfo CreateResourceInfo(HttpContentHeaders contentHeaders)
    {
        var suggestedFileName = contentHeaders.ContentDisposition?.FileNameStar 
                                ?? contentHeaders.ContentDisposition?.FileName;
        
        var contentType = contentHeaders.ContentType?.MediaType;
        var contentLength = contentHeaders.ContentLength;

        string fileExtension = GetExtensionFromHeaders(suggestedFileName, contentType);
        
        return new ResourceInfo(suggestedFileName, contentType, fileExtension, contentLength);
    }

    private string GetExtensionFromHeaders(string? suggestedFileName, string? contentType)
    {
        if (!string.IsNullOrEmpty(suggestedFileName))
        {
            string ext = Path.GetExtension(suggestedFileName).TrimStart('.');
            if (!string.IsNullOrEmpty(ext)) return ext.ToLower();
        }

        return contentType?.ToLower() switch
        {
            "image/png" => "png",
            "image/jpeg" => "jpg",
            "image/gif" => "gif",
            "image/webp" => "webp",
            "application/pdf" => "pdf",
            "application/zip" => "zip",
            "video/mp4" => "mp4",
            "video/x-matroska" => "mkv",
            "text/html" => "html",
            "application/json" => "json",
            _ => "bin"
        };
    }

    private async Task<ResourceInfo?> InspectUsingGetAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await Client.GetAsync(
                url, 
                HttpCompletionOption.ResponseHeadersRead, 
                cancellationToken);
            
            response.EnsureSuccessStatusCode();
            return CreateResourceInfo(response.Content.Headers);
        }
        catch
        {
            return null;
        }
    }

    private async Task<ResourceInfo?> InspectUsingHeadAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Head, url);
            using HttpResponseMessage response = await Client.SendAsync(
                request, 
                HttpCompletionOption.ResponseHeadersRead, 
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await InspectUsingGetAsync(url, cancellationToken);
            }

            response.EnsureSuccessStatusCode();
            return CreateResourceInfo(response.Content.Headers);
        }
        catch
        {
            return await InspectUsingGetAsync(url, cancellationToken);
        }
    }
    
    public async Task<ResourceInfo?> InspectResourceAsync(string url, CancellationToken cancellationToken)
    {
        return await InspectUsingHeadAsync(url, cancellationToken);
    }
}