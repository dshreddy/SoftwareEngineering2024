/******************************************************************************
 * Filename    = MainPageViewModel.cs
 *
 * Author      = Sai Hemanth Reddy
 * 
 * Project     = FileCloner
 *
 * Description = Constructor for ViewModel for MainPage, handling directory loading,
 *               file structure generation, and managing file/folder selection states.
 *               Implements commands for sending requests, summarizing responses, 
 *               and starting file cloning.
 *****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.IO;
using FileCloner.Models;
using FileCloner.Models.NetworkService;

namespace FileCloner.ViewModels;
[ExcludeFromCodeCoverage]

/// <summary>
/// ViewModel for the MainPage, handling file operations, commands, and UI bindings.
/// </summary>
partial class MainPageViewModel : ViewModelBase
{
    /// <summary>
    /// Constructor for MainPageViewModel - initializes paths, commands, and communication setup.
    /// </summary>
    public MainPageViewModel()
    {
        s_instance = this; // Set instance for static access

        if (!Directory.Exists(Constants.DefaultFolderPath))
        {
            Directory.CreateDirectory(Constants.DefaultFolderPath);
        }
        // Set default root directory path
        RootDirectoryPath = Constants.DefaultFolderPath;

        // Initialize FileExplorerServiceProvider to manage file operations
        _fileExplorerServiceProvider = new FileExplorerServiceProvider();

        // Initialize UI commands and their respective handlers
        SendRequestCommand = new RelayCommand(SendRequest);
        SummarizeCommand = new RelayCommand(SummarizeResponses);
        StartCloningCommand = new RelayCommand(StartCloning);
        BrowseFoldersCommand = new RelayCommand(BrowseFolders);
        StopSessionCommand = new RelayCommand(StopSession);

        //For watching files and updating any changes in the UI accordingly
        Thread fileWatcherThread = new(() => WatchFile(RootDirectoryPath));
        fileWatcherThread.IsBackground = true;
        fileWatcherThread.Start();

        //Only SendRequest button will be enabled in the beginning
        IsSendRequestEnabled = true;
        IsSummarizeEnabled = false;
        IsStartCloningEnabled = false;
        IsStopSessionEnabled = false;

        // Subscribe to CheckBoxClickEvent to update selection counts when a checkbox is clicked
        Node.CheckBoxClickEvent += UpdateCounts;

        // Initialize the Tree structure representing files and folders
        Tree = [];
        TreeGenerator(_rootDirectoryPath);  // Load the initial tree structure

        // Initialize server and client for handling file transfer communication
        _server = Server.GetServerInstance(UpdateLog);
        _client = new Client(UpdateLog);
    }
}
