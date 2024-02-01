﻿//By Sauraav Jayrajh
using System;
using System.Threading;
using System.Windows;
using SharpDX.XInput;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Reflection;
namespace lightlauncher
{
    public partial class MainWindow : Window
    {
        public readonly string programName = "light_launcher";
        //List Of Games To Be Displayed
        public static List<Game> games = new List<Game>();
        //Name of currently highlighted game
        public static string currentGameName = "";
        //Name of currently running game or process
        public static string currentRunningProcess = "";
        //Path to folder containing game art
        public static string folderPath = "";
        //Object Of Controller
        private Controller usersController;
        //Instance Of Thread
        private Thread controllerThread;
        public customMessageBox csm;
        //Boolean To Check If Thread Is Running
        //Note: Volatile keyword ensures that field may be modified by multiple threads at the same time
        private string currentDir = string.Empty;
        private volatile bool running = true;
        private bool previousLeftShoulder = false;
        private bool previousRightShoulder = false;
        private bool previousLeftThumb = false;
        private bool previousRightThumb = false;
        private bool previousB = false;
        private bool previousA = false;
        private bool previousDPadLeft = false;
        private bool previousDPadRight = false;
        private bool previousY = false;
        private bool previousComboKill = false;
        private bool previousL2 = false;
        public MainWindow()
        {
            try
            {
                string startupFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), $"{programName}.lnk");
                string executablePath = Assembly.GetExecutingAssembly().Location;
                CreateShortcut(startupFolderPath, executablePath);
            }
            catch (Exception ex)
            {
                csm = new customMessageBox(this, "Error", "An error occurred creating a shortcut in the startup folder: " + ex.Message);
                csm.ShowDialog();
            }
            try
            {
                using (var context = new DBContext())
                {
                    context.Database.CreateIfNotExists();
                }
            }
            catch (Exception ex)
            {
                csm = new customMessageBox(this, "Error", "An error occurred creating a database on your system. You may not have\nSQL Server installed: " + ex.Message);
                csm.ShowDialog();
            }
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string newFolderName = "GameCovers";
            folderPath = Path.Combine(programDirectory, newFolderName);
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
            catch (Exception ex)
            {
                csm = new customMessageBox(this, "Error", "An error occurred creating your game cover folder: " + ex.Message);
                csm.ShowDialog();
            }
            InitializeComponent();
            loadGamesFromDB();
            this.Hide();
            //Map the controller index to Port 1
            usersController = new Controller(UserIndex.One);
            if (!usersController.IsConnected)
            {
                MessageBox.Show("No controller was detected!\nPlease make sure you are using an Xbox or XInput Compatible Controller.", "Light Launcher - Error");
                killProgram();
                return;
            }
            controllerThread = new Thread(pollControllerState);
            controllerThread.IsBackground = true;
            controllerThread.Start();
        }
        private static void CreateShortcut(string shortcutPath, string targetPath)
        {
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.Description = "StartUp shortcut for LightLauncher By Saupernova_13";
            shortcut.Save();
        }
        public void pollControllerState()
        {
            while (running)
            {
                if (!usersController.IsConnected)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var csm = new customMessageBox(this, "Error", "No controller is not detected!\nPlease make sure you are using an Xbox or XInput Compatible Controller.");
                        csm.ShowDialog();
                    });
                    killProgram();
                    return;
                }
                var state = usersController.GetState();
                bool leftShoulder = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder);
                bool rightShoulder = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder);
                bool leftThumb = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb);
                bool rightThumb = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb);
                bool bPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                bool aPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
                bool dPadLeft = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft);
                bool dPadRight = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight);
                bool yPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);
                bool comboKill = rightShoulder && bPressed;
                if ((leftShoulder && !previousLeftShoulder) && (rightShoulder && !previousRightShoulder) && (leftThumb && !previousLeftThumb) && (rightThumb && !previousRightThumb))
                {
                    Dispatcher.Invoke(() => this.Show());
                }
                if (bPressed && !previousB)
                {
                    Dispatcher.Invoke(() => this.Hide());
                }

                if (this.IsVisible)
                {
                    bool L2Pressed = false;
                    if (state.Gamepad.LeftTrigger == 255)
                    {
                        L2Pressed = true;
                    }
                    if (L2Pressed && !previousL2)
                    {
                        Dispatcher.Invoke(() => removeGame());
                    }
                    if (dPadLeft && !previousDPadLeft)
                    {
                        Dispatcher.Invoke(moveCursorLeft);
                    }
                    if (dPadRight && !previousDPadRight)
                    {
                        Dispatcher.Invoke(moveCursorRight);
                    }
                    if (yPressed && !previousY)
                    {
                        Dispatcher.Invoke(() => this.Hide());
                        Dispatcher.Invoke(showAddGameWindow);
                    }
                    if (aPressed && !previousA)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            int id = 0;
                            for (int i = 0; i < games.Count; i++)
                            {
                                if (games[i].name.Equals(currentGameName))
                                {
                                    id = games[i].ID;
                                }
                            }
                            launchGame(id);
                        });
                    }

                    previousL2 = L2Pressed;
                }
                if (comboKill && !previousComboKill)
                {
                    killProgram();
                }
                previousLeftShoulder = leftShoulder;
                previousRightShoulder = rightShoulder;
                previousLeftThumb = leftThumb;
                previousRightThumb = rightThumb;
                previousB = bPressed;
                previousA = aPressed;
                previousDPadLeft = dPadLeft;
                previousDPadRight = dPadRight;
                previousY = yPressed;
                previousComboKill = comboKill;
                Thread.Sleep(125);
            }
        }
        public void removeGame()
        {
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].name.Equals(currentGameName))
                {
                    currentDir = games[i].executablePath;
                    File.Delete(games[i].imagePath);
                }
            }
            using (SqlConnection sqlConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=lightlauncher.DBContext;Integrated Security=True"))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand("DELETE FROM Games WHERE executablePath=@executablePath", sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@executablePath", currentDir);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            loadGamesFromDB();
        }
        public void addGameIcon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            showAddGameWindow();
        }
        //Window used to add new games to program
        public void showAddGameWindow()
        {
            AddGameForm addGameForm = new AddGameForm(this);
            addGameForm.ShowDialog();
        }
        public void moveCursorLeft()
        {
            if (gameListBox.SelectedIndex > 0)
            {
                gameListBox.SelectedIndex = gameListBox.SelectedIndex - 1;
                gameListBox.ScrollIntoView(gameListBox.SelectedItem);
            }
        }
        public void moveCursorRight()
        {
            if (gameListBox.SelectedIndex < gameListBox.Items.Count - 1)
            {
                gameListBox.SelectedIndex = gameListBox.SelectedIndex + 1;
                gameListBox.ScrollIntoView(gameListBox.SelectedItem);
            }
        }
        //Populates the UI  Layout
        public void populateGameList(MainWindow mainWindow, Game game)
        {
            Viewbox viewbox = new Viewbox()
            {
                Name = $"viewBox_Game{game.ID}",
                Margin = new Thickness(20, 20, 20, 50)
            };
            Grid grid = new Grid()
            {
                Name = $"grid_Game{game.ID}"
            };
            Image image = new Image()
            {
                Name = $"image_Game{game.ID}",
                Height = 900,
                Width = 600
            };
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameCovers", game.imagePath);
            image.Source = new BitmapImage(new Uri(imagePath));
            image.PreviewMouseLeftButtonDown += (sender, e) => launchGame(game.ID);
            grid.Children.Add(image);
            viewbox.Child = grid;
            mainWindow.gameListBox.Items.Add(viewbox);
        }
        //Intakes gameID and launches game with said ID
        public void launchGame(int gameID)
        {
            bool gameFound = false;
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].ID == gameID)
                {
                    try
                    {
                        gameFound = true;
                        Process.Start(games[i].executablePath);
                        this.Hide();
                        break;
                    }
                    catch (Win32Exception ex)
                    {
                        csm = new customMessageBox(this, "Error", "An error occurred: " + ex.Message);
                        csm.ShowDialog();
                        break;
                    }
                    catch (Exception ex)
                    {
                        csm = new customMessageBox(this, "Error", "An error occurred: " + ex.Message);
                        csm.ShowDialog();
                        break;
                    }
                }
            }
            if (!gameFound)
            {
                csm = new customMessageBox(this, "Error", "An error occurred: Game not found");
                csm.ShowDialog();
            }
        }
        //Loads games from database into memory for program to use
        public void loadGamesFromDB()
        {
            games.Clear();
            gameListBox.Items.Clear();
            Game currentGame = null;
            SqlConnection sqlConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=lightlauncher.DBContext;Integrated Security=True");
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Games", sqlConnection);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                currentGame = new Game();
                currentGame.ID = Convert.ToInt32(sqlDataReader["ID"]);
                currentGame.name = sqlDataReader["name"].ToString();
                currentGame.imagePath = sqlDataReader["imagePath"].ToString();
                currentGame.executablePath = sqlDataReader["executablePath"].ToString();
                games.Add(currentGame);
            }
            if (games.Count.Equals(0))
            {
                gameTitleTextBlock.Text = "No games In library!";
            }
            else
            {

                gameTitleTextBlock.Text = games[0].name;
            }
            foreach (Game game in games)
            {
                populateGameList(this, game);
            }
        }
        private void gameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gameListBox.SelectedIndex >= 0 && gameListBox.SelectedIndex < games.Count)
            {
                gameTitleTextBlock.Text = games[gameListBox.SelectedIndex].name;
                currentGameName = gameTitleTextBlock.Text;
            }
            else
            {
                gameTitleTextBlock.Text = "";
                currentGameName = "";
            }
        }
        private void rightNav_Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            moveCursorRight();
        }
        private void leftNav_Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            moveCursorLeft();
        }
        private void addGameImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            showAddGameWindow();
        }
        public void killProgram()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }
        public string getFileNameFromPath(string path)
        {
            return Path.GetFileName(path);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            running = false;
        }
    }
}