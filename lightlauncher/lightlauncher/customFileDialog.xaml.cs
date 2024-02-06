//By Sauraav Jayrajh
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        public string selectedItem = string.Empty;
        private bool previousShouldersPressed = false;
        private bool previousDPadLeftOrUp = false;
        private bool previousDPadRightOrDown = false;
        private bool previousY = false;
        private bool previousB = false;
        private bool previousX = false;
        private bool previousR2 = false;
        private bool previousL2 = false;
        private string[] driveList;
        private int driveIndex;
        public customFileDialog(MainWindow mw)
        {
            InitializeComponent();
            InitializeDriveList();
            usersController = new Controller(UserIndex.One);
            controllerThread = new Thread(pollControllerState);
            controllerThread.IsBackground = true;
            controllerThread.Start();
            fileDirectory_listBox.SelectedIndex = 0;
            mainWindow = mw;
            loadCurrentDirItems();
        }
        private void InitializeDriveList()
        {
            driveList = Directory.GetLogicalDrives();
            Array.Sort(driveList, StringComparer.OrdinalIgnoreCase);
            driveIndex = 0;
            filePathURL_textBox.Text = driveList[driveIndex];
        }
        public void pollControllerState()
        {
            while (running)
            {
                if (!usersController.IsConnected)
                {
                    Dispatcher.Invoke(() =>
                    {
                        customMessageBox csm = new customMessageBox(mainWindow, "Error", "Controller not detected! Please make sure you are using an Xbox or XInput Compatible Controller.");
                        csm.ShowDialog();
                        csm.Close();
                    });
                    killProgram();
                    return;
                }
                State state = usersController.GetState();
                bool shouldersPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder);
                if (shouldersPressed && !previousShouldersPressed)
                {
                    Dispatcher.Invoke(openKeyboard);
                }
                bool dPadLeftOrUp = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp);
                if (dPadLeftOrUp && !previousDPadLeftOrUp)
                {
                    Dispatcher.Invoke(moveCursorUp);
                }
                bool dPadRightOrDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown);
                if (dPadRightOrDown && !previousDPadRightOrDown)
                {
                    Dispatcher.Invoke(moveCursorDown);
                }
                bool yPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);
                if (yPressed && !previousY)
                {
                    Dispatcher.Invoke(openPreviousDirectory);
                }
                bool bPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                if (bPressed && !previousB)
                {
                    Dispatcher.Invoke(() => this.Close());
                }
                bool xPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X);
                if (xPressed && !previousX)
                {
                    Dispatcher.Invoke(selectOption);
                }
                bool L2Pressed = false;
                if (state.Gamepad.LeftTrigger == 255)
                {
                    L2Pressed = true;
                }
                if (L2Pressed && !previousL2)
                {
                    Dispatcher.Invoke(() => previousDrive());
                }
                bool R2Pressed = false;
                if (state.Gamepad.RightTrigger == 255)
                {
                    R2Pressed = true;
                }
                if (R2Pressed && !previousR2)
                {
                    Dispatcher.Invoke(() => nextDrive());
                }
                previousShouldersPressed = shouldersPressed;
                previousDPadLeftOrUp = dPadLeftOrUp;
                previousDPadRightOrDown = dPadRightOrDown;
                previousY = yPressed;
                previousB = bPressed;
                previousX = xPressed;
                previousL2 = L2Pressed;
                previousR2 = R2Pressed;
                Thread.Sleep(120);
            }
        }
        public void previousDrive()
        {
            if ((driveIndex > 0))
            {
                driveIndex--;
                filePathURL_textBox.Text = driveList[driveIndex];
                loadCurrentDirItems();
            }
        }
        public void nextDrive()
        {
            if (driveIndex < driveList.Count() - 1)
            {
                driveIndex++;
                filePathURL_textBox.Text = driveList[driveIndex];
                loadCurrentDirItems();
            }
        }
        public void openPreviousDirectory()
        {
            try
            {
                var parentDir = Directory.GetParent(filePathURL_textBox.Text);
                if (parentDir != null)
                {
                    previousDir = parentDir.FullName;
                }
                else
                {
                    return;
                }
                filePathURL_textBox.Text = previousDir;
                loadCurrentDirItems();
            }
            catch (Exception)
            {
                customMessageBox csm = new customMessageBox(mainWindow, "Error", "The current folder is not contained in another folder.");
                csm.ShowDialog();
                csm.Close();
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
            if (!(fileDirectory_listBox.SelectedIndex == -1))
            {
                selectedItem = string.Empty;
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
                        filePathURL_textBox.Text = Path.Combine(currentDir, currentDirFolders[fileDirectory_listBox.SelectedIndex]);
                        loadCurrentDirItems();
                        fileDirectory_listBox.SelectedIndex = 0;
                    }
                    else
                    {
                        selectedItem = Path.Combine(currentDir, currentDirFiles[fileDirectory_listBox.SelectedIndex - currentDirFolders.Count]);
                        customMessageBox csm = null;
                        //csm = new customMessageBox(mainWindow, "Success", "The file you selected was: " + selectedItem);
                        if (selectedItem.EndsWith(".jpg") || selectedItem.EndsWith(".jpeg") || selectedItem.EndsWith(".png") || selectedItem.EndsWith(".bmp"))
                        {
                            AddGameForm.gameCoverPath = selectedItem;
                        }
                        else if (selectedItem.EndsWith(".exe") || selectedItem.EndsWith(".lnk") || selectedItem.EndsWith(".iso") || selectedItem.EndsWith(".cso"))
                        {
                            AddGameForm.gamePath = selectedItem;
                        }
                        else
                        {
                            csm = new customMessageBox(mainWindow, "Error", "The file you selected was of an incompatiable type.");
                            csm.ShowDialog();
                            csm.Close();
                        }
                        this.Close();
                        this.running = false;
                    }
                    isFolder.Clear();
                }
                else
                {
                    customMessageBox csm = new customMessageBox(mainWindow, "Error", "The number of items in the current directory does not match the number of items in the listbox. Please contact the developer.");
                    csm.ShowDialog();
                    csm.Close();
                }
            }
            else
            {
                customMessageBox csm = new customMessageBox(mainWindow, "Error", "No item was selected!");
                csm.ShowDialog();
                csm.Close();
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
                csm.ShowDialog();
                csm.Close();
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
        private void openKeyboard()
        {
            controllerKeyboard ck = new controllerKeyboard(filePathURL_textBox);
            ck.ShowDialog();
            ck.Close();
        }
        private void filePathURL_textBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            loadCurrentDirItems();
        }
        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            filePathURL_textBox.Text = "D:\\Games\\";
            loadCurrentDirItems();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            running = false;
        }
    }
}
