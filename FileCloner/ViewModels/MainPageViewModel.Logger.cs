/******************************************************************************
 * Filename    = MainPageViewModel.Logger.cs
 *
 * Author(s)      = Sai Hemanth Reddy & Sarath A
 * 
 * Project     = FileCloner
 *
 * Description = Updates the log by adding a new log to the list of logs, and 
 *               updating the UI at the same time.
 *****************************************************************************/
using System.Diagnostics.CodeAnalysis;
namespace FileCloner.ViewModels;

partial class MainPageViewModel : ViewModelBase
{
    private readonly object _writeLock = new();

    /// <summary>
    /// Adds a message to the log with timestamp for UI display.
    /// </summary>
    /// <param name="message">Message to be updated in the UI</param>
    [ExcludeFromCodeCoverage]
    private void UpdateLog(string message)
    {
        Dispatcher.Invoke(() => {
            lock (_writeLock)
            {
                LogMessages.Insert(0, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]-  {message}");
                OnPropertyChanged(nameof(LogMessages));
            }
        });
    }

}
