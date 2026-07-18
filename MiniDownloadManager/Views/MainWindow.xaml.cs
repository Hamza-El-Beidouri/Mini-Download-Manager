using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MiniDownloadManager.Models;
using MiniDownloadManager.Services;

namespace MiniDownloadManager.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    
    private readonly ObservableCollection<DownloadJob> _downloads;
    private readonly Dictionary<Guid, CancellationTokenSource> _tokens;
    private readonly DownloadEngine _engine;
    
    public MainWindow()
    {
        InitializeComponent();

        _downloads = new ObservableCollection<DownloadJob>();
        DownloadsGrid.ItemsSource = _downloads;
        
        _tokens = new Dictionary<Guid, CancellationTokenSource>();
        
        _engine = new DownloadEngine();
        
        // Subscribe to event handlers
        DownloadDetailsViewControl.CloseRequested += DownloadDetailsView_CloseRequested;
        _engine.ProgressChanged += Engine_ProgressChanged;
    }
    
    private void AddDownloadButton_Click(object? sender, EventArgs e)
    {
        NewDownloadView dialog = new NewDownloadView();

        dialog.DownloadRequested += Dialog_DownloadRequested;

        DialogHost.Content = dialog;
        NewDownloadOverlay.Visibility = Visibility.Visible;
    }
    
    private async void Dialog_DownloadRequested(object? sender, DownloadJob job)
    {
        NewDownloadOverlay.Visibility = Visibility.Collapsed;
        DialogHost.Content = null;

        _downloads.Add(job);

        CancellationTokenSource cts = new CancellationTokenSource();

        _tokens.Add(job.Id, cts);

        job.Status = DownloadJob.DownloadStatus.Downloading;

        await _engine.Download(job, cts.Token);
    }
    
    private void Engine_ProgressChanged(object? sender, DownloadProgressEventArgs e)
    {
        DownloadJob? job = _downloads.FirstOrDefault(d => d.Id == e.DownloadId);

        if (job == null)
            return;

        Dispatcher.Invoke(() =>
        {
            job.DownloadedBytes = e.DownloadedBytes;
            job.Speed = Helpers.FormatSpeed(e.SpeedBytesPerSecond);
        });
    }
    
    private void DownloadDetailsView_CloseRequested(object? sender, EventArgs e)
    {
        DetailsPanelOverlay.Visibility = Visibility.Collapsed;
    }

    private void DownloadsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DataGrid? dataGrid = (DataGrid)sender;
        if (dataGrid != null && dataGrid.SelectedItem != null)
        {
            // A row was clicked! Open up the detailed side panel context drawer
            DetailsPanelOverlay.Visibility = Visibility.Visible;
        }
        else
        {
            // No row selected, make sure it stays hidden
            DetailsPanelOverlay.Visibility = Visibility.Collapsed;
        }
    }


    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        DownloadJob job = (DownloadJob)((Button)sender).DataContext;

        _tokens[job.Id].Cancel();

        job.Status = DownloadJob.DownloadStatus.Paused;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DownloadJob job = (DownloadJob)((Button)sender).DataContext;

        _tokens[job.Id].Cancel();

        job.Status = DownloadJob.DownloadStatus.Cancelled;
    }
}