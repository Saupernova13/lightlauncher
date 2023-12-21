using SharpDX.XInput;
using System.Data.SqlClient;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System;
using System.Windows.Media;

namespace lightlauncher
{
    public partial class AddGameForm : Window
    {
        public Game newGame = new Game();
        public MainWindow mainWindow;
        int gameCount;
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        public bool[] isCompleted = new bool[3];
        customMessageBox csm;
        public AddGameForm(MainWindow mw)
        {
            SqlConnection sqlConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=lightlauncher.DBContext;Integrated Security=True");
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) FROM Games", sqlConnection);
            gameCount = (int)sqlCommand.ExecuteScalar();
            sqlConnection.Close();
            if (gameCount == 0)
            {
                newGame.ID = 1;
            }
            else
            {
                newGame.ID = gameCount + 1;
            }
            InitializeComponent();
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
                State state = usersController.GetState();
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
                {
                    Dispatcher.Invoke(moveCursorUp);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
                {
                    Dispatcher.Invoke(moveCursorDown);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
                {
                    switch (Dispatcher.Invoke(() => optionsListBox.SelectedIndex))
                    {
                        case 0:
                            break;
                        case 1:
                            Dispatcher.Invoke(getGamePath);
                            break;
                        case 2:
                            Dispatcher.Invoke(getGameCover);
                            break;
                        case 3:
                            Dispatcher.Invoke(addGameToDB);
                            break;
                        default:
                            break;
                    }
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
                {
                    Dispatcher.Invoke(this.Close);
                    Dispatcher.Invoke(() => mainWindow.Show());
                }
                Thread.Sleep(150);
            }
        }
        public void moveCursorUp()
        {
            optionsListBox.SelectedIndex = optionsListBox.SelectedIndex - 1;
        }

        public void moveCursorDown()
        {
            optionsListBox.SelectedIndex = optionsListBox.SelectedIndex + 1;
        }
        private void gameAddButton_Click(object sender, RoutedEventArgs e)
        {
            addGameToDB();
        }
        public void addGameToDB()
        {

            mainWindow.gameListBox.SelectedIndex = 0;
            newGame.name = gameNameTextBox.Text;
            isCompleted[0] = true;
            string coverArtFileName = newGame.ID + Path.GetExtension(newGame.imagePath);
            File.Copy(newGame.imagePath, Path.Combine(MainWindow.folderPath, newGame.ID + Path.GetExtension(newGame.imagePath)), true);
            newGame.imagePath = coverArtFileName;
            SqlConnection sqlConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=lightlauncher.DBContext;Integrated Security=True");
            sqlConnection.Open();
            SqlCommand identityInsertCommand = new SqlCommand("SET IDENTITY_INSERT Games ON", sqlConnection);
            identityInsertCommand.ExecuteNonQuery();
            SqlCommand sqlCommand = new SqlCommand("INSERT INTO Games (ID, name, executablePath, imagePath) VALUES (@ID, @Name, @ExecutablePath, @ImagePath)", sqlConnection);
            sqlCommand.Parameters.AddWithValue("@ID", newGame.ID);
            sqlCommand.Parameters.AddWithValue("@Name", newGame.name);
            sqlCommand.Parameters.AddWithValue("@ExecutablePath", newGame.executablePath);
            sqlCommand.Parameters.AddWithValue("@ImagePath", newGame.imagePath);
            sqlCommand.ExecuteNonQuery();
            identityInsertCommand = new SqlCommand("SET IDENTITY_INSERT Games OFF", sqlConnection);
            identityInsertCommand.ExecuteNonQuery();
            sqlConnection.Close();
            mainWindow.gameListBox.SelectedIndex = 0;
            mainWindow.loadGamesFromDB();
            this.Close();
            csm = new customMessageBox(mainWindow, "Game Successfully Added!", $"{newGame.name} has been added to your library!");
            csm.ShowDialog();
        }
        private void gameExecutablePickButton_Click(object sender, RoutedEventArgs e)
        {
            getGamePath();
        }
        public void getGamePath()
        {
            OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "Select a Game Executable File";
            dialog.Filter = "Executable Files (*.exe)|*.exe";
            dialog.FilterIndex = 1;
            dialog.Multiselect = false;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            DialogResult result = dialog.ShowDialog();
            if (!result.HasFlag(System.Windows.Forms.DialogResult.OK))
            {
                gameLocationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFF00"));
                gameLocationLabel.Content = $"No file was selected!";
            }
            else
            {
                newGame.executablePath = dialog.FileName;
                gameLocationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                gameLocationLabel.Content = $"The path '{newGame.executablePath}' has been found!";
            }
            isCompleted[1] = true;
        }
        private void gameCoverPickButton_Click(object sender, RoutedEventArgs e)
        {
            getGameCover();
        }
        public void getGameCover()
        {
            OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "Select a Game Cover Art File";
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|JPEG Files (*.jpg, *.jpeg)|*.jpg;*.jpeg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            dialog.FilterIndex = 1;
            dialog.Multiselect = false;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            DialogResult result = dialog.ShowDialog();
            if (!result.HasFlag(System.Windows.Forms.DialogResult.OK))
            {
                gameCoverLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFF00"));
                gameCoverLabel.Content = $"No image was added!";
            }
            else
            {
                newGame.imagePath = dialog.FileName;
                gameCoverLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                gameCoverLabel.Content = $"The cover image has been found!";
            }
            isCompleted[2] = true;
        }

        private void optionsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

    }
}
