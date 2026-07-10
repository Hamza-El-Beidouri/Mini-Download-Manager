using System.Windows.Controls;
using System.Windows;

namespace MiniDownloadManager.Views;

public partial class NewDownloadView : UserControl
{

    public event EventHandler? CancelRequested;
    public event EventHandler? DownloadRequested;
    
    public NewDownloadView()
    {
        InitializeComponent();
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        DownloadRequested?.Invoke(this, EventArgs.Empty);
    }

}