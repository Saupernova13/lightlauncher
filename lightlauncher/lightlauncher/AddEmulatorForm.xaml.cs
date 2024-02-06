//By Sauraav Jayrajh
using SharpDX.XInput;
using System.Threading;
using System.Windows;
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
        public static Emulator currentEmulator;
        public MainWindow mainWindow;
        public AddEmulatorForm()
        {
            InitializeComponent();
        }
    }
}
