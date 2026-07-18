using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace MiniDownloadManager.Models;

public class DownloadJob : INotifyPropertyChanged
{
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
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
    private string _fileName;

    public string FileName
    {
        get => _fileName;
        set
        {
            if (_fileName == value)
                return;

            _fileName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileExtension));
        }
    }
    public string DestinationPath { get; }
    private DownloadStatus _status;

    public DownloadStatus Status
    {
        get => _status;
        set
        {
            if (_status == value)
                return;

            _status = value;
            OnPropertyChanged();
        }
    }
    
    private long _downloadedBytes;

    public long DownloadedBytes
    {
        get => _downloadedBytes;
        set
        {
            if (_downloadedBytes == value)
                return;

            _downloadedBytes = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressPercentage));
        }
    }
    private long? _totalBytes;

    public long? TotalBytes
    {
        get => _totalBytes;
        set
        {
            if (_totalBytes == value)
                return;

            _totalBytes = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(TotalSize));
        }
    }
    public string ContentType { get; }
    public DateTime DateAdded { get; }
    
    private string _speed = "0 KB/s";

    public string Speed
    {
        get => _speed;
        set
        {
            if (_speed == value)
                return;

            _speed = value;
            OnPropertyChanged();
        }
    }

    // Derived property prevents contradicting state
    public string FileExtension => Path.GetExtension(FileName).TrimStart('.');

    public int ProgressPercentage => TotalBytes.HasValue && TotalBytes.Value > 0 ? (int)(DownloadedBytes * 100 / TotalBytes.Value) : 0;
    
    public string TotalSize => TotalBytes.HasValue ? Helpers.FormatFileSize(TotalBytes) : "Unknown";

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