/******************************************************************************
 * Filename    = MainPageViewModel.FileWatcher.cs
 *
 * Author(s)      = Sai Hemanth Reddy & Sarath A
 * 
 * Project     = FileCloner
 *
 * Description = Updates the UI if any file gets cloned or gets newly created
 *               in the directory which is set now.
 *****************************************************************************/
using System.Diagnostics;
using System.IO;

namespace FileCloner.ViewModels;

partial class MainPageViewModel : ViewModelBase
{
    /// <summary>
    /// Watches the file present at the given path.
    /// </summary>
    /// <param name="path">Since that is the file we are viewing at UI. Path will be the root directory path</param>
    public void WatchFile(string path)
    {
        Trace.WriteLine($"Started watching at {path}");
        using FileSystemWatcher watcher = new();
        watcher.Path = path;

        //The following changes are notified to the watcher
        watcher.NotifyFilter = NotifyFilters.Attributes |
        NotifyFilters.DirectoryName |
        NotifyFilters.FileName |
        NotifyFilters.LastWrite |
        NotifyFilters.Size;

        //Watching all kinds of files. * is a wildcard which means changes in all files
        //with all extensions are watched upon.
        watcher.Filter = "*.*";

        //Setting event handlers for the changes
        watcher.Created += new FileSystemEventHandler(OnChanged);
        watcher.Deleted += new FileSystemEventHandler(OnChanged);
        watcher.Renamed += new RenamedEventHandler(OnRenamed);

        watcher.EnableRaisingEvents = true;
        //Watch for as long as the UI is kept running
        while (true)
        {
            ;
        }
    }

    //Update the UI as and when the name of an object is changed.
    [ExcludeFromCodeCoverage]
    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        Dispatcher.Invoke(() => {
            TreeGenerator(_rootDirectoryPath);
        });
    }

    //Update the UI as and when the a file is created or deleted.
    [ExcludeFromCodeCoverage]
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        Dispatcher.Invoke(() => {
            TreeGenerator(_rootDirectoryPath);
        });
    }
}