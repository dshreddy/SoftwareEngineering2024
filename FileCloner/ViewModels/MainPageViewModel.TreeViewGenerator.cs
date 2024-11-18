﻿using FileCloner.Models;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace FileCloner.ViewModels;

partial class MainPageViewModel : ViewModelBase
{
    /// <summary>
    /// Generates the initial tree structure of the directory specified in RootDirectoryPath.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private void TreeGenerator(string filePath)
    {
        try
        {
            // Clear any existing nodes in the tree and reset counters
            Tree.Clear();
            ResetCounts();
            if (filePath == Constants.OutputFilePath)
            {
                RootGenerator(filePath);
                return;
            }

            // Generate input file representing the structure of the root directory
            _fileExplorerServiceProvider.GenerateInputFile(RootDirectoryPath);

            // Parse the input file and create tree nodes
            RootGenerator(Constants.InputFilePath);
        }
        catch (Exception e)
        {
            // Show error if tree generation fails
            MessageBox.Show(e.Message);
        }
    }

    /// <summary>
    /// Parses the JSON file representing directory structure and generates the root node.
    /// </summary>
    /// 
    [ExcludeFromCodeCoverage]
    private void RootGenerator(string filePath)
    {
        try
        {
            string jsonContent = File.ReadAllText(filePath);
            Dictionary<string, JsonElement>? rootDictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent);

            foreach (KeyValuePair<string, JsonElement> root in rootDictionary)
            {
                string color = root.Value.TryGetProperty("COLOR", out JsonElement colorProperty) ? colorProperty.GetString() : "";

                // Safely parse the "LAST_MODIFIED" property
                string lastModifiedStr = root.Value.TryGetProperty("LAST_MODIFIED", out JsonElement lastModified) ? lastModified.GetString() : null;
                string lastModifiedFormatted = null;
                if (!string.IsNullOrEmpty(lastModifiedStr) && DateTimeOffset.TryParse(lastModifiedStr, out DateTimeOffset lastModifiedDate))
                {
                    lastModifiedFormatted = lastModifiedDate.LocalDateTime.ToString();
                }

                // Create a root node for each directory
                var rootNode = new Node {
                    Name = root.Key,
                    IsFile = false,
                    IconPath = new Uri(Constants.FolderIconPath, UriKind.Absolute),
                    Color = color,
                    LastModified = lastModifiedFormatted ?? "Unknown", // Default to "Unknown" if parsing fails
                    RelativePath = root.Value.TryGetProperty("RELATIVE_PATH", out JsonElement relativePath) ? relativePath.GetString() : "",
                    IpAddress = root.Value.TryGetProperty("ADDRESS", out JsonElement address) ? address.GetString() : null,
                    FullFilePath = root.Value.TryGetProperty("FULL_PATH", out JsonElement fullFilePath) ? fullFilePath.GetString() : "PATH NOT GIVEN!",
                };

                // Add root node to the tree and increment folder count
                Tree.Add(rootNode);
                FolderCount++;
                PopulateChildren(rootNode, (JsonElement)root.Value);  // Recursively populate child nodes
            }
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred: {e.Message}", e);
        }

    }

    /// <summary>
    /// Recursively populates child nodes for a given parent node.
    /// </summary>
    /// 
    [ExcludeFromCodeCoverage]
    private void PopulateChildren(Node parentNode, JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("CHILDREN", out JsonElement childrenElement))
        {
            foreach (JsonProperty child in childrenElement.EnumerateObject())
            {
                bool isFile = child.Value.TryGetProperty("SIZE", out JsonElement sizeElement);
                string color = child.Value.TryGetProperty("COLOR", out JsonElement colorProperty) ? colorProperty.GetString() : "";
                var childNode = new Node {
                    Name = child.Name,
                    Color = color,
                    IpAddress = child.Value.TryGetProperty("ADDRESS", out JsonElement address) ? address.GetString() : "localhost",
                    FullFilePath = child.Value.TryGetProperty("FULL_PATH", out JsonElement fullFilePath) ? fullFilePath.GetString() : "PATH NOT GIVEN!",
                    LastModified = DateTimeOffset.Parse(child.Value.TryGetProperty("LAST_MODIFIED", out JsonElement lastModified) ? lastModified.GetString() : "").LocalDateTime.ToString(),
                    IsFile = isFile,
                    Size = isFile ? sizeElement.GetInt32() : 0,
                    Parent = parentNode,
                    RelativePath = child.Value.TryGetProperty("RELATIVE_PATH", out JsonElement relativePath) ? relativePath.GetString() : "",
                    IconPath = new Uri(isFile ? Constants.FileIconPath : Constants.FolderIconPath, UriKind.Absolute)
                };
                if (color == "GREEN" || color == "RED")
                {
                    childNode.IsChecked = true;
                    childNode.CheckBoxClick();
                }
                parentNode.Children.Add(childNode);

                // Increment counters based on node type
                if (isFile)
                {
                    FileCount++;
                }
                else
                {
                    FolderCount++;
                    PopulateChildren(childNode, child.Value); // Recurse if it's a folder
                }
            }
        }
    }

}
