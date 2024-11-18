using FileCloner.Models.DiffGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloner.Models.DiffGenerator;

    public class Node
    {
        public string _node_name;
        public Dictionary<string, Node> _children;
        public DateTime LastModified { get; set; }
        public string IpAddress { get; set; }
        public string Color { get; set; }

        public string FullPath { get; set; }

        public int? Size { get; set; }

        public string RelativePaths { get; set; }

        public Node(string node_name, FileMetadata fileContents)
        {
            this._node_name = node_name;
            this._children = new Dictionary<string, Node>();
            this.LastModified = fileContents.LastModified;
            this.IpAddress = fileContents.Address;
            this.Color = "White";


        }

    }

