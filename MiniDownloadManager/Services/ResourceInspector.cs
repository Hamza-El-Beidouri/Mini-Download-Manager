using System.Net.Http;
using System.Net.Http.Headers;
using MiniDownloadManager.Models;

namespace MiniDownloadManager.Services;

public class ResourceInspector
{
    
    private static readonly HttpClient Client = new HttpClient();

    private ResourceInfo CreateResourceInfo(HttpContentHeaders contentHeaders)
    {
        
        var suggestedFileName = contentHeaders.ContentDisposition?.FileNameStar ?? contentHeaders.ContentDisposition?.FileName;
        var contentType = contentHeaders.ContentType?.MediaType;
        var contentLength = contentHeaders.ContentLength;
        
        return new ResourceInfo(suggestedFileName, contentType, contentLength);
    }

    private async Task<ResourceInfo?> InspectUsingGetAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using HttpResponseMessage response = await Client.GetAsync(url, 
                HttpCompletionOption.ResponseHeadersRead ,cancellationToken);
            
            // This method is used to throw an exception if the response wasn't resolved successfully
            response.EnsureSuccessStatusCode();
            
            HttpContentHeaders headers = response.Content.Headers; // Fetch headers
            
            return CreateResourceInfo(headers);
            
        }
        catch (HttpRequestException)
        {
            // Log later for now return null
            return null;
        }
        catch (TaskCanceledException)
        {
            // Log later for now return null
            return null;
        }
    }

    private async Task<ResourceInfo?> InspectUsingHeadAsync(string url, CancellationToken cancellationToken)
    {
        try
        {

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);

            using HttpResponseMessage response = await Client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // Fallback to a GET method if the server doesn't support a HEAD method
                return await InspectUsingGetAsync(url, cancellationToken);
            }

            // This method is used to throw an exception if the response wasn't resolved successfully
            response.EnsureSuccessStatusCode();

            HttpContentHeaders headers = response.Content.Headers; // Fetch headers
            return CreateResourceInfo(headers);

        }
        catch (HttpRequestException)
        {
            // Log later for now return null
            return null;
        }
        catch (TaskCanceledException)
        {
            // Log later for now return null
            return null;
        }
    }
    
    public async Task<ResourceInfo?> InspectResourceAsync(string url, CancellationToken cancellationToken)
    {
        return await InspectUsingHeadAsync(url, cancellationToken);
    }
    
}