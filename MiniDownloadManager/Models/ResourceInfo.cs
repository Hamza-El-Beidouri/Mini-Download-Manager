namespace MiniDownloadManager.Models;

public class ResourceInfo
{
    public string? SuggestedFileName  { get; }
    public string? ContentType { get; }
    public long? Size { get; }
    
    public ResourceInfo(string? suggestedFileName, string? contentType, long? size = null)
    {
        SuggestedFileName = suggestedFileName;
        ContentType = contentType;
        Size = size;
        
    }
    
}