/******************************************************************************
 * Filename    = DiffGenerator.cs
 *
 * Author(s)      = Evans Samuel Biju
 * 
 * Project     = FileCloner
 *
 * Description = Creates a diff file
 *****************************************************************************/



using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using FileCloner.Models.DiffGenerator;

namespace FileCloner.Models.DiffGenerator;
[ExcludeFromCodeCoverage]
public class DiffGenerator
{
    private string _diffFilePath;
    private readonly object _syncLock = new();

    public DiffGenerator(string diffFilePath)
    {
        _diffFilePath = diffFilePath;
    }

    public void GenerateSummary(List<string> jsonFiles)
    {
        Dictionary<string, FileMetadata> allFiles = new();

        for (int i = 0; i < jsonFiles.Count; i++)
        {
            try
            {
                string file = jsonFiles[i];



                string ipAddress = Path.GetFileNameWithoutExtension(file);
                string text = File.ReadAllText(file);
                Root jsonRoot = JsonSerializer.Deserialize<Root>(text);

                foreach (string rootKey in jsonRoot.Files.Keys)
                {
                    JsonElement jsonElement = jsonRoot.Files[rootKey]; // This is a JsonElement for root key (e.g., "A", "B")
                    // Deserialize JsonElement into FileMetadata
                    FileMetadata? rootFile = JsonSerializer.Deserialize<FileMetadata>(jsonElement.GetRawText());
                    rootFile.Address = ipAddress;

                    // Process the children of this root
                    if (rootFile?.Children != null)
                    {

                        if (i == 0)
                        {
                            ProcessChildren(rootFile.Children, allFiles, rootFile.Address, "White", rootKey);
                        }
                        else
                        {
                            ProcessChildren(rootFile.Children, allFiles, rootFile.Address, "#90ee90", rootKey);
                        }
                    }
                    else
                    {
                        Console.WriteLine("null");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading or deserializing file {jsonFiles[i]}: {ex.Message}");
            }
        }

        // Write all files to the output file
        WriteAllFilesToFile(allFiles, _diffFilePath);

    }
    // Recursive method to process children
    public void ProcessChildren(Dictionary<string, FileMetadata> children, Dictionary<string, FileMetadata> allFiles, string iPaddress, string color, string rootName)
    {


        foreach ((string fileName, FileMetadata fileData) in children)
        {

            fileData.InitDirectoryName = rootName;
            if (fileData.Children.Count > 0)
            {
                // If this is a folder, recursively process its children
                fileData.Address = iPaddress;

                ProcessChildren(fileData.Children, allFiles, fileData.Address, color, rootName);
            }
            else
            {
                //we need relative file name ,only then it makes sense
                string relativeFileName = "";
                bool encountered = false;
                foreach (string file in fileData.FullPath.Split('\\').ToList())
                {

                    if (encountered == true)
                    {
                        relativeFileName = relativeFileName + "\\" + file;
                    }
                    if (file == fileData.InitDirectoryName)
                    {

                        encountered = true;
                    }

                }



                // If this is a file, add or update it in the allFiles dictionary
                if (allFiles.TryGetValue(relativeFileName, out FileMetadata? existingFile))
                {
                    if (fileData.LastModified > existingFile.LastModified)
                    {
                        if (fileData.Color != "#90ee90")
                        {
                            allFiles[relativeFileName] = fileData;
                            allFiles[relativeFileName].Color = "#ffff00";
                        }
                        else
                        {
                            allFiles[relativeFileName] = fileData;
                            fileData.Color = "Green";
                        }

                    }
                }
                else
                {
                    allFiles[relativeFileName] = fileData;
                    allFiles[relativeFileName].Color = color;
                }
                fileData.Address = iPaddress;
                fileData.InitDirectoryName = rootName;
            }
        }
    }


    public void Add_to_Tree(Node node, List<string> fullPath, int index, FileMetadata fileMetaData, string pathSoFar)
    {
        if (index == fullPath.Count)
        {

            node.Size = fileMetaData.Size;

        }
        else
        {
            node.Size = 0;
        }

        if (index >= fullPath.Count)
        {
            return;
        }

        if (fileMetaData.Color == "#ffff00")
        {
            node.Color = "#ffff00";
        }
        else if (fileMetaData.Color == "#90ee90" && node.Color != "#ffff00")
        {
            node.Color = "#90ee90";
        }

        if (node._children.ContainsKey(fullPath[index]))
        {
            node = node._children[fullPath[index]];
            Add_to_Tree(node, fullPath, index + 1, fileMetaData, node.FullPath);
        }
        else
        {

            node._children[fullPath[index]] = new Node(fullPath[index], fileMetaData);


            node.LastModified = node.LastModified > fileMetaData.LastModified
                ? node.LastModified
                : fileMetaData.LastModified;

            node = node._children[fullPath[index]];
            node.FullPath = pathSoFar + "\\" + fullPath[index];

            for (int i = 0; i <= index; i++)
            {
                {
                    node.RelativePaths = node.RelativePaths + "\\" + fullPath[i];
                }
                Add_to_Tree(node, fullPath, index + 1, fileMetaData, node.FullPath);
            }
        }
    }


    public void WriteAllFilesToFile(Dictionary<string, FileMetadata> files, string outputFilePath)
    {
        // Creating a dictionary to store the final tree structure
        Dictionary<string, Node> tree_address = new();
        lock (_syncLock)
        {
            using StreamWriter writer = new StreamWriter(outputFilePath);
            foreach ((string fileName, FileMetadata fileData) in files)
            {
                List<string> result = fileData.FullPath.Split('\\').ToList();


                string absPath = "";
                int count = 0;
                foreach (string folderName in result)
                {

                    count += 1;
                    if (folderName == fileData.InitDirectoryName)
                    {
                        absPath = absPath + "\\" + folderName;
                        break;
                    }
                    else
                    {
                        absPath = absPath + "\\" + folderName;
                    }
                }

                result = result.Skip(count - 1).ToList();



                if (tree_address.ContainsKey(absPath))
                {

                    Add_to_Tree(tree_address[absPath], result, 1, fileData, tree_address[absPath].FullPath);
                }
                else
                {
                    // If not, create a new node and add it to the tree
                    tree_address[absPath] = new Node(result[0], fileData) {
                        FullPath = absPath
                    };
                    Add_to_Tree(tree_address[absPath], result, 1, fileData, tree_address[absPath].FullPath);
                }
            }

            // Start writing the tree structure to the file
            writer.WriteLine("{");

            bool firstNode = true;
            foreach (KeyValuePair<string, Node> entry in tree_address)
            {
                if (!firstNode)
                {
                    writer.WriteLine(",");
                }
                firstNode = false;
                WriteNode(writer, entry.Value, 0);
            }


            writer.WriteLine("}");
        }

    }

    public void WriteNode(StreamWriter writer, Node node, int indentLevel)
    {
        // Create indentation for better readability
        string indent = new string(' ', indentLevel * 2);

        // Write the node as a JSON-like structure
        writer.WriteLine($"{indent}\"{node._node_name}\": {{");
        writer.WriteLine($"{indent}  \"LAST_MODIFIED\": \"{node.LastModified:MM-dd-yyyy}\",");
        writer.WriteLine($"{indent}  \"FULL_PATH\": \"{node.FullPath}\",");
        writer.WriteLine($"{indent}  \"COLOR\": \"{node.Color}\",");
        writer.WriteLine($"{indent}  \"ADDRESS\": \"{node.IpAddress}\",");
        writer.WriteLine($"{indent}  \"NODE_SIZE\": \"{node.Size}\"");
        writer.WriteLine($"{indent}  \"RELATIVE_PATH\": \"{node.RelativePaths}\"");


        if (node._children.Count > 0)
        {
            // If there are children, recursively write them
            writer.WriteLine($"{indent}  \"CHILDREN\": {{");

            bool firstChild = true;
            foreach (KeyValuePair<string, Node> child in node._children)
            {
                if (!firstChild)
                {
                    writer.WriteLine(",");
                }
                firstChild = false;

                WriteNode(writer, child.Value, indentLevel + 1);
            }

            writer.WriteLine($"{indent}  }}");
        }


        writer.WriteLine($"{indent}}}");
    }

}
