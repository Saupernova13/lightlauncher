//By Sauraav Jayrajh
using SharpDX.XInput;
using System.Data.SqlClient;
using System.Threading;
using System.Windows;
using System.IO;
using System;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using System.Security.Cryptography;

namespace lightlauncher
{
    public partial class AddGameForm : Window
    {
        public Game newGame = new Game();
        public MainWindow mainWindow;
        private int gameCount;
        private bool needsEmulator = false;
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        public bool[] isCompleted = new bool[3];
        public controllerKeyboard onscreenKeyboard;
        public customMessageBox csm;
        public static string gamePath;
        public static string gameCoverPath;
        private bool previousDPadLeftOrUp = false;
        private bool previousDPadRightOrDown = false;
        private bool previousA = false;
        private bool previousB = false;
        private bool dbEmpty = true;
        public AddGameForm(MainWindow mw)
        {
            SqlConnection sqlConnection = null;
            try
            {
                sqlConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=lightlauncher.DBContext;Integrated Security=True");
                sqlConnection.Open();
            }
            catch (Exception)
            {
                try
                {
                    sqlConnection = new SqlConnection("Data Source=.\\SQLEXPRESS;Initial Catalog=lightlauncher.DBContext;Integrated Security=True");
                    sqlConnection.Open();
                }
                catch (Exception)
                {
                    customMessageBox csm1 = new customMessageBox("Error", "An error occurred: Could not connect to database");
                    csm1.ShowDialog();
                    csm1.Close();
                }
                customMessageBox csm = new customMessageBox("Error", "An error occurred: After a secondary attempt, the program could not connect to the database");
                csm.ShowDialog();
                csm.Close();
                killProgram();
            }
            SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) FROM Games", sqlConnection);
            gameCount = (int)sqlCommand.ExecuteScalar();
            if (gameCount == 0)
            {
                dbEmpty = true;
                newGame.ID = 1;
            }
            else
            {
                dbEmpty = false;
                SqlCommand sqlCommandReadGameID = new SqlCommand("SELECT * FROM Games", sqlConnection);
                SqlDataReader sqlDataReader = sqlCommandReadGameID.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    newGame.ID=Convert.ToInt32(sqlDataReader["ID"]);
                }
                newGame.ID++;
            }
            sqlConnection.Close();
            InitializeComponent();
            checkboxImage.Visibility = Visibility.Hidden;
            optionsListBox.SelectedIndex = 0;
            mainWindow = mw;
            usersController = new Controller(UserIndex.One);
            controllerThread = new Thread(pollControllerState);
            controllerThread.IsBackground = true;
            controllerThread.Start();
        }
        private void pollControllerState()
        {
            while (running)
            {
                if (Dispatcher.Invoke(() => this.IsActive))
                {
                    State state = usersController.GetState();
                    // Debounced button checks
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
                    bool aPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
                    if (aPressed && !previousA)
                    {
                        int selectedIndex = Dispatcher.Invoke(() => optionsListBox.SelectedIndex);
                        Dispatcher.Invoke(() =>
                        {
                            switch (selectedIndex)
                            {
                                case 0:
                                    onscreenKeyboard = new controllerKeyboard(gameNameTextBox);
                                    onscreenKeyboard.ShowDialog();
                                    onscreenKeyboard.Close();
                                    break;
                                case 1:
                                    getGamePath();
                                    break;
                                case 2:
                                    getGameCover();
                                    break;
                                case 3:
                                    tagGameAsEmulated();
                                    break;
                                case 4:
                                    addGameToDB();
                                    break;
                                default:
                                    break;
                            }
                        });
                    }

                    bool bPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                    if (bPressed && !previousB)
                    {
                        Dispatcher.Invoke(this.Close);
                        Dispatcher.Invoke(() => mainWindow.ShowDialog());
                    }

                    // Remember button states for the next poll
                    previousDPadLeftOrUp = dPadLeftOrUp;
                    previousDPadRightOrDown = dPadRightOrDown;
                    previousA = aPressed;
                    previousB = bPressed;

                    // Sleep to avoid high CPU load
                    Thread.Sleep(120);
                }
            }
        }

        private void tagGameAsEmulated()
        {
            switch (checkboxImage.Visibility)
            {
                case Visibility.Visible:
                    checkboxImage.Visibility = Visibility.Hidden;
                    needsEmulator = false;
                    break;
                case Visibility.Hidden:
                    checkboxImage.Visibility = Visibility.Visible;
                    needsEmulator = true;
                    break;
            }
        }

        public void moveCursorUp()
        {
            if (optionsListBox.SelectedIndex > 0)
            {
                optionsListBox.SelectedIndex = optionsListBox.SelectedIndex - 1;
            }
        }
        public void moveCursorDown()
        {
            if (optionsListBox.SelectedIndex < optionsListBox.Items.Count - 1)
            {
                optionsListBox.SelectedIndex = optionsListBox.SelectedIndex + 1;
            }
        }
        private void gameAddButton_Click(object sender, RoutedEventArgs e)
        {
            addGameToDB();
        }
        public void addGameToDB()
        {
            if ((gameNameTextBox.Text.Equals(String.Empty) || (gamePath == null) || (gameCoverPath == null))) {
                csm = new customMessageBox("Error adding game", $"You have not completed all fields! Please make sure all fields have been populated.");
                csm.ShowDialog();
            }
            else
            {
                try
                {
                    mainWindow.gameListBox.SelectedIndex = 0;
                    newGame.name = gameNameTextBox.Text;
                    newGame.needsEmulator = needsEmulator;
                    isCompleted[0] = true;
                    string coverArtFileName = newGame.ID + Path.GetExtension(newGame.imagePath);
                    File.Copy(newGame.imagePath, Path.Combine(MainWindow.folderPath, newGame.ID + Path.GetExtension(newGame.imagePath)), true);
                    newGame.imagePath = coverArtFileName;
                    SqlConnection sqlConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=lightlauncher.DBContext;Integrated Security=True");
                    sqlConnection.Open();
                    if (dbEmpty)
                    {
                        SqlCommand identityInsertCommand = new SqlCommand("SET IDENTITY_INSERT Games ON", sqlConnection);
                        identityInsertCommand.ExecuteNonQuery();
                        SqlCommand sqlCommand = new SqlCommand("INSERT INTO Games (ID, name, executablePath, imagePath, needsEmulator) VALUES (@ID, @name, @executablePath, @imagePath, @needsEmulator)", sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@ID", newGame.ID);
                        sqlCommand.Parameters.AddWithValue("@name", newGame.name);
                        sqlCommand.Parameters.AddWithValue("@executablePath", newGame.executablePath);
                        sqlCommand.Parameters.AddWithValue("@imagePath", newGame.imagePath);
                        sqlCommand.Parameters.AddWithValue("@needsEmulator", newGame.needsEmulator);
                        sqlCommand.ExecuteNonQuery();
                        identityInsertCommand = new SqlCommand("SET IDENTITY_INSERT Games OFF", sqlConnection);
                        identityInsertCommand.ExecuteNonQuery();
                    }
                    else {
                        SqlCommand sqlCommand = new SqlCommand("INSERT INTO Games (name, executablePath, imagePath, needsEmulator) VALUES (@name, @executablePath, @imagePath, @needsEmulator)", sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@name", newGame.name);
                        sqlCommand.Parameters.AddWithValue("@executablePath", newGame.executablePath);
                        sqlCommand.Parameters.AddWithValue("@imagePath", newGame.imagePath);
                        sqlCommand.Parameters.AddWithValue("@needsEmulator", newGame.needsEmulator);
                        sqlCommand.ExecuteNonQuery();
                    }
                    sqlConnection.Close();
                    mainWindow.gameListBox.SelectedIndex = 0;
                    mainWindow.loadGamesFromDB();
                    this.Close();
                    csm = new customMessageBox("Game Successfully Added!", $"{newGame.name} has been added to your library!");
                    mainWindow.Show();
                    csm.ShowDialog();
                }
                catch (Exception)
                {
                    csm = new customMessageBox("Failed to add game", $"There was an error trying to add your game to the system. Please make sure no files are open in other processes, or are restricted by any admins, and try again.");
                    mainWindow.Show();
                    csm.ShowDialog();
                }
            }
        }
        private void gameExecutablePickButton_Click(object sender, RoutedEventArgs e)
        {
            getGamePath();
        }
        public void getGamePath()
        {
            gamePath = string.Empty;
            customFileDialog cfd = new customFileDialog(mainWindow, "getGameDetails");
            cfd.ShowDialog();
            if (gamePath == null)
            {
                csm = new customMessageBox("Error!", "No executable path was selected!");
                csm.ShowDialog();
                gameLocationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFF00"));
                gameLocationLabel.Content = $"No file was selected!";
                return;
            }
            else
            {
                newGame.executablePath = gamePath;
                try
                {
                    if (newGame.executablePath.Equals(String.Empty))
                    {
                        gameLocationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFF00"));
                        gameLocationLabel.Content = $"No file was selected!";
                    }
                    else
                    {
                        gameLocationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                        gameLocationLabel.Content = $"The path '{newGame.executablePath}' has been found!";
                    }
                }
                catch (Exception)
                {
                    csm = new customMessageBox("Error!", "An error occured while trying to get the game's executable path!");
                    csm.ShowDialog();
                }
                isCompleted[1] = true;
            }
            cfd.Close();
        }
        private void gameCoverPickButton_Click(object sender, RoutedEventArgs e)
        {
            getGameCover();
        }
        public void getGameCover()
        {
            customFileDialog cfd = new customFileDialog(mainWindow, "getGameDetails");
            cfd.ShowDialog();
            if (gameCoverPath == null)
            {
                csm = new customMessageBox("Error!", "No image was selected!");
                csm.ShowDialog();
                gameCoverLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFF00"));
                gameCoverLabel.Content = $"No image was selected!";
                return;
            }
            else
            {
                newGame.imagePath = gameCoverPath;
                try
                {
                    if (newGame.imagePath.Equals(String.Empty))
                    {
                        gameCoverLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFF00"));
                        gameCoverLabel.Content = $"No image was selected!";
                    }
                    else
                    {
                        gameCoverLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                        gameCoverLabel.Content = $"An image has been found!";
                    }
                }
                catch (Exception)
                {
                    csm = new customMessageBox("Error!", "An error occured while trying to get the selected image!");
                    csm.ShowDialog();
                }
                isCompleted[2] = true;
            }
            cfd.Close();
        }
        private void optionsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            running = false;
        }
        public void killProgram()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }
    }
}
