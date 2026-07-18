using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MiniDownloadManager.Models;

namespace MiniDownloadManager.Services;

public class DownloadEngine
{
    private static readonly HttpClient _client = new();
    
    public event EventHandler<DownloadProgressEventArgs>? ProgressChanged;
    
    private int CalculatePercentage(long downloadedBytes, long totalBytes)
    {
        if (totalBytes == 0) return 0;
        return (int)((double)downloadedBytes / totalBytes * 100);
    }

    /// <summary>
    /// Checks if one second has elapsed, calculates speed, and resets tracking.
    /// </summary>
    private double? TryCalculateSpeed(ref int bytesIntervalAccumulator, Stopwatch stopwatch)
    {
        if (stopwatch.ElapsedMilliseconds < 1000) 
            return null;

        double speed = bytesIntervalAccumulator / stopwatch.Elapsed.TotalSeconds;
        
        // Reset state for next interval
        bytesIntervalAccumulator = 0;
        stopwatch.Restart();
        
        return speed;
    }
    
    private void EnsureCorrectFileExtension(DownloadJob downloadJob, ReadOnlySpan<byte> firstChunk)
    {
        // Detect using our optimized resource detector
        string detectedExt = ResourceDetector.DetectCorrectExtension(firstChunk);
    
        if (!downloadJob.FileName.EndsWith($".{detectedExt}", StringComparison.OrdinalIgnoreCase))
        {
            downloadJob.FileName = Path.ChangeExtension(downloadJob.FileName, detectedExt);
        }
    }
    
    public async Task Download(DownloadJob downloadJob, CancellationToken token)
    {
        try
        {
            using var response = await _client.GetAsync(downloadJob.Url, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            await using var streamSource = await response.Content.ReadAsStreamAsync(token);

            // 16 KB buffer
            byte[] buffer = new byte[16 * 1024];
            
            // --- STEP 1: READ FIRST CHUNK IN MEMORY FIRST ---
            int bytesReceived = await streamSource.ReadAsync(buffer, 0, buffer.Length, token);
            if (bytesReceived <= 0) return; // Empty stream

            // --- STEP 2: DETECT AND CHOOSE FINAL PATH BEFORE CREATING STREAM ---
            EnsureCorrectFileExtension(downloadJob, buffer.AsSpan(0, bytesReceived));

            // Combined directory path + resolved final safe file name
            string fullPath = Path.Combine(downloadJob.DestinationPath, downloadJob.FileName);

            // --- STEP 3: CREATE THE FILESTREAM & WRITE FIRST CHUNK ---
            await using var destinationStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await destinationStream.WriteAsync(buffer, 0, bytesReceived, token);

            // Setup speed and progress tracking state
            double currentSpeed = 0;
            int bytesIntervalAccumulator = bytesReceived; // Accumulate first chunk
            downloadJob.DownloadedBytes = bytesReceived; // Set initial downloaded bytes
            
            Stopwatch stopwatch = Stopwatch.StartNew();

            // --- STEP 4: CONTINUE STANDARD DOWNLOAD STREAM ---
            while ((bytesReceived = await streamSource.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {

                await destinationStream.WriteAsync(buffer, 0, bytesReceived, token);

                int previousPercent = downloadJob.TotalBytes is { } totalBytes
                    ? CalculatePercentage(downloadJob.DownloadedBytes, totalBytes) 
                    : 0;
                
                // Track accumulated parameters correctly
                downloadJob.DownloadedBytes += bytesReceived;
                bytesIntervalAccumulator += bytesReceived; 

                if (TryCalculateSpeed(ref bytesIntervalAccumulator, stopwatch) is { } newSpeed)
                {
                    currentSpeed = newSpeed;
                }
                
                if (downloadJob.TotalBytes is not { } totalBytesLimit) 
                    continue;

                int currentPercent = CalculatePercentage(downloadJob.DownloadedBytes, totalBytesLimit);

                if (currentPercent > previousPercent)
                {
                    ProgressChanged?.Invoke(this,
                        new DownloadProgressEventArgs(
                            downloadJob.Id,
                            currentPercent,
                            downloadJob.DownloadedBytes,
                            currentSpeed));
                }
            }
            
            downloadJob.Status = DownloadJob.DownloadStatus.Completed;
            downloadJob.Speed = "Finished";
        }
        catch (OperationCanceledException)
        {
            // Graceful cancellation handling
        }
        catch (Exception e)
        {
            // Log errors later
        }
    }
}