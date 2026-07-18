namespace MiniDownloadManager.Models;

public class DownloadProgressEventArgs
{

    public Guid DownloadId { get; }
    public int Percentage { get; }
    public long DownloadedBytes { get; }
    public double SpeedBytesPerSecond { get; }

    public DownloadProgressEventArgs(
        Guid downloadId,
        int percentage,
        long downloadedBytes,
        double speedBytesPerSecond)
    {
        DownloadId = downloadId;
        Percentage = percentage;
        DownloadedBytes = downloadedBytes;
        SpeedBytesPerSecond = speedBytesPerSecond;
    }
    
}