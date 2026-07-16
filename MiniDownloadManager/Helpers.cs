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
    
}