namespace MiniDownloadManager.Models;

public class ResourceInfo
{
    public string? SuggestedFileName { get; }
    public string? ContentType { get; }
    public string FileExtension { get; set; }
    public long? Size { get; }
    
    public ResourceInfo(string? suggestedFileName, string? contentType, string fileExtension, long? size = null)
    {
        SuggestedFileName = suggestedFileName;
        ContentType = contentType;
        FileExtension = fileExtension;
        Size = size;
    }
}