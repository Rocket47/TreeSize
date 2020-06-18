using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreeSize
{    
    public partial class Form1 : Form
    {
        ListViewItem item;
        List<DirectoryInfo> saveList = new List<DirectoryInfo>();
        public delegate void MethodContainer();
        public Form1()
        {
            InitializeComponent();
            PopulateTreeView();            
            this.treeView1.NodeMouseClick +=
            new TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            
        }

        public void UpdateSize()
        {

        }

        private void PopulateTreeView()
        {
            TreeNode rootNode;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in allDrives)
            {
                DirectoryInfo info = new DirectoryInfo(@driveInfo.Name);
                if (info.Exists)
                {
                    rootNode = new TreeNode(info.Name);
                    rootNode.Tag = info;
                    GetDirectories(info.GetDirectories(), rootNode);
                    treeView1.Nodes.Add(rootNode);
                }
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs,
            TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            foreach (DirectoryInfo subDir in subDirs)
            {
                try
                {
                    subDir.GetFiles();
                }
                catch (UnauthorizedAccessException ex)
                {
                    continue;
                }
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        void treeView1_NodeMouseClick(object sender,
    TreeNodeMouseClickEventArgs e)
        {            
            TreeNode newSelected = e.Node;
            listview.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems = new ListViewItem.ListViewSubItem[] { };
            ListViewItem item = null;
            FillDirectory(nodeDirInfo);                   
            //FillFiles(nodeDirInfo);            
        }

        public long DirSize(DirectoryInfo d)
        {
            long size = 0;
            try
            {
                FileInfo[] fis = d.GetFiles();
                if (fis.Length != 0)
                {
                    foreach (FileInfo fi in fis)
                    {
                        //listview.Items[index].SubItems[3].Text = size.ToString();
                        size += fi.Length;
                    }
                }
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    listview.Items[0].SubItems[3].Text = size.ToString();
                    size += DirSize(di);                    
                }
                return size;
            }
            catch (UnauthorizedAccessException ex)
            {
                return 0;
            }

        }

        public void FillDirectory(DirectoryInfo nodeDirInfo)
        {
            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {                
                try
                {                    
                    Thread thread = new Thread(() =>
                    {
                        Invoke((Action)(() => SetInfoDirectory(dir)));                                           
                    });                    
                    thread.Start();
                    saveList.Add(dir);
                }
                catch (UnauthorizedAccessException ex)
                {
                    continue;
                }
            }            
        }

        public void UpdateSize(DirectoryInfo nodeDirInfo)
        {
            foreach (DirectoryInfo dir in saveList)
            {                
                try
                {
                    Thread thread = new Thread(() =>
                    {
                        Invoke((Action)(() => DirSize(dir)));
                    });
                    thread.Start();
                }
                catch (UnauthorizedAccessException ex)
                {
                    continue;
                }
            }
        }


        public void SetInfoDirectory(DirectoryInfo dir)
        {
            int index = 0;
            item = new ListViewItem(dir.Name, 0);
            item.SubItems.Add("Directory");
            item.SubItems.Add(dir.LastAccessTime.ToShortDateString());          
           // ListViewItem.ListViewSubItem[] subItems = new ListViewItem.ListViewSubItem[]
           //{ new ListViewItem.ListViewSubItem(item, "Directory"),
           //  new ListViewItem.ListViewSubItem(item,
           //      dir.LastAccessTime.ToShortDateString()),
           //  new ListViewItem.ListViewSubItem(item, DirSize(dir).ToString()),
           //  new ListViewItem.ListViewSubItem(item, GetCountDirectories(dir).ToString())};
            //item.SubItems.AddRange(subItems);            
            listview.Items.Add(item);
            index = listview.Items.Count - 1;                       
        }               

        public int GetCountDirectories(DirectoryInfo dir)
        {
            int countDirectories;
            try
            {
                countDirectories = Directory.EnumerateDirectories(dir.FullName, "*",
                     SearchOption.AllDirectories).Count();
            }
            catch (UnauthorizedAccessException ex)
            {
                return 0;
            }

            return countDirectories;
        }

        public void FillFiles(DirectoryInfo nodeDirInfo)
        {
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                ListViewItem item = new ListViewItem(file.Name, 1);
                ListViewItem.ListViewSubItem[] subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, "File"),
             new ListViewItem.ListViewSubItem(item,
                file.LastAccessTime.ToShortDateString()), new ListViewItem.ListViewSubItem(item,
                file.Length.ToString()) };

                item.SubItems.AddRange(subItems);
                listview.Items.Add(item);
            }
        }
    }
}

