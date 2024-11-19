### Steps to Use `--collect` for Code Coverage

1. **Add Coverlet Collector to Your Project**
   Ensure you have the `coverlet.collector` package installed in your test project. Run:
   ```bash
   dotnet add package coverlet.collector
   ```

2. **Run Tests with Code Coverage**
   Use the `--collect` option with the `XPlat Code Coverage` data collector:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. **Find the Coverage Report**
   After running the command, you’ll see output like:
   ```
   Attachments:
     /path/to/TestResults/<guid>/coverage.cobertura.xml
   ```
   This XML file contains the code coverage data in Cobertura format.

4. **Generate a Human-Readable Report**
   Use a tool like **ReportGenerator** to transform the Cobertura XML into an HTML report:
   - Install ReportGenerator as a global tool:
     ```bash
     dotnet tool install -g dotnet-reportgenerator-globaltool
     ```
   - Generate the report:
     ```bash
     reportgenerator -reports:TestResults/**/*.cobertura.xml -targetdir:CoverageReport
     ```
   - Open the HTML report in your browser:
     ```bash
     start CoverageReport/index.html
     ```

5. **Optional Configuration**
   If you want to customize the output format (e.g., `json`, `opencover`), you can configure Coverlet in your project file or use `dotnet test` options.