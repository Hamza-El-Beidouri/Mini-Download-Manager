using System.Windows.Controls;
using System.Windows;

namespace MiniDownloadManager.Views;

public partial class DownloadDetailsView : UserControl
{
    
    public event EventHandler? CloseRequested; 
    
    public DownloadDetailsView()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
    
}