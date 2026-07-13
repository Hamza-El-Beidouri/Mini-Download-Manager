using System.Windows.Controls;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using MiniDownloadManager.Models;
using MiniDownloadManager.Services;

namespace MiniDownloadManager.Views;

public partial class NewDownloadView : UserControl
{

    private DispatcherTimer _inputDelayTimer;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly string _urlRegex;
    private ResourceInfo? _resourceInfo;
    private ResourceInspector _inspector;
    
    
    private void ConfigureTimer()
    {
        // Create a timer with a 0.5 second interval.
        _inputDelayTimer = new DispatcherTimer();
        // Hook up the Elapsed event for the timer. 
        _inputDelayTimer.Tick += OnTimedEvent;
        _inputDelayTimer.Interval = TimeSpan.FromSeconds(0.5);
        
    }
    
    public NewDownloadView()
    {
        InitializeComponent();
        ConfigureTimer();
        _urlRegex =
            @"^https?:\/\/(?:www\.)?[a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)$";
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

    private void OnTimedEvent(object sender, EventArgs e)
    {
        
    }
    
    
    private bool IsValidUrl(string url)
    {
        return Regex.IsMatch(url, _urlRegex);
    }
    
    private void UrlInput_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _inputDelayTimer.Stop(); // Stop the timer
        _cancellationTokenSource.Cancel(); // Cancel the previous inspection if one was already running
        _cancellationTokenSource.Dispose(); // Clean up the old one completely
        _cancellationTokenSource = new CancellationTokenSource(); // Instantiate the fresh one
        
        string url = ((TextBox)sender).Text;

        if (!string.IsNullOrWhiteSpace(url) && IsValidUrl(url))
            _inputDelayTimer.Start(); // Start the timer

    }
}