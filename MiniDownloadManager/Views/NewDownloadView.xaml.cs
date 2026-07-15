using System.Windows.Controls;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using MiniDownloadManager.Models;
using MiniDownloadManager.Services;

namespace MiniDownloadManager.Views;

public partial class NewDownloadView : UserControl
{

    private  DispatcherTimer _inputDelayTimer;
    private static readonly TimeSpan InspectionDelay = TimeSpan.FromMilliseconds(500);
    private CancellationTokenSource _cancellationTokenSource;
    private ResourceInfo? _resourceInfo;
    private ResourceInspector _inspector;
    
    
    private void ConfigureTimer()
    {
        // Create a timer with a 0.5 second interval.
        _inputDelayTimer = new DispatcherTimer();
        // Hook up the Elapsed event for the timer. 
        _inputDelayTimer.Tick += OnTimedEvent;
        _inputDelayTimer.Interval = InspectionDelay;
        
    }
    
    public NewDownloadView()
    {
        InitializeComponent();
        ConfigureTimer();
        _cancellationTokenSource = new CancellationTokenSource();
        _inspector = new ResourceInspector();
        _resourceInfo = null;
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        
    }

    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private static string FormatFileSize(long? bytes)
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
    
    private void DisplayResourceInfo()
    {
        FileNameText.Text = $"Filename: {_resourceInfo?.SuggestedFileName ?? "Unknown"}";
        FileSizeText.Text = $"Size: {(_resourceInfo.Size.HasValue ? FormatFileSize(_resourceInfo.Size) : "Unknown")}";
        ContentTypeText.Text = $"ContentType: {_resourceInfo?.ContentType ?? "Unknown"}";
        ResourceInfoPanel.Visibility = Visibility.Visible;
    }
    
    private async void OnTimedEvent(object? sender, EventArgs e)
    {
        _resourceInfo = await _inspector.InspectResourceAsync(UrlInput.Text, _cancellationTokenSource.Token);

        if (_resourceInfo != null)
        {
            DisplayResourceInfo();
            DownloadNowBtn.IsEnabled = true;
        }
    }

    private void ResetInspectionState()
    {
        _inputDelayTimer.Stop(); // Stop the timer
        _cancellationTokenSource.Cancel(); // Cancel the previous inspection if one was already running
        _cancellationTokenSource.Dispose(); // Clean up the old one completely
        _cancellationTokenSource = new CancellationTokenSource(); // Instantiate the fresh one
    }
    
    private void UrlInput_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        
        ResetInspectionState();

        string? url = ((TextBox)sender).Text;

        if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var result))
            _inputDelayTimer.Start(); // Start the timer
        else
        {
            ResourceInfoPanel.Visibility = Visibility.Collapsed;
            DownloadNowBtn.IsEnabled = false;
        }

    }
}