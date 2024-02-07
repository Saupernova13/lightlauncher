//By Sauraav Jayrajh
using SharpDX.XInput;
using System.Data.SqlClient;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

namespace lightlauncher
{
    public partial class AddEmulatorForm : Window
    {
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        private bool previousDPadLeftOrUp = false;
        private bool previousDPadRightOrDown = false;
        private bool previousA = false;
        private bool previousB = false;
        public string emulatorPath;
        public Emulator newEmulator = new Emulator();
        public controllerKeyboard onscreenKeyboard;
        public customMessageBox csm;
        public MainWindow mainWindow;
        public emulatorMenu EmulatorMenu;
        private int emulatorCount;
        public AddEmulatorForm(MainWindow mw, emulatorMenu em)
        {

            this.Topmost = true;
            SqlConnection sqlConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=lightlauncher.DBContext;Integrated Security=True");
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(*) FROM Emulators", sqlConnection);
            emulatorCount = (int)sqlCommand.ExecuteScalar();
            sqlConnection.Close();
            if (emulatorCount == 0)
            {
                newEmulator.ID = 1;
            }
            else
            {
                newEmulator.ID = emulatorCount + 1;
            }
            InitializeComponent();
            optionsListBox.SelectedIndex = 0;
            mainWindow = mw;
            EmulatorMenu = em;
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
                                    onscreenKeyboard = new controllerKeyboard(emulatorNameTextBox);
                                    onscreenKeyboard.ShowDialog();
                                    onscreenKeyboard.Close();
                                    break;
                                case 1:
                                    getEmulatorPath();
                                    break;
                                case 2:
                                    addEmulatorToDB();
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
                        Dispatcher.Invoke(() => EmulatorMenu.ShowDialog());
                    }
                    previousDPadLeftOrUp = dPadLeftOrUp;
                    previousDPadRightOrDown = dPadRightOrDown;
                    previousA = aPressed;
                    previousB = bPressed;
                    Thread.Sleep(120);
                }
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
        public void addEmulatorToDB()
        {
            if ((emulatorNameTextBox.Text.Equals(String.Empty) || (emulatorPath == null) || (emulatorPath.Equals(String.Empty))))
            {
                csm = new customMessageBox("Error adding emulator", $"You have not completed all fields! Please make sure all fields have been populated.");
                csm.ShowDialog();
            }
            else
            {
                try
                {
                    EmulatorMenu.optionsListBox.SelectedIndex = 0;
                    newEmulator.name = emulatorNameTextBox.Text;
                    SqlConnection sqlConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=lightlauncher.DBContext;Integrated Security=True");
                    sqlConnection.Open();
                    SqlCommand identityInsertCommand = new SqlCommand("SET IDENTITY_INSERT Emulators ON", sqlConnection);
                    identityInsertCommand.ExecuteNonQuery();
                    SqlCommand sqlCommand = new SqlCommand("INSERT INTO Emulators (ID, name, executablePath) VALUES (@ID, @name, @executablePath)", sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@ID", newEmulator.ID);
                    sqlCommand.Parameters.AddWithValue("@name", newEmulator.name);
                    sqlCommand.Parameters.AddWithValue("@executablePath", newEmulator.executablePath);
                    sqlCommand.ExecuteNonQuery();
                    identityInsertCommand = new SqlCommand("SET IDENTITY_INSERT Emulators OFF", sqlConnection);
                    identityInsertCommand.ExecuteNonQuery();
                    sqlConnection.Close();
                    EmulatorMenu.optionsListBox.SelectedIndex = 0;
                    //EmulatorMenu.loadGamesFromDB();
                    this.Close();
                    csm = new customMessageBox("Emulator Successfully Added!", $"{newEmulator.name} has been added to the system!");
                    mainWindow.Show();
                    csm.ShowDialog();
                }
                catch (Exception)
                {
                    csm = new customMessageBox("Failed to add emulator", $"There was an error trying to add your emulator to the system. Please make sure no files are open in other processes, or are restricted by any admins, and try again.");
                    mainWindow.Show();
                    csm.ShowDialog();
                }
            }
        }
        public void getEmulatorPath()
        {
            emulatorPath = string.Empty;
            customFileDialog cfd = new customFileDialog(mainWindow, "getEmulatorDetails", this);
            cfd.ShowDialog();
            if (emulatorPath == null)
            {
                csm = new customMessageBox("Error!", "No executable path was selected!");
                csm.ShowDialog();
                emulatorLocationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFF00"));
                emulatorLocationLabel.Content = $"No file was selected!";
                return;
            }
            else
            {
                newEmulator.executablePath = emulatorPath;
                try
                {
                    if (newEmulator.executablePath.Equals(String.Empty))
                    {
                        emulatorLocationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAFF00"));
                        emulatorLocationLabel.Content = $"No file was selected!";
                    }
                    else
                    {
                        emulatorLocationLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                        emulatorLocationLabel.Content = $"The path '{newEmulator.executablePath}' has been found!";
                    }
                }
                catch (Exception)
                {
                    csm = new customMessageBox("Error!", "An error occured while trying to get the game's executable path!");
                    csm.ShowDialog();
                }
            }
            cfd.Close();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            running = false;
        }
    }
}
