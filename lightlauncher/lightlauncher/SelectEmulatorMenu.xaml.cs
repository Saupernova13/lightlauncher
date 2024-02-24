//By Sauraav Jayrajh
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace lightlauncher
{
    public partial class SelectEmulatorMenu : Window
    {
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        private bool previousDPadLeftOrUp = false;
        private bool previousDPadRightOrDown = false;
        private bool previousA = false;
        private bool previousB = false;
        private int GameID;
        public MainWindow mainWindow;
        public List<Emulator> emulators = new List<Emulator>();
        public List<Label> listLabels = new List<Label>();
        public string currentEmulatorName = "";
        public SelectEmulatorMenu(MainWindow mw, int gameID)
        {
            InitializeComponent();
            loadEmulatorsFromDB();
            GameID = gameID; usersController = new Controller(UserIndex.One);
            controllerThread = new Thread(pollControllerState);
            controllerThread.IsBackground = true;
            controllerThread.Start();
            this.Topmost = true;
            mainWindow = mw;
        }
        public void loadEmulatorsFromDB()
        {
            emulators.Clear();
            selectEmulatorListBox.Items.Clear();
            Emulator currentEmulator = null;
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
            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Emulators", sqlConnection);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                currentEmulator = new Emulator();
                currentEmulator.ID = Convert.ToInt32(sqlDataReader["ID"]);
                currentEmulator.name = sqlDataReader["name"].ToString();
                currentEmulator.executablePath = sqlDataReader["executablePath"].ToString();
                emulators.Add(currentEmulator);
            }
            if (emulators.Count.Equals(0))
            {
                customMessageBox csm = new customMessageBox("Error", "No emulators were saved in this program.");
                csm.ShowDialog();
            }
            foreach (Emulator emu in emulators)
            {
                populateEmulatorList(emu);
            }
        }
        private void populateEmulatorList(Emulator emu)
        {
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.HorizontalAlignment = HorizontalAlignment.Center;
            Border border = new Border();
            border.CornerRadius = new CornerRadius(10);
            border.Background = new SolidColorBrush(Color.FromArgb(255, 4, 66, 117));
            border.VerticalAlignment = VerticalAlignment.Center;
            border.HorizontalAlignment = HorizontalAlignment.Center;
            border.Width = 280;
            Label label = new Label();
            label.Name = $"LABEL_{emu.ID}";
            label.Margin = new Thickness(0);
            label.FontSize = 16;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.Foreground = new SolidColorBrush(Colors.White);
            label.Content = emu.name;
            border.Child = label;
            listBoxItem.Content = border;
            selectEmulatorListBox.Items.Add(listBoxItem);
            listLabels.Add(label);
        }
        public void pollControllerState()
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
                        Dispatcher.Invoke(() =>
                        {
                            for (int i = 0; i < emulators.Count; i++)
                            {
                                for (int j = 0; j < listLabels.Count; j++)
                                {
                                    if (emulators[i].ID == getEmuIDFromLabelName(listLabels[j].Name.ToString() + ""))
                                    {
                                        if (selectEmulatorListBox.SelectedIndex == j)
                                        {

                                            ProcessStartInfo startInfo = new ProcessStartInfo();
                                            startInfo.UseShellExecute = true;
                                            startInfo.FileName = emulators[j].executablePath;
                                            startInfo.Arguments = $"\"{mainWindow.games[GameID].executablePath}\"";
                                            Process.Start(startInfo);
                                            this.Close();
                                        }
                                    }
                                }
                            }
                        });
                    }
                    bool bPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                    if (bPressed && !previousB)
                    {
                        Dispatcher.Invoke(this.Close);
                        Dispatcher.Invoke(() => mainWindow.ShowDialog());
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
            if (selectEmulatorListBox.SelectedIndex > 0)
            {
                selectEmulatorListBox.SelectedIndex = selectEmulatorListBox.SelectedIndex - 1;
            }
        }
        public void moveCursorDown()
        {
            if (selectEmulatorListBox.SelectedIndex < selectEmulatorListBox.Items.Count - 1)
            {
                selectEmulatorListBox.SelectedIndex = selectEmulatorListBox.SelectedIndex + 1;
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            running = false;
        }
        private void selectEmulatorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private int getEmuIDFromLabelName(string labelName)
        {
            int underscoreIndex = labelName.IndexOf('_');
            if (underscoreIndex != -1)
            {
                labelName = labelName.Substring(underscoreIndex + 1);
            }
            int id = int.Parse(labelName);
            return (id);
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
