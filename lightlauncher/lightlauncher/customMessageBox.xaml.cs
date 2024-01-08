using SharpDX.XInput;
using System.Threading;
using System.Windows;

namespace lightlauncher
{
    public partial class customMessageBox : Window
    {
        private Controller usersController;
        private Thread controllerThread;
        private volatile bool running = true;
        public static MainWindow mainWindow;
        private bool previousB = false;
        private bool previousX = false;
        public customMessageBox(MainWindow mw, string csmTitle, string csmContent)
        {
            InitializeComponent();
            mainWindow = mw;
            usersController = new Controller(UserIndex.One);
            controllerThread = new Thread(pollControllerState);
            controllerThread.IsBackground = true;
            controllerThread.Start();
            csmWindowTitle.Content = csmTitle;
            csmMessageContent.Text = csmContent;
            this.Topmost = true;
        }
        public void pollControllerState()
        {
            while (running)
            {
                if (!usersController.IsConnected)
                {
                    // Instead of showing a MessageBox, which could block the controller thread,
                    // you might opt to handle this in a way that is better integrated with your application's UI.
                    killProgram();
                    return;
                }
                else
                {
                    State state = usersController.GetState();

                    // Get current button states
                    bool bPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                    bool xPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X);

                    // Check for state transitions from not-pressed to pressed
                    if (bPressed && !previousB)
                    {
                        Dispatcher.Invoke(() => closeAndContinue());
                    }

                    if (xPressed && !previousX)
                    {
                        Dispatcher.Invoke(() => closeAndContinue());
                    }

                    // Remember button states for the next poll
                    previousB = bPressed;
                    previousX = xPressed;

                    // Sleep to avoid high CPU load
                    Thread.Sleep(125);
                }
            }
        }
        public void killProgram()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }
        public void closeAndContinue()
        {
            this.Close();
            //mainWindow.Show();
        }
        public void show()
        {
            this.show();
        }
        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            closeAndContinue();
        }
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            closeAndContinue();
        }
    }
}
