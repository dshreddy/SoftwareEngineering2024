/******************************************************************************
 * Filename    = FileClonerLoggerUnitTests.cs
 *
 * Author(s)   = Neeraj Krishna N
 * 
 * Project     = FileClonerForUnitTests
 *
 * Description = UnitTests for FileClonerLogger
 *****************************************************************************/

using FileCloner.FileClonerLogging;

namespace FileClonerUnitTests;

[TestClass]
public class FileClonerLoggerTests
{
    private FileClonerLogger? _logger;
    private string? _logFile;

    [TestInitialize]
    public void Setup()
    {
        _logger = new("FileClonerLoggerUnitTests");
    }

    [TestMethod]
    public void LoggingTest()
    {
        Assert.IsNotNull(_logger);
        string infoLog = "Logging Info to Log File";
        _logger.Log(infoLog);
        _logFile = _logger.LogFile;
        string logFileContents = File.ReadAllText(_logFile);

        bool result = logFileContents.Contains(infoLog);
        Assert.IsTrue( result, "Log File does not contain the desired contents" );

        string errorLog = "Error Log info to log file";
        _logger.Log(errorLog, isErrorMessage: true);

        logFileContents = File.ReadAllText(_logFile);
        result = logFileContents.Contains(errorLog);
        result = result && logFileContents.Contains("ERROR");
        
        Assert.IsTrue( result, "Log File does not contain the desired contents" );


    }


    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_logFile))
        {
            File.Delete(_logFile);
        }
    }
}
