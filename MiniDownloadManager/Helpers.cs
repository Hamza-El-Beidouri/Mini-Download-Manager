namespace MiniDownloadManager;

public static class Helpers
{
    
    public static string FormatFileSize(long? bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };

        long? size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }
    
    public static string FormatSpeed(double bytesPerSecond)
    {
        const double KB = 1024;
        const double MB = KB * 1024;
        const double GB = MB * 1024;

        if (bytesPerSecond >= GB)
            return $"{bytesPerSecond / GB:F2} GB/s";

        if (bytesPerSecond >= MB)
            return $"{bytesPerSecond / MB:F2} MB/s";

        if (bytesPerSecond >= KB)
            return $"{bytesPerSecond / KB:F2} KB/s";

        return $"{bytesPerSecond:F0} B/s";
    }
    
}