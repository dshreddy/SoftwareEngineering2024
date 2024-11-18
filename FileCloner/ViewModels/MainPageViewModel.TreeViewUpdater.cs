using FileCloner.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FileCloner.ViewModels;

partial class MainPageViewModel : ViewModelBase
{
    /// <summary>
    /// Resets the file and folder counters and updates UI.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void ResetCounts()
    {
        FileCount = 0;
        FolderCount = 0;
        Node.SelectedFolderCount = 0;
        Node.SelectedFilesCount = 0;
        Node.SumOfSelectedFilesSizeInBytes = 0;
        UpdateCounts();
    }

    /// <summary>
    /// Updates the UI counters for selected files, folders, and their total size.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void UpdateCounts()
    {
        SelectedFoldersCount = Node.SelectedFolderCount;
        SelectedFilesCount = Node.SelectedFilesCount;
        SumofSelectedFilesSizeInBytes = Node.SumOfSelectedFilesSizeInBytes;
    }

    // Static method to retrieve the instance's RootDirectoryPath
    [ExcludeFromCodeCoverage]
    public static string GetRootDirectoryPath()
    {
        // You could retrieve the instance from a service locator or use a singleton pattern if appropriate.
        // For simplicity, let's assume a single instance is created somewhere and stored as a static reference.
        return s_instance?._rootDirectoryPath ?? string.Empty;
    }

    /// <summary>
    /// Adds or removes files to/from _selectedFiles based on checkbox selection.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static void UpdateSelectedFiles(string address, string fullPath, string relativePath, bool isChecked)
    {
        if (address == Constants.IPAddress)
        {
            return;
        }

        string rootDirectoryPath = GetRootDirectoryPath();

        if (isChecked)
        {
            if (!SelectedFiles.ContainsKey(address))
            {
                SelectedFiles[address] = [];
            }
            SelectedFiles[address].Add($"{fullPath}, {Path.Combine(rootDirectoryPath, relativePath)}");
        }
        else
        {
            if (SelectedFiles.ContainsKey(address))
            {
                SelectedFiles[address].Remove($"{fullPath}, {Path.Combine(rootDirectoryPath, relativePath)}");

                // Remove entry if no files are left for the address
                if (SelectedFiles[address].Count == 0)
                {
                    SelectedFiles.Remove(address);
                }
            }
        }
    }

    /// <summary>
    /// Opens a folder picker dialog for selecting a root directory.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void BrowseFolders()
    {
        using var dialog = new CommonOpenFileDialog();
        dialog.IsFolderPicker = true;
        dialog.Title = "Select the Root Directory for File Cloning";

        // If a new directory is selected, update and regenerate the tree
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok && dialog.FileName != RootDirectoryPath)
        {
            RootDirectoryPath = dialog.FileName;
            TreeGenerator(_rootDirectoryPath);
        }
    }

}
