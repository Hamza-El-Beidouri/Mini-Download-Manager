using System.Text;

namespace MiniDownloadManager.Services;

public static class ResourceDetector
{
    
    /// <summary>
    /// Detects the file extension based on the first few bytes (magic numbers) of the file.
    /// </summary>
    public static string DetectCorrectExtension(ReadOnlySpan<byte> data)
    {
        if (data.Length < 12) return "bin";

        // --- IMAGES & DOCUMENTS ---

        // PNG: 89 50 4E 47
        if (data[..4].SequenceEqual((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47]))
            return "png";

        // JPEG: FF D8 FF
        if (data[..3].SequenceEqual((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
            return "jpg";

        // GIF87a or GIF89a
        if (data[..4].SequenceEqual((ReadOnlySpan<byte>)[0x47, 0x49, 0x46, 0x38]) && (data[4] == 0x37 || data[4] == 0x39) && data[5] == 0x61)
            return "gif";

        // BMP: BM
        if (data[0] == 0x42 && data[1] == 0x4D)
            return "bmp";

        // PDF: %PDF
        if (data[..4].SequenceEqual((ReadOnlySpan<byte>)[0x25, 0x50, 0x44, 0x46]))
            return "pdf";

        // ZIP / Office Docs (PK..)
        if (data[0] == 0x50 && data[1] == 0x4B)
            return "zip";

        // WEBP: RIFF....WEBP
        if (data[..4].SequenceEqual((ReadOnlySpan<byte>)[0x52, 0x49, 0x46, 0x46]) && data[8..12].SequenceEqual((ReadOnlySpan<byte>)[0x57, 0x45, 0x42, 0x50]))
            return "webp";

        // AVI: RIFF....AVI 
        if (data[..4].SequenceEqual((ReadOnlySpan<byte>)[0x52, 0x49, 0x46, 0x46]) && data[8..12].SequenceEqual((ReadOnlySpan<byte>)[0x41, 0x56, 0x49, 0x20]))
            return "avi";

        // --- MKV & WebM (Unified Check) ---
        if (data[..4].SequenceEqual((ReadOnlySpan<byte>)[0x1A, 0x45, 0xDF, 0xA3]))
        {
            // Read first 50 bytes safely to distinguish WebM from MKV
            int checkLength = Math.Min(50, data.Length);
            string ebmlHeader = Encoding.UTF8.GetString(data[..checkLength]);
            
            if (ebmlHeader.Contains("webm"))
                return "webm";
                
            return "mkv";
        }

        // --- VIDEO FORMATS ---

        // MP4, MOV, 3GP (ftyp)
        if (data[4..8].SequenceEqual((ReadOnlySpan<byte>)[0x66, 0x74, 0x79, 0x70]))
        {
            ReadOnlySpan<byte> subtype = data[8..12];
            if (subtype.SequenceEqual((ReadOnlySpan<byte>)[0x69, 0x73, 0x6F, 0x6D]) || 
                subtype.SequenceEqual((ReadOnlySpan<byte>)[0x4D, 0x53, 0x4E, 0x56]) || 
                subtype.SequenceEqual((ReadOnlySpan<byte>)[0x6D, 0x70, 0x34, 0x32]))
            {
                return "mp4";
            }
            return "mp4"; // Default generic mp4
        }

        // FLV
        if (data[..4].SequenceEqual((ReadOnlySpan<byte>)[0x46, 0x4C, 0x56, 0x01]))
            return "flv";

        // WMV
        if (data[..12].SequenceEqual((ReadOnlySpan<byte>)[0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA]))
            return "wmv";

        // MPEG/MPG
        if (data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x01 && (data[3] == 0xBA || data[3] == 0xB3))
            return "mpg";

        // QuickTime MOV (moov or mdat at position 4)
        if (data[4..8].SequenceEqual((ReadOnlySpan<byte>)[0x6D, 0x6F, 0x6F, 0x76]) || data[4..8].SequenceEqual((ReadOnlySpan<byte>)[0x6D, 0x64, 0x61, 0x74]))
            return "mov";

        // --- TEXT FORMATS (XML, HTML, JSON) ---
        try
        {
            int checkLen = Math.Min(100, data.Length);
            string start = Encoding.UTF8.GetString(data[..checkLen]).TrimStart();
            string startLower = start.ToLower();

            if (startLower.StartsWith("<html") || startLower.StartsWith("<!doctype"))
                return "html";
            if (startLower.StartsWith("<?xml"))
                return "xml";
            if (start.StartsWith('{') || start.StartsWith('['))
                return "json";
        }
        catch
        {
            // Fail silently and drop down to fallback
        }

        return "bin"; // Fallback to raw binary extension
    }
    
}