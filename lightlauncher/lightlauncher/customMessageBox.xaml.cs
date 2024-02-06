//By Sauraav Jayrajh
using SharpDX.XInput;
using System.ComponentModel;
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
        private bool previousStart = false;
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
                    killProgram();
                    return;
                }
                else
                {
                    State state = usersController.GetState();
                    bool startPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);
                    if (startPressed && !previousStart)
                    {
                        Dispatcher.Invoke(() => closeAndContinue());
                    }
                    previousStart = startPressed;
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
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            running = false;
        }
    }
}
