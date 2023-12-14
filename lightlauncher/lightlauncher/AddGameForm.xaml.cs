using SharpDX.XInput;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System;

namespace lightlauncher
{
    public partial class AddGameForm : Window
    {
        public Game newGame = new Game();
        int gameCount;
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;

        public AddGameForm()
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
                } Thread.Sleep(150);
            }
        }

        private void gameAddButton_Click(object sender, RoutedEventArgs e)
        {
            newGame.name = gameNameTextBox.Text;
            string coverArtFileName = newGame.ID+"";
            File.Copy(newGame.imagePath, System.IO.Path.Combine(MainWindow.folderPath, newGame.ID + System.IO.Path.GetExtension(newGame.imagePath)), true);
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

            MainWindow.games.Add(newGame);
            this.Close();
        }

        private void gameExecutablePickButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Title = "Select a Game Executable File";
            dialog.Filter = "Executable Files (*.exe)|*.exe";
            dialog.FilterIndex = 1;
            dialog.Multiselect = false;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            DialogResult result = dialog.ShowDialog();
            if (!result.HasFlag(System.Windows.Forms.DialogResult.OK))
            {
                System.Windows.MessageBox.Show("No new game was added");
            }
            else
            {
                newGame.executablePath = dialog.FileName;
            }
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
                System.Windows.MessageBox.Show("No cover art was added");
            }
            else
            {
                newGame.imagePath = dialog.FileName;
            }

        }
    }
}
