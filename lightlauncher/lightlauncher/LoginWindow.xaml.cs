// By Sauraav Jayrajh
using Microsoft.Extensions.Configuration;
using SharpDX.XInput;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System;

namespace lightlauncher
{
    public partial class LoginWindow : Window
    {
        public static string uuid;
        public customMessageBox csm;
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        public controllerKeyboard onscreenKeyboard;
        private readonly AuthService authService;

        public LoginWindow()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            authService = new AuthService(configuration);

            InitializeComponent();
            usersController = new Controller(UserIndex.One);
            if (!usersController.IsConnected)
            {
                MessageBox.Show("No controller was detected! Please make sure you are using an Xbox or XInput Compatible Controller.", "Error");
                killProgram();
                return;
            }
            controllerThread = new Thread(pollControllerState);
            controllerThread.IsBackground = true;
            controllerThread.Start();
        }

        private void pollControllerState()
        {
            while (running)
            {
                if (!usersController.IsConnected)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Controller not detected! Please make sure you are using an Xbox or XInput Compatible Controller.", "Error");
                    });
                    killProgram();
                    return;
                }

                var state = usersController.GetState();
                bool dPadUp = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp);
                bool dPadDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown);
                bool aPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);

                if (dPadUp)
                {
                    Dispatcher.Invoke(moveCursorUp);
                }
                if (dPadDown)
                {
                    Dispatcher.Invoke(moveCursorDown);
                }
                if (aPressed)
                {
                    Dispatcher.Invoke(selectOption);
                }
                Thread.Sleep(125);
            }
        }

        private void moveCursorUp()
        {
            if (loginOptionsListBox.SelectedIndex > 0)
            {
                loginOptionsListBox.SelectedIndex = loginOptionsListBox.SelectedIndex - 1;
            }
        }

        private void moveCursorDown()
        {
            if (loginOptionsListBox.SelectedIndex < loginOptionsListBox.Items.Count - 1)
            {
                loginOptionsListBox.SelectedIndex = loginOptionsListBox.SelectedIndex + 1;
            }
        }

        private void selectOption()
        {
            switch (loginOptionsListBox.SelectedIndex)
            {
                case 0:
                    usernameTextBox.Clear();
                    usernameTextBox.Focus();
                    onscreenKeyboard = new controllerKeyboard(usernameTextBox);
                    onscreenKeyboard.ShowDialog();
                    onscreenKeyboard.Close();
                    break;
                case 1:
                    passwordBox.Clear();
                    passwordBox.Focus();
                    onscreenKeyboard = new controllerKeyboard(passwordBox);
                    onscreenKeyboard.ShowDialog();
                    onscreenKeyboard.Close();
                    break;
                case 2:
                    Dispatcher.Invoke(() => this.Hide());
                    Dispatcher.Invoke(performLogin);
                    break;
                case 3:
                    Dispatcher.Invoke(() => this.Hide());
                    Dispatcher.Invoke(performSignUp);
                    break;
            }
        }

        private async void performLogin()
        {
            try
            {
                var email = usernameTextBox.Text;
                var password = passwordBox.Text;
                var authResponse = await authService.SignInAsync(email, password);
                uuid = authResponse.LocalId;
                csm = new customMessageBox("Login Success", $"Logged in as {authResponse.Email}");
                csm.ShowDialog();
                MainWindow mainWindow = new MainWindow();
                mainWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                csm = new customMessageBox("Login Failure", $"Login failed: {ex.Message}");
                csm.ShowDialog();
                this.ShowDialog();
            }
        }

        private void performSignUp()
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }

        private void killProgram()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            running = false;
        }
    }
}