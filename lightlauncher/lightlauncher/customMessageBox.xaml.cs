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
            Topmost = true;
        }
        public void pollControllerState()
        {
            while (running)
            {
                if (!usersController.IsConnected)
                {
                    MessageBox.Show("No controller is not detected!\nPlease make sure you are using an Xbox or XInput Compatible Controller.");
                    killProgram();
                    return;
                }
                else
                {
                    State state = usersController.GetState();
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
                    {
                            Dispatcher.Invoke(() => closeAndContinue());
                    }
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
                    {
                            Dispatcher.Invoke(() => closeAndContinue());
                    }
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
            mainWindow.Show();
            this.Close();
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
