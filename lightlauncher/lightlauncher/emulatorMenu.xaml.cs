//By Sauraav Jayrajh
using lightlauncher.Migrations;
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
    public partial class emulatorMenu : Window
    {
        private string currentDir = string.Empty;
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        private bool previousDPadLeftOrUp = false;
        private bool previousDPadRightOrDown = false;
        private bool previousB = false;
        private bool previousL2 = false;
        public List<Emulator> emulators = new List<Emulator>();
        private bool previousY = false;
        public List<Label> listLabels = new List<Label>();
        public MainWindow mainWindow;
        public emulatorMenu(MainWindow mw)
        {
            InitializeComponent();
            loadEmulatorsFromDB();
            usersController = new Controller(UserIndex.One);
            controllerThread = new Thread(pollControllerState);
            controllerThread.IsBackground = true;
            controllerThread.Start();
            this.Topmost = true;
            mainWindow = mw;
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

                    bool L2Pressed = false;
                    if (state.Gamepad.LeftTrigger == 255)
                    {
                        L2Pressed = true;
                    }
                    if (L2Pressed && !previousL2)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                removeGame();
                                customMessageBox csm = new customMessageBox("Success", $"The emulator you selected has been removed from your library");
                                csm.ShowDialog();
                                csm.Close(); ;
                            }
                            catch (Exception)
                            {
                                customMessageBox csm = new customMessageBox("Error", "You need to have an emulator in your library to remove.");
                                csm.ShowDialog();
                                csm.Close(); ;
                            }
                        });
                    }
                    bool bPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                    if (bPressed && !previousB)
                    {
                        Dispatcher.Invoke(this.Close);
                        Dispatcher.Invoke(() => mainWindow.ShowDialog());
                    }
                    bool yPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);
                    if (yPressed && !previousY)
                    {
                        Dispatcher.Invoke(() => this.Hide());
                        Dispatcher.Invoke(showAddEmulatorWindow);
                    }
                    previousDPadLeftOrUp = dPadLeftOrUp;
                    previousDPadRightOrDown = dPadRightOrDown;
                    previousB = bPressed;
                    previousY = yPressed;
                    Thread.Sleep(120);
                }
            }
        }
        public void removeGame()
        {
            for (int i = 0; i < emulators.Count; i++)
            {
                for (int j = 0; j < listLabels.Count; j++)
                {
                    if (emulators[i].ID == getEmuIDFromLabelName(listLabels[j].Name.ToString() + ""))
                    {
                        if (optionsListBox.SelectedIndex == j)
                        {
                            currentDir = emulators[j].executablePath;
                        }
                    }
                }
            }
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
            using (sqlConnection)
            {
                using (SqlCommand sqlCommand = new SqlCommand("DELETE FROM Emulators WHERE executablePath=@executablePath", sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@executablePath", currentDir);
                    sqlCommand.ExecuteNonQuery();
                }
            }
            loadEmulatorsFromDB();
        }
        public void showAddEmulatorWindow()
        {
            AddEmulatorForm addEmulatorForm = new AddEmulatorForm(mainWindow, this);
            addEmulatorForm.ShowDialog();
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
        private void optionsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            running = false;
        }
        private void populateEmulatorList(Emulator emu)
        {
            ListBoxItem listBoxItem = new ListBoxItem();
            listBoxItem.Name = $"listBoxItem_{emu.ID}";
            listBoxItem.HorizontalAlignment = HorizontalAlignment.Center;
            Border border = new Border();
            border.CornerRadius = new CornerRadius(10);
            border.Background = new SolidColorBrush(Color.FromRgb(4, 66, 117));
            border.VerticalAlignment = VerticalAlignment.Center;
            border.HorizontalAlignment = HorizontalAlignment.Center;
            border.Width = 590;
            StackPanel stackPanel = new StackPanel();
            stackPanel.SetValue(Grid.RowProperty, 0);
            stackPanel.SetValue(Grid.ColumnProperty, 0);
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            stackPanel.Height = 80;
            Label gameNeedsEmulatorLabel = new Label();
            gameNeedsEmulatorLabel.Name = $"LABEL_{emu.ID}";
            gameNeedsEmulatorLabel.Margin = new Thickness(0);
            gameNeedsEmulatorLabel.FontSize = 16;
            gameNeedsEmulatorLabel.VerticalAlignment = VerticalAlignment.Center;
            gameNeedsEmulatorLabel.HorizontalAlignment = HorizontalAlignment.Center;
            gameNeedsEmulatorLabel.Foreground = Brushes.White;
            gameNeedsEmulatorLabel.Content = emu.name;
            TextBox gameNameTextBox = new TextBox();
            gameNameTextBox.Name = $"gameNameTextBox_{emu.ID}";
            gameNameTextBox.Width = 550;
            gameNameTextBox.Height = 30;
            gameNameTextBox.Background = new SolidColorBrush(Color.FromRgb(1, 38, 68));
            gameNameTextBox.Margin = new Thickness(0);
            gameNameTextBox.IsReadOnly = true;
            gameNameTextBox.Text = emu.executablePath;
            gameNameTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            gameNameTextBox.Style = (Style)Application.Current.Resources["RoundedTextBox"];
            stackPanel.Children.Add(gameNeedsEmulatorLabel);
            stackPanel.Children.Add(gameNameTextBox);
            border.Child = stackPanel;
            listBoxItem.Content = border;
            optionsListBox.Items.Add(listBoxItem);
            listLabels.Add(gameNeedsEmulatorLabel);
        }
        public void loadEmulatorsFromDB()
        {
            emulators.Clear();
            optionsListBox.Items.Clear();
            Emulator currentEmulator = null; SqlConnection sqlConnection = null;
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
