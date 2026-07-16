using System;
using System.IO;

namespace MiniDownloadManager.Models;

public class DownloadJob
{
    public enum DownloadStatus
    {
        Queued,
        Downloading,
        Paused,
        Cancelled,
        Completed,
        Failed
    }
    
    public Guid Id { get; }
    public string Url { get; }
    public string FileName { get; set; }
    public string DestinationPath { get; }
    public DownloadStatus Status { get; set; }
    
    public long DownloadedBytes { get; set; }
    public long? TotalBytes { get; set; }
    public string ContentType { get; }
    public DateTime DateAdded { get; }

    // Derived property prevents contradicting state
    public string FileExtension => Path.GetExtension(FileName).TrimStart('.');

    public DownloadJob(string url, string fileName, string destinationPath, string contentType, long? totalBytes = null)
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