namespace MiniDownloadManager.Models;

public class DownloadProgressEventArgs
{

    public int Percentage { get; }
    public long DownloadedBytes { get; }
    public double SpeedBytesPerSecond { get; }

    public DownloadProgressEventArgs(int percentage, long downloadedBytes, double speedBytesPerSecond)
    {
        Percentage = percentage;
        DownloadedBytes = downloadedBytes;
        SpeedBytesPerSecond = speedBytesPerSecond;
    }
    
    
}