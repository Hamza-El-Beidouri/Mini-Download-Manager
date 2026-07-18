# Mini Download Manager

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![.NET 8](https://img.shields.io/badge/.NET-8.0-blue?style=for-the-badge&logo=dotnet)
![WPF](https://img.shields.io/badge/WPF-Desktop-purple?style=for-the-badge)

A modern, feature-rich download manager built with C# and WPF that allows users to inspect remote resources before downloading, monitor progress in real-time, and manage downloads through a clean, responsive desktop interface. This project emphasizes clean architecture, asynchronous programming, and maintainable code.

## Features

- **Create Downloads**: Add new downloads with automatic URL validation
- **URL Inspection**: Performs HEAD requests to inspect remote resources before downloading
- **Resource Metadata Retrieval**: Extracts filename, content type, and file size from HTTP headers
- **Custom Save Location**: Browse and select download destination with file name customization
- **Streaming Downloads**: Efficient streaming architecture that doesn't load files into memory
- **Real-time Progress Tracking**: Live updates to progress percentage, downloaded bytes, and download speed
- **Download Speed Calculation**: Calculates and displays current download speed (B/s, KB/s, MB/s, GB/s)
- **Automatic File Extension Detection**: Detects file type from magic numbers (file signatures) instead of relying solely on headers
- **Cancellation Support**: Gracefully cancel active downloads with CancellationToken
- **Real-time UI Updates**: Download list updates automatically via ObservableCollection and INotifyPropertyChanged
- **Responsive Details Panel**: Click any download to view detailed information in a side panel

## Screenshots

![Main Window](docs/main-window.png)
![New Download Dialog](docs/new-download-dialog.png)
![Download Details Panel](docs/download-details.png)

## Architecture

The project follows a layered, separation-of-concerns architecture:

```
MiniDownloadManager/
│
├── Models/
│   ├── DownloadJob.cs              # Represents a download with INotifyPropertyChanged for UI binding
│   ├── DownloadProgressEventArgs.cs # Event args for progress updates
│   └── ResourceInfo.cs             # Metadata about remote resources
│
├── Services/
│   ├── DownloadEngine.cs           # Core streaming download logic
│   ├── ResourceInspector.cs        # HTTP HEAD/GET inspection of remote resources
│   └── ResourceDetector.cs         # File type detection from magic numbers
│
├── Views/
│   ├── MainWindow.xaml(.cs)        # Main application window with download list
│   ├── NewDownloadView.xaml(.cs)   # Dialog for creating new downloads
│   └── DownloadDetailsView.xaml(.cs) # Details panel for selected download
│
├── Helpers.cs                      # Utility functions (file size formatting, speed formatting)
├── App.xaml(.cs)                   # Application entry point and resources
└── AssemblyInfo.cs                 # Assembly metadata
```

### How Components Collaborate

1. **User initiates download** → MainWindow displays NewDownloadView dialog
2. **URL entered** → DispatcherTimer triggers ResourceInspector.InspectResourceAsync()
3. **ResourceInspector** → Performs HEAD request, extracts metadata, populates ResourceInfo
4. **User confirms** → MainWindow creates DownloadJob with selected destination and filename
5. **Download starts** → MainWindow calls DownloadEngine.Download() with CancellationToken
6. **DownloadEngine streams** → ReadOnlySpan detects magic numbers, DownloadJob properties update
7. **Progress events** → DownloadEngine.ProgressChanged fires, MainWindow uses Dispatcher to update UI
8. **Download completes** → DownloadJob.Status transitions to Completed

## Download Workflow

The complete lifecycle of a download:

1. **User initiates**: Clicks "Add Download" button in main window
2. **Dialog opens**: NewDownloadView dialog displays with URL input field
3. **Resource inspection**: As user types URL, ResourceInspector performs HEAD request (with 500ms debounce)
4. **Metadata displayed**: ResourceInfo shows suggested filename, content type, and file size
5. **User selects destination**: Clicks "Browse" to open SaveFileDialog, selects save location
6. **DownloadJob created**: MainWindow instantiates DownloadJob with URL, filename, path, and metadata
7. **Download engine starts**: MainWindow calls DownloadEngine.Download() with CancellationToken
8. **Streaming begins**: DownloadEngine reads first chunk to detect file signature, corrects extension if needed
9. **FileStream created**: Opens destination file for writing
10. **Data streamed**: DownloadEngine continues reading and writing in 16 KB chunks
11. **Progress tracked**: Every percentage point change fires ProgressChanged event
12. **Speed calculated**: Stopwatch measures 1-second intervals, calculates and reports speed
13. **UI updates**: Dispatcher marshals progress events back to UI thread for safe binding updates
14. **Download completes**: Status changes to Completed, speed indicator shows "Finished"
15. **User views details**: Can click any download row to view detailed information in side panel

## Technologies Used

- **Language**: C# (.NET 8.0)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **XAML**: Declarative UI markup with data binding and styles
- **Networking**: HttpClient with async/await for non-blocking HTTP requests
- **Async/Await**: Asynchronous patterns for responsive UI
- **CancellationToken**: Cooperative cancellation for graceful download termination
- **ObservableCollection**: Automatic DataGrid updates when downloads list changes
- **INotifyPropertyChanged**: Property-level UI binding and synchronization
- **Dispatcher**: Thread-safe UI updates from background download tasks
- **FileStream**: Direct file I/O for efficient disk writes
- **Stream Processing**: Memory-efficient streaming without buffering entire files
- **Event-Driven Architecture**: Progress events, property change notifications

## Design Decisions

### Streaming Over Memory Buffering
Files are streamed directly from the HTTP response to disk in 16 KB chunks rather than loaded entirely into memory. This enables downloading files larger than available RAM and keeps memory usage constant regardless of file size.

### Automatic File Extension Detection
The `ResourceDetector` analyzes file magic numbers (file signatures) from the first bytes of the download to determine the actual file type, independent of HTTP headers. This prevents misnamed files when servers report incorrect content types. Detection supports 25+ common formats (PNG, JPEG, GIF, PDF, ZIP, MP4, etc.).

### Resource Inspection Before Download
`ResourceInspector` performs HEAD requests (falling back to GET if HEAD isn't supported) to fetch metadata without downloading the file. This allows users to review filename, size, and content type before committing to a download.

### ObservableCollection for Automatic UI Updates
Download jobs are stored in `ObservableCollection<DownloadJob>`, which automatically notifies the DataGrid of additions/removals. Combined with `INotifyPropertyChanged`, property changes (progress, speed, status) automatically reflect in the UI without manual refresh calls.

### Event-Driven Progress Reporting
`DownloadEngine` raises `ProgressChanged` events only when the percentage changes (not every byte), reducing UI update overhead. The UI uses `Dispatcher.Invoke()` to safely marshal events from the download thread to the UI thread.

### 500ms Debounced Inspection
When users type URLs, a `DispatcherTimer` with a 500ms delay prevents rapid resource inspections. This reduces network traffic and server load while providing responsive feedback.

### CancellationToken for Graceful Shutdown
Downloads support cancellation through `CancellationTokenSource`, allowing the user to cancel mid-download with immediate effect. The exception is caught and download status is marked as Cancelled.

## Project Structure

```
MiniDownloadManager/
├── Models/
│   ├── DownloadJob.cs              - Download state, progress, and INotifyPropertyChanged
│   ├── DownloadProgressEventArgs.cs - Event data for progress updates
│   └── ResourceInfo.cs             - Remote resource metadata
├── Services/
│   ├── DownloadEngine.cs           - Streaming download implementation
│   ├── ResourceInspector.cs        - HTTP HEAD/GET for metadata
│   └── ResourceDetector.cs         - Magic number-based file detection
├── Views/
│   ├── MainWindow.xaml(.cs)        - Main UI and download management
│   ├── NewDownloadView.xaml(.cs)   - New download dialog with URL input
│   └── DownloadDetailsView.xaml(.cs) - Details panel for selected download
├── Helpers.cs                      - FormatFileSize(), FormatSpeed() utilities
├── App.xaml(.cs)                   - Application bootstrap
├── AssemblyInfo.cs                 - Assembly info
├── MiniDownloadManager.csproj       - Project configuration
└── MiniDownloadManager.sln         - Solution file
```

## Current Limitations

- **Pause/Resume Not Fully Implemented**: Pause button marks download as paused but doesn't support resuming with HTTP Range requests
- **No Download Persistence**: Downloads are cleared when the application closes
- **No Queue Management**: Downloads start immediately; no queue or scheduling system
- **Single-Session Downloads**: No state serialization or recovery after application restart
- **No Retry Mechanism**: Failed downloads don't automatically retry
- **Limited Error Reporting**: Exceptions are caught silently without detailed logging
- **No Bandwidth Throttling**: Downloads use full available bandwidth

## Future Improvements

- **Resume Support**: Implement HTTP Range requests to resume interrupted downloads
- **Download Queue**: Add queue management with configurable concurrency
- **Parallel Downloading**: Support downloading multiple files simultaneously
- **Retry Mechanism**: Automatic retry with exponential backoff for failed downloads
- **Bandwidth Limiting**: Option to cap download speed
- **Settings Persistence**: Save user preferences and download history
- **Comprehensive Logging**: File and console logging for debugging
- **Checksum Verification**: SHA-256 verification against provided hashes
- **Advanced Filtering**: Filter/search downloads by status, name, date
- **Pause/Resume**: Full implementation of pause/resume functionality

## How to Run

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code with C# extension
- Windows OS (WPF is Windows-only)

### From Visual Studio

1. Clone the repository:
   ```bash
   git clone https://github.com/Hamza-El-Beidouri/Mini-Download-Manager.git
   cd Mini-Download-Manager
   ```

2. Open the solution:
   ```bash
   start MiniDownloadManager.sln
   ```

3. Build and run:
   - Press `Ctrl+F5` (or Debug → Start Without Debugging)
   - The application window opens automatically

### From Command Line

```bash
cd MiniDownloadManager
dotnet run
```

## Learning Objectives

This project demonstrates several advanced C# and .NET concepts:

- **Asynchronous Programming**: Async/await patterns for non-blocking I/O, preventing UI freezing during network operations
- **Event-Driven Architecture**: Event handlers, custom EventArgs, and publisher-subscriber patterns
- **Stream Processing**: Efficient handling of binary data streams without loading into memory
- **Separation of Concerns**: Models, Services, and Views organized by responsibility
- **File I/O**: Direct file operations with FileStream for efficient disk writes
- **HTTP Networking**: HttpClient usage, HEAD vs GET requests, content negotiation
- **WPF Data Binding**: Two-way binding, INotifyPropertyChanged, ObservableCollection
- **UI Threading**: Dispatcher for thread-safe updates from background tasks
- **Cooperative Cancellation**: CancellationToken and CancellationTokenSource patterns
- **Real-time UI Updates**: Responsive UI with live progress indication
- **XAML Markup**: Declarative UI with data triggers, styles, and templates
- **Span<T> and Performance**: ReadOnlySpan for zero-copy magic number detection
- **Error Handling**: Exception handling in async contexts with graceful fallbacks

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

**Built with ❤️ as a demonstration of clean architecture and modern C# practices.**
