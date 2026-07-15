using System.Windows;
using System.Windows.Controls;
namespace MiniDownloadManager.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent(); // Will resolve immediately once namespaces match
        
        // Subscribe to event handlers
        DownloadDetailsViewControl.CloseRequested += DownloadDetailsView_CloseRequested;
    }

    
    private void NewDownloadView_DownloadRequested(object? sender, EventArgs e)
    {
        var dialog = new NewDownloadView();
        DialogHost.Content = dialog;
        NewDownloadOverlay.Visibility = Visibility.Visible;
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
}