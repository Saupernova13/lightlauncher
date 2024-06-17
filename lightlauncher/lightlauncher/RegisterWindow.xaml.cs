//By Sauraav Jayrajh
using SharpDX.XInput;
using SharpDX.XInput;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace lightlauncher
{
    public partial class RegisterWindow : Window
    {
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        public controllerKeyboard onscreenKeyboard;

        public RegisterWindow()
        {
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
            if (registerOptionsListBox.SelectedIndex > 0)
            {
                registerOptionsListBox.SelectedIndex = registerOptionsListBox.SelectedIndex - 1;
            }
        }

        private void moveCursorDown()
        {
            if (registerOptionsListBox.SelectedIndex < registerOptionsListBox.Items.Count - 1)
            {
                registerOptionsListBox.SelectedIndex = registerOptionsListBox.SelectedIndex + 1;
            }
        }

        private void selectOption()
        {
            switch (registerOptionsListBox.SelectedIndex)
            {
                case 0:
                    registerUsernameTextBox.Clear();
                    registerUsernameTextBox.Focus();
                    onscreenKeyboard = new controllerKeyboard(registerUsernameTextBox);
                    onscreenKeyboard.ShowDialog();
                    onscreenKeyboard.Close();
                    break;
                case 1:
                    registerPasswordBox.Clear();
                    registerPasswordBox.Focus();
                    onscreenKeyboard = new controllerKeyboard(registerPasswordBox);
                    onscreenKeyboard.ShowDialog();
                    onscreenKeyboard.Close();
                    break;
                case 2:
                    confirmPasswordBox.Clear();
                    confirmPasswordBox.Focus();
                    onscreenKeyboard = new controllerKeyboard(confirmPasswordBox);
                    onscreenKeyboard.ShowDialog();
                    onscreenKeyboard.Close();
                    break;
                case 3:
                    Dispatcher.Invoke(() => this.Hide());
                    Dispatcher.Invoke(performSignUp);
                    break;
                case 4:
                    Dispatcher.Invoke(() => this.Hide());
                    Dispatcher.Invoke(performLogin);
                    break;
            }
        }

        private void performLogin()
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
        }
        private void performSignUp()
        {
            MessageBox.Show("Sign up button pressed!");
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