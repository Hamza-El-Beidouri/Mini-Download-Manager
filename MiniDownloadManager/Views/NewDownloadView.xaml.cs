using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
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
    
    private string? _selectedFileName;
    private string? _selectedDestinationPath;
    
    public event EventHandler<DownloadJob>? DownloadRequested;
    
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
        if (_resourceInfo == null)
            return;

        if (_selectedDestinationPath == null || _selectedFileName == null)
            return;

        DownloadJob job = new DownloadJob(
            UrlInput.Text,
            _selectedFileName,
            _selectedDestinationPath,
            _resourceInfo.ContentType ?? "application/octet-stream",
            _resourceInfo.Size);

        DownloadRequested?.Invoke(this, job);
    }
    
    private void DisplayResourceInfo()
    {
        FileNameText.Text = $"Filename: {_resourceInfo?.SuggestedFileName ?? "Unknown"}";
        FileSizeText.Text = $"Size: {(_resourceInfo.Size.HasValue ? Helpers.FormatFileSize(_resourceInfo.Size) : "Unknown")}";
        ContentTypeText.Text = $"ContentType: {_resourceInfo?.ContentType ?? "Unknown"}";
        ResourceInfoPanel.Visibility = Visibility.Visible;
    }
    
    private void UpdateDownloadButtonState()
    {
        DownloadNowBtn.IsEnabled =
            _resourceInfo != null &&
            !string.IsNullOrWhiteSpace(_selectedDestinationPath) &&
            !string.IsNullOrWhiteSpace(_selectedFileName);
    }
    
    private async void OnTimedEvent(object? sender, EventArgs e)
    {
        _resourceInfo = await _inspector.InspectResourceAsync(UrlInput.Text, _cancellationTokenSource.Token);

        if (_resourceInfo != null)
        {
            DisplayResourceInfo();
            UpdateDownloadButtonState();
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

    private void BrowsePathBtn_Click(object sender, RoutedEventArgs e)
    {
        
        SaveFileDialog dialog = new SaveFileDialog
        {
            Title = "Choose Download Location",
            FileName = _resourceInfo?.SuggestedFileName ?? "Download",
            DefaultExt = _resourceInfo?.FileExtension,
            AddExtension = true,
            Filter = $"{(_resourceInfo?.FileExtension ?? "All").ToUpper()} files|*.{_resourceInfo?.FileExtension}|All files|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            string fullPath = dialog.FileName;

            _selectedDestinationPath = Path.GetDirectoryName(fullPath)!;
            _selectedFileName = Path.GetFileName(fullPath);

            PathInput.Text = fullPath;
        }
        
    }
}