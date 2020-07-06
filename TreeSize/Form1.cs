using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreeSize
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            PopulateTreeView();
        }

        //@///////////////////////////////////////////////////
        private void PopulateTreeView()
        {
            TreeNode rootNode;

            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in allDrives)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(drive.Name);
                if (directoryInfo.Exists)
                {
                    rootNode = new TreeNode(directoryInfo.Name);
                    rootNode.Tag = directoryInfo;
                    GetDirectories(directoryInfo.GetDirectories(), rootNode);
                    treeView.Nodes.Add(rootNode);
                }
            }
        }
        //@///////////////////////////////////////////////////
        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode node;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {               
                node = new TreeNode(subDir.Name, 0, 0);
                node.Tag = subDir;
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, node);
                }
                nodeToAddTo.Nodes.Add(node);
            }
        }
    }
}
