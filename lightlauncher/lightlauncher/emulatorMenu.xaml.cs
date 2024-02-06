//By Sauraav Jayrajh
using SharpDX.XInput;
using System.Threading;
using System.Windows;

namespace lightlauncher
{
    public partial class emulatorMenu : Window
    {
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        private bool previousDPadLeftOrUp = false;
        private bool previousDPadRightOrDown = false;
        private bool previousA = false;
        private bool previousB = false;
        public MainWindow mainWindow;
        public emulatorMenu(MainWindow mw)
        {
            InitializeComponent();
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
                    bool aPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
                    if (aPressed && !previousA)
                    {
                        int selectedIndex = Dispatcher.Invoke(() => optionsListBox.SelectedIndex);
                        Dispatcher.Invoke(() =>
                        {
                            switch (selectedIndex)
                            {
                                case 0:
                                    customFileDialog cfd = new customFileDialog(mainWindow, "getEmulatorDetails");
                                    cfd.ShowDialog();
                                    break;
                                case 1:
                                    break;
                                case 2:
                                    break;
                                case 3:
                                    break;
                                case 4:
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
        private void optionsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
