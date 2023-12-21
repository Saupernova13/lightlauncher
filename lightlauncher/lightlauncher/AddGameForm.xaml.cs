using SharpDX.XInput;
using System.Data.SqlClient;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System;

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
        public  bool[] isCompleted = new bool[3];
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
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
                {
                    Dispatcher.Invoke(this.Close);
                    Dispatcher.Invoke(() => mainWindow.Show());
                } Thread.Sleep(150);
            }
        }
        private void gameAddButton_Click(object sender, RoutedEventArgs e)
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
            csm = new customMessageBox(mainWindow, "Game Successfully Added!", $"{newGame.name} has been added to your library!");
            csm.ShowDialog();
            this.Close();
        }

        private void gameExecutablePickButton_Click(object sender, RoutedEventArgs e)
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
                csm = new customMessageBox(mainWindow, "Executable Not Found", $"No File Was Selected!");
                csm.ShowDialog();
            }
            else
            {
                newGame.executablePath = dialog.FileName;
            }
            csm = new customMessageBox(mainWindow,"Executable Found", $"The path '{newGame.executablePath}' has been found!");
            isCompleted[1] = true;
            csm.ShowDialog();
        }

        private void gameCoverPickButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "Select a Game Cover Art File";
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|JPEG Files (*.jpg, *.jpeg)|*.jpg;*.jpeg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*";
            dialog.FilterIndex = 1;
            dialog.Multiselect = false;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            DialogResult result = dialog.ShowDialog();
            if (!result.HasFlag(System.Windows.Forms.DialogResult.OK))
            {
                csm = new customMessageBox(mainWindow, "File Not Found", $"No Image Was Added!");
                csm.ShowDialog();
            }
            else
            {
                newGame.imagePath = dialog.FileName;
            }
            csm = new customMessageBox(mainWindow, "Image Found", $"Your Game Cover Image Is Accepted!");
            isCompleted[2] = true;
            csm.ShowDialog();
        }

        private void optionsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
