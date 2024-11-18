using System.Diagnostics.CodeAnalysis;

namespace FileCloner.ViewModels;

partial class MainPageViewModel : ViewModelBase
{
    private readonly object _writeLock = new();

    /// <summary>
    /// Adds a message to the log with timestamp for UI display.
    /// </summary>

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
