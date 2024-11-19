/******************************************************************************
 * Filename    = MainPageViewModel.Properties.cs
 *
 * Author(s)      = Sai Hemanth Reddy & Sarath A
 * 
 * Project     = FileCloner
 *
 * Description = Defines all the properties that are to be viewed in the UI.
 *****************************************************************************/


using FileCloner.Models.NetworkService;
using FileCloner.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FileCloner.ViewModels;

partial class MainPageViewModel : ViewModelBase
{
    // Bindable property for the root directory path
    private string _rootDirectoryPath;
    public string RootDirectoryPath
    {
        get => _rootDirectoryPath;
        set {
            _rootDirectoryPath = value;
            OnPropertyChanged(nameof(RootDirectoryPath));
        }
    }

    //Keeps track of the node which is currently selected.
    private Node _selectedNode;
    public Node SelectedNode
    {
        get => _selectedNode;
        set {
            _selectedNode = value;
            OnPropertyChanged(nameof(SelectedNode));
        }
    }

    // Property to track the number of files in the directory
    private int _fileCount;
    public int FileCount
    {
        get => _fileCount;
        set {
            _fileCount = value;
            OnPropertyChanged(nameof(FileCount));
        }
    }

    // Property to track the number of folders in the directory
    private int _folderCount;
    public int FolderCount
    {
        get => _folderCount;
        set {
            _folderCount = value;
            OnPropertyChanged(nameof(FolderCount));
        }
    }

    // Property to track the count of selected folders in the UI
    private int _selectedFoldersCount;
    public int SelectedFoldersCount
    {
        get => _selectedFoldersCount;
        set {
            _selectedFoldersCount = value;
            OnPropertyChanged(nameof(SelectedFoldersCount));
        }
    }

    // Property to track the count of selected files in the UI
    private int _selectedFilesCount;
    public int SelectedFilesCount
    {
        get => _selectedFilesCount;
        set {
            _selectedFilesCount = value;
            OnPropertyChanged(nameof(SelectedFilesCount));
        }
    }

    // Property to track the total size of selected files in bytes
    private long _sumOfSelectedFilesSizeInBytes;
    public long SumofSelectedFilesSizeInBytes
    {
        get => _sumOfSelectedFilesSizeInBytes;
        set {
            _sumOfSelectedFilesSizeInBytes = value;
            OnPropertyChanged(nameof(SumofSelectedFilesSizeInBytes));
        }
    }

    //Properties which check if one of the four command buttons
    //are enabled or not.
    private bool _isSendRequestEnabled;
    public bool IsSendRequestEnabled
    {
        get => _isSendRequestEnabled;
        set {
            _isSendRequestEnabled = value;
            OnPropertyChanged(nameof(IsSendRequestEnabled));
        }
    }
    private bool _isSummarizeEnabled;
    public bool IsSummarizeEnabled
    {
        get => _isSummarizeEnabled;
        set {
            _isSummarizeEnabled = value;
            OnPropertyChanged(nameof(IsSummarizeEnabled));
        }
    }
    private bool _isStartCloningEnabled;
    public bool IsStartCloningEnabled
    {
        get => _isStartCloningEnabled;
        set {
            _isStartCloningEnabled = value;
            OnPropertyChanged(nameof(IsStartCloningEnabled));
        }
    }
    private bool _isStopSessionEnabled;
    public bool IsStopSessionEnabled
    {
        get => _isStopSessionEnabled;
        set {
            _isStopSessionEnabled = value;
            OnPropertyChanged(nameof(IsStopSessionEnabled));
        }
    }

    // UI Commands for button actions
    public ICommand BrowseFoldersCommand { get; }
    public ICommand SendRequestCommand { get; }
    public ICommand SummarizeCommand { get; }
    public ICommand StartCloningCommand { get; }
    public ICommand StopSessionCommand { get; }

    // Collection to store log messages for display
    public ObservableCollection<string> LogMessages { get; set; } = [];

    // Tree structure of nodes representing files and directories
    public ObservableCollection<Node> Tree { get; set; }
    private readonly FileExplorerServiceProvider _fileExplorerServiceProvider;

    // Dictionary to store selected files, with address as key and list of file paths as value
    public static Dictionary<string, List<string>> SelectedFiles = [];

    // Client and Server instances for network communication
    private readonly Client _client;
    private readonly Server _server;

    // Singleton instance (assuming this pattern is suitable for your use case)
    private static MainPageViewModel? s_instance;
}
