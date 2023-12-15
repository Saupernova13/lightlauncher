using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using SharpDX.XInput;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Documents;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Data.SqlClient;

namespace lightlauncher
{
    public partial class MainWindow : Window
    {
        //List Of Games To Be Displayed
        public static List<Game> games = new List<Game>();
        //Name of currently highlighted game
        public static string currentGameName = "";
        //Path to folder containing game art
        public static string folderPath = "";
        //Object Of Controller
        private Controller usersController;
        //Instance Of Thread
        private Thread controllerThread;
        //Boolean To Check If Thread Is Running
        //Note: Volatile keyword ensures that field may be modified by multiple threads at the same time
        private volatile bool running = true;
        public MainWindow()
        {
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
                System.Windows.Forms.MessageBox.Show($"An error occurred: {ex.Message}");
            }
            InitializeComponent();
            loadGamesFromDB();
            gameTitleTextBlock.Text = games[0].name;
            foreach (Game game in games)
            {
                populateGameList(game);
            }
            this.Hide();
            //Map the controller index to Port 1
            usersController = new Controller(UserIndex.One);

            if (!usersController.IsConnected)
            {
                System.Windows.MessageBox.Show("No controller is not detected!\nPlease make sure you are using an Xbox or XInput Compatible Controller.");
                System.Windows.Application.Current.Shutdown();
                return;
            }
            // Start the thread for polling controller state
            controllerThread = new Thread(pollControllerState);
            //Make sure the program will still read input when the window is not in focus
            controllerThread.IsBackground = true;
            controllerThread.Start();
        }

        private void pollControllerState()
        {
            while (running)
            {
                //Get state of controller 
                State state = usersController.GetState();
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    //If button is pressed, call the method, but do it safely via the dispatcher. The reason we
                    //do this is because the thread is not the same as the UI thread, and we cannot modify UI elements
                    //from a non-UI thread
                    Dispatcher.Invoke(this.Show);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft))
                {
                    Dispatcher.Invoke(moveCursorLeft);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
                {
                    Dispatcher.Invoke(moveCursorRight);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y))
                {
                    Dispatcher.Invoke(showAddGameWindow);
                }
                int id = 0;
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
                {
                    for (int i = 0; i < games.Count; i++)
                    {
                        if (games[i].name.Equals(currentGameName))
                        {
                            id = games[i].ID;
                        }
                    }
                    launchGame(id);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
                {
                    killProgram();
                }
                //This means that there will be a 150ms gap until another button press is registered
                Thread.Sleep(125);
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            running = false; // Indicate that the thread should no longer run.
            controllerThread.Join(); // Wait for the thread to finish executing to avoid any potential issues.
            base.OnClosed(e);
        }

        private void addGameIcon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            showAddGameWindow();
        }
        //Window used to add new games to program
        public static void showAddGameWindow()
        {
            AddGameForm addGameForm = new AddGameForm();
            addGameForm.Show();
        }
        private void moveCursorLeft()
        {
            if (gameListBox.SelectedIndex > 0)
            {
                gameListBox.SelectedIndex = gameListBox.SelectedIndex - 1;
                gameListBox.ScrollIntoView(gameListBox.SelectedItem);
            }
        }
        private void moveCursorRight()
        {
            if (gameListBox.SelectedIndex < gameListBox.Items.Count - 1)
            {
                gameListBox.SelectedIndex = gameListBox.SelectedIndex + 1;
                gameListBox.ScrollIntoView(gameListBox.SelectedItem);
            }
        }
        //Populates the UI  Layout
        private void populateGameList(Game game)
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
            gameListBox.Items.Add(viewbox);
        }
        //Intakes gameID and launches game with said ID
        private void launchGame(int gameID)
        {
            int index = 0;
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].ID == gameID)
                {
                    index = i;
                    //dies if user deny admin rights, must fix
                    Process.Start(games[index].executablePath);
                }
                if (games.Count == i)
                {
                    System.Windows.Forms.MessageBox.Show("No Game File Was Found");
                }
            }
        }
        //Loads games from database into memory for program to use
        public static void loadGamesFromDB()
        {
            games.Clear();
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
        }
        private void AddGameIcon_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e, string executableDirectory)
        {
        }
        private void gameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gameTitleTextBlock.Text = games[gameListBox.SelectedIndex].name;
            currentGameName = gameTitleTextBlock.Text;
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
        public static void killProgram()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}