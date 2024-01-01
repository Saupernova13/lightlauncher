using SharpDX.Multimedia;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace lightlauncher
{
    public partial class customFileDialog : Window
    {
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        public List<string> currentDirFiles = new List<string>();
        public List<string> currentDirFolders = new List<string>();
        public string currentDir = string.Empty;
        public string previousDir = string.Empty;
        public static MainWindow mainWindow;
        public static string selectedItem = string.Empty;
        //public bool isCoverArt;
        public string getSelectedItem() { 
            return selectedItem;
        }
        public customFileDialog(MainWindow mw)
        {
            InitializeComponent();
            usersController = new Controller(UserIndex.One);
            controllerThread = new Thread(pollControllerState);
            controllerThread.IsBackground = true;
            controllerThread.Start();
            fileDirectory_listBox.SelectedIndex = 0;
            mainWindow = mw;
            loadCurrentDirItems();
        }
        public void pollControllerState()
        {
            while (running)
            {
                if (!usersController.IsConnected)
                {
                    customMessageBox csm = new customMessageBox(mainWindow, "Error", "No controller is not detected! Please make sure you are using an Xbox or XInput Compatible Controller.");
                    csm.Show();
                    killProgram();
                    return;
                }
                else
                {
                    State state = usersController.GetState();
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
                    {
                        Dispatcher.Invoke(moveCursorUp);
                    }
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
                    {
                        Dispatcher.Invoke(moveCursorDown);
                    }
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
                    {
                        Dispatcher.Invoke(() => openPreviousDirectory());
                    }
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                    {
                        Dispatcher.Invoke(() => selectOption());
                    }
                    Thread.Sleep(180);
                }
            }
        }
        public void openPreviousDirectory()
        {
            try
            {

                previousDir = filePathURL_textBox.Text;
                previousDir = previousDir.Substring(0, previousDir.LastIndexOf('\\'));
                filePathURL_textBox.Text = previousDir;
                loadCurrentDirItems();
            }
            catch (Exception)
            {
                customMessageBox csm = new customMessageBox(mainWindow, "Error", "The current folder is not contained in another folder.");
                csm.Show();
                return;
            }
        }
        public void moveCursorUp()
        {
            if (fileDirectory_listBox.SelectedIndex > 0)
            {
                fileDirectory_listBox.SelectedIndex = fileDirectory_listBox.SelectedIndex - 1;
                fileDirectory_listBox.ScrollIntoView(fileDirectory_listBox.SelectedItem);
            }
        }
        public void moveCursorDown()
        {
            if (fileDirectory_listBox.SelectedIndex < fileDirectory_listBox.Items.Count - 1)
            {
                fileDirectory_listBox.SelectedIndex = fileDirectory_listBox.SelectedIndex + 1;
                fileDirectory_listBox.ScrollIntoView(fileDirectory_listBox.SelectedItem);
            }
        }
        public void selectOption()
        {
            List<bool> isFolder = new List<bool>();
            for (int i = 0; i < currentDirFolders.Count; i++)
            {
                isFolder.Add(true);
            }
            for (int i = 0; i < currentDirFiles.Count; i++)
            {
                isFolder.Add(false);
            }
            if (isFolder.Count == fileDirectory_listBox.Items.Count)
            {

                if (isFolder[fileDirectory_listBox.SelectedIndex])
                {
                    filePathURL_textBox.Text = currentDir + "\\" + currentDirFolders[fileDirectory_listBox.SelectedIndex];
                    loadCurrentDirItems();
                }
                else
                {
                    selectedItem = currentDir + "\\" + currentDirFiles[fileDirectory_listBox.SelectedIndex - currentDirFolders.Count];
                    customMessageBox csm = new customMessageBox(mainWindow, "Success", "The file you selected was: " + selectedItem);
                    //if (isCoverArt)
                    //{
                    //    AddGameForm.gameCoverPath = selectedItem;
                    //}
                    //else
                    //{
                    //    AddGameForm.gamePath = selectedItem;
                    //}
                    csm.ShowDialog();
                }
                isFolder.Clear();
                this.Close();
            }
            else
            {
                customMessageBox csm = new customMessageBox(mainWindow, "Error", "The number of items in the current directory does not match the number of items in the listbox. Please contact the developer.");
                csm.Show();
            }
            fileDirectory_listBox.SelectedIndex = 0;
        }
        public void killProgram()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }
        public void loadCurrentDirItems()
        {
            currentDirFiles.Clear();
            currentDirFolders.Clear();
            fileDirectory_listBox.Items.Clear();
            try
            {
                currentDir = filePathURL_textBox.Text;
                currentDirFiles = System.IO.Directory.GetFiles(currentDir).ToList();
                currentDirFolders = System.IO.Directory.GetDirectories(currentDir).ToList();
                for (int i = 0; i < currentDirFolders.Count; i++)
                {
                    currentDirFolders[i] = currentDirFolders[i].Replace(currentDir + "\\", string.Empty);
                    CreateListBoxItem(i, true);
                }
                for (int i = 0; i < currentDirFiles.Count; i++)
                {
                    currentDirFiles[i] = currentDirFiles[i].Replace(currentDir + "\\", string.Empty);
                    CreateListBoxItem(i, false);
                }
            }
            catch (Exception)
            {
                customMessageBox csm = new customMessageBox(mainWindow, "Error", "The Directory you entered does not exist. Please check for spelling errors.");
                csm.Show();
                return;
            }
        }
        private void CreateListBoxItem(int itemNum, bool isFolder)
        {
            Viewbox viewbox = new Viewbox
            {
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Height = 150,
            };
            ListBoxItem fileDir_listBoxItem = new ListBoxItem
            {
                Margin = new Thickness(10),
                Height = 150,
                Width = 670,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Name = $"fileDir_{itemNum}_listBoxItem"
            };
            StackPanel fileDir_stackPanel = new StackPanel
            {
                Name = $"fileDir_{itemNum}_stackPanel",
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Image fileDir_image = new Image
            {
                Name = $"fileDir_{itemNum}_image",
                Height = 200,
                Width = 200,
                Margin = new Thickness(10)
            };
            if (isFolder)
            {

                fileDir_image.Source = new BitmapImage(new Uri("/Images/Folder_Icon.png", UriKind.Relative));
            }
            else
            {
                fileDir_image.Source = new BitmapImage(new Uri("/Images/File_Icon.png", UriKind.Relative));
            }
            Label fileDir_label = new Label
            {
                Name = $"fileDir_{itemNum}_label",
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 36,
                Width = 800,
                Foreground = Brushes.White
            };
            if (isFolder)
            {

                fileDir_label.Content = currentDirFolders[itemNum];
            }
            else
            {
                fileDir_label.Content = currentDirFiles[itemNum];
            }
            fileDir_stackPanel.Children.Add(fileDir_image);
            fileDir_stackPanel.Children.Add(fileDir_label);
            viewbox.Child = fileDir_stackPanel;
            fileDir_listBoxItem.Content = viewbox;
            fileDirectory_listBox.Items.Add(fileDir_listBoxItem);
        }
        private void filePathURL_textBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            loadCurrentDirItems();
        }
    }
}
