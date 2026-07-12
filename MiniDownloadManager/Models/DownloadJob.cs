namespace MiniDownloadManager.Models;

public class DownloadJob
{

    public enum DownloadStatus
    {
        Queued,
        Downloading,
        Paused,
        Completed,
        Failed
    };
    
    public Guid Id { get; }
    public string Url { get; }
    public string FileName { get; }
    public string DestinationPath { get; }
    public DownloadStatus Status { get;  set; }
    public long DownloadedBytes  { get;  set; }
    public long TotalBytes { get; }
    public string ContentType { get;  }
    public DateTime DateAdded { get; }

    public DownloadJob(string url, string fileName, string destinationPath, string contentType, long totalBytes = 0)
    {
        Id = Guid.NewGuid();
        Url = url;
        FileName = fileName;
        DestinationPath = destinationPath;
        Status = DownloadStatus.Queued;
        ContentType = contentType;
        TotalBytes = totalBytes;
        DateAdded = DateTime.Now;
    }
    
}