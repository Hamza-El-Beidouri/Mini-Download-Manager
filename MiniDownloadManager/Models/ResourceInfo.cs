namespace MiniDownloadManager.Models;

public class ResourceInfo
{
    public string SuggestedFileName  { get; }
    public string ContentType { get; }
    public long? Size { get; }
    public bool IsSizeKnown { get; }

    public ResourceInfo(string suggestedFileName, string contentType, bool isSizeKnown, long? size = null)
    {
        SuggestedFileName = suggestedFileName;
        ContentType = contentType;
        IsSizeKnown = isSizeKnown;
        Size = size;
    }
}