using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreeSize
{
    public partial class Form1 : Form
    {
        int saveCountDrives = 0; //use in the progressBar
        public Form1()
        {
            InitializeComponent();
            this.treeView.NodeMouseClick +=
                new TreeNodeMouseClickEventHandler(this.TreeView_NodeMouseClick);
            progressBar.Value += 5;
            PopulateTreeView();
        }

        //@///////////////////////////////////////////////////
        private async void PopulateTreeView()
        {
            TreeNode rootNode;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            saveCountDrives = allDrives.Length;

            foreach (DriveInfo drive in allDrives)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(drive.Name);
                if (directoryInfo.Exists)
                {
                    rootNode = new TreeNode(directoryInfo.Name);
                    rootNode.Tag = directoryInfo;
                    await Task.Run(() =>
                    {
                        GetDirectories(directoryInfo.GetDirectories(), rootNode);
                    });
                    treeView.Nodes.Add(rootNode);
                    LoadingProgressBar();
                }
            }
            progressBar.Value = progressBar.Maximum;
            labelLoading.Text = "Complete";
        }
        //@///////////////////////////////////////////////////
        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode node;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                if (CheckIsHiddenDirectory(subDir))
                {
                    continue;
                }
                try
                {
                    if (CheckIsDirectory(subDir))
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
                catch (UnauthorizedAccessException)
                {
                    break;
                }
            }
        }

        //@///////////////////////////////////////////////////
        private bool CheckIsDirectory(DirectoryInfo subDir)
        {
            FileAttributes attr = File.GetAttributes(subDir.FullName);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //@///////////////////////////////////////////////////
        private bool CheckIsHiddenDirectory(DirectoryInfo subDir)
        {
            FileAttributes attr = File.GetAttributes(subDir.FullName);

            if ((attr & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //@/////////////////////////////////////////////////////
        private void LoadingProgressBar()
        {
            if (progressBar.InvokeRequired)
            {

                progressBar.Invoke(new Action(LoadingProgressBar));
            }
            else
            {
                progressBar.Value += 100 / saveCountDrives;
            }
        }

        //@/////////////////////////////////////////////////////
        private void TreeView_NodeMouseClick(object sender,
            TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listView.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                if (CheckIsHiddenDirectory(dir)) { continue; }
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                    {
                        new ListViewItem.ListViewSubItem(item, "Directory"),
                        new ListViewItem.ListViewSubItem(item,
                        dir.LastAccessTime.ToShortDateString()),
                        new ListViewItem.ListViewSubItem(item, GetSizeDirectory(dir.FullName).ToString()),
                        new ListViewItem.ListViewSubItem(item, GetCountDirectory(dir.FullName).ToString())
                    };
                item.SubItems.AddRange(subItems);
                listView.Items.Add(item);
            }

            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                {
                    new ListViewItem.ListViewSubItem(item, "File"),
                    new ListViewItem.ListViewSubItem(item,
                    file.LastAccessTime.ToShortDateString()),
                    new ListViewItem.ListViewSubItem(item, GetSizeFile(file.FullName).ToString()),
                    new ListViewItem.ListViewSubItem(item, "---")};
                item.SubItems.AddRange(subItems);
                listView.Items.Add(item);
            }

            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
        //@///////////////////////////////////////////////////////
        private long GetSizeDirectory(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            FileAttributes attr = File.GetAttributes(info.FullName);
            long totalSize = info.EnumerateFiles().Sum(file => file.Length);
            return totalSize;
        }
        //@///////////////////////////////////////////////////////
        private long GetSizeFile(string path)
        {
            long totalSize = new FileInfo(path).Length;
            return totalSize;
        }
        //@///////////////////////////////////////////////////////
        private int GetCountDirectory(string path)
        {
            int filesCount = Directory.EnumerateFiles(path).Count();
            return filesCount;
        }
    }
}

