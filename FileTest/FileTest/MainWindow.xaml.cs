using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using File = System.Net.WebRequestMethods.File;


namespace FileTest
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string currentFile = "";
        string savedContent = "";
        string path = "";
        string listPath = "";
        int mruIndex = 0;
        List<string> RecentFileList = new List<string>();
        List<string[]> RecentList = new List<string[]>();
        string[] RecentFileArr;

        public MainWindow()
        {
            listPath = @"C:\Users\kristiyana.za\Desktop\test\List.txt";
            AddLastMRUToList();
            InitializeComponent();
            UpdateTitle();
            FindSeparator();
        }
        private void AddLastMRUToList()
        {
            using (var sr = new StreamReader(listPath))
            {
                string[] RecentFileArr = sr.ReadToEnd().Split('\n').ToArray();             
                RecentFileList = RecentFileArr.ToList();
                RecentFileList.RemoveAt(RecentFileList.Count-1);
            }
        }
        private void FindSeparator()
        {
            object mruObject = MainMenu.FindName("MRU_Beginning");
            mruIndex = MainMenu.Items.IndexOf(mruObject);
        }
        private void AddMenuBetweenSeparator()
        {
            int index = 1;
            for (int i = 0; i < RecentFileList.Count; i++)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Header = $"_{index++}:" + RecentFileList[i];
                menuItem.Click += NewMenuItem1_Click;
                MainMenu.Items.Insert(mruIndex + 1 + i , menuItem);
            }
        }
        private void DeleteMRU()
        {
            object mruObject = MainMenu.FindName("MRU_End");
            int mruIndexEnd = MainMenu.Items.IndexOf(mruObject);

            for (int i = mruIndexEnd - 1; i >= mruIndex; i--)
            {
                Control mi = MainMenu.Items.GetItemAt(i) as Control;
                if (mi.Name == "MRU_Beginning")
                {
                    break;
                }
                else
                {
                    MainMenu.Items.RemoveAt(i);
                }
            }
        }
        private void NewMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            string fileToOpen = "";
            path = "";
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                fileToOpen = menuItem.Header.ToString();
                path = fileToOpen.Remove(0, 3);

            }
            if (String.IsNullOrEmpty(fileToOpen))
                return;

            LoadFile(path);

            currentFile = path;
            UpdateTitle();

            DeleteMRU();
            AddToListMRU();
            SaveLastMRUPath();
            AddMenuBetweenSeparator();

        }
        private void AddToListMRU()
        {
            if (!string.IsNullOrEmpty(currentFile))
            {
                RecentFileList.Remove(currentFile);
                RecentFileList.Insert(0, currentFile);
                //RecentFileList.Add(currentFile);

                while (RecentFileList.Count > 5)
                {
                    RecentFileList.RemoveAt(RecentFileList.Count - 1);
                }
            }
        }
        private void UpdateTitle()
        {
            string title;
            if (string.IsNullOrEmpty(currentFile))
            {
                title = "Notes";
            }
            else
            {
                title = currentFile;
            }

            if (ContentChanged)
            {
                title = "*" + title;
            }

            Title = title;
        }
        private bool ContentChanged
        {
            get { return tbContent.Text != savedContent; }
        }
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (ContentChanged)
            {
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Question;
                MessageBoxResult result = MessageBox.Show("Do you want to save the changes?", "Save changes", button, icon, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    LoadedFileCheckAndSave();
                }
            }
            OpenDialogAndLoadFile();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            LoadedFileCheckAndSave();
            DeleteMRU();
            AddToListMRU();
            AddMenuBetweenSeparator();
            SaveLastMRUPath();
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteFile();
        }
        private void OpenDialogAndLoadFile()
        {
            DeleteMRU();
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Text documents (.txt)|*.txt";

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result != true)
                return;

            LoadFile(dialog.FileName);
            currentFile = dialog.FileName;
            UpdateTitle();

            DeleteMRU();
            AddToListMRU();
            AddMenuBetweenSeparator();
            SaveLastMRUPath();
        }
        private void LoadFile(string path)
        {
            try
            {
                // Open the text file using a stream reader.
                using (var sr = new StreamReader(path))
                {
                    // Read the stream as a string, and write the string to the console.
                    savedContent = sr.ReadToEnd();
                    tbContent.Text = savedContent;
                }
            }
            catch (IOException ex)
            {
                string caption = "Error";
                string messageBoxText = $"The file could not be opened:\n{ex.Message}";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                tbContent.Text = " ";    
            }
        }
        private void SaveFile()
        {
            try
            {
                // Open the text file using a stream writer.
                using (var sw = new StreamWriter(currentFile))
                {
                    sw.Write(tbContent.Text);
                    savedContent = tbContent.Text;
                    UpdateTitle();
                }
            }
            catch (IOException ex)
            {
                
                string caption = "Error";
                string messageBoxText = $"The file could not be opened:\n{ex.Message}";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            } 
        }
        private void LoadedFileCheckAndSave()
        {
            if (string.IsNullOrEmpty(currentFile))
            {
                // Configure open file dialog box
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.Filter = "Text documents (.txt)|*.txt";

                //Show open file dialog box
                bool? result = dialog.ShowDialog();

                //Process open file dialog box results
                if (result != true)
                    return;

                currentFile = dialog.FileName;
            }

            SaveFile();
        }
        private void DeleteFile()
        {
            DeleteMRU();
            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Text documents (.txt)|*.txt";

            //Show open file dialog box
            bool? result = dialog.ShowDialog();

            //Process open file dialog box results
            if (result != true)
                return;

            try
            {
                // Open the text file using a stream reader.
                var sr = new FileInfo(dialog.FileName);
                sr.Delete();
                tbContent.Clear();

            }
            catch (IOException ex)
            {
                string caption = "Error";
                string messageBoxText = $"The file could not be opened:\n{ex.Message}";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
            }
        }
        private void TbContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTitle();
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenDialogAndLoadFile();        
        }

        private void MenuClose_Click(object sender, RoutedEventArgs e)
        {
            SaveLastMRUPath();

            this.Close();
        }
        private void SaveLastMRUPath()
        {
            listPath = @"C:\Users\kristiyana.za\Desktop\test\List.txt";
            if (RecentFileList.Count != 0)
            {
                using (var sw = new StreamWriter(listPath))
                {
                    for(int i = 0; i < RecentFileList.Count; i++)
                    {
                        sw.Write($"{RecentFileList[i]}\n");
                    } 
                }
            }
        }
        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            LoadedFileCheckAndSave();

            DeleteMRU();
            AddToListMRU();
            AddMenuBetweenSeparator();
            SaveLastMRUPath();
        }
        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            DeleteMRU();

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Text documents (.txt)|*.txt";

            bool? result = dialog.ShowDialog();

            currentFile = dialog.FileName;
            UpdateTitle();
            SaveFile();

            DeleteMRU();
            AddToListMRU();
            AddMenuBetweenSeparator();
            SaveLastMRUPath();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            AddMenuBetweenSeparator();
            AddLastMRUToList();
        }
    }
}

