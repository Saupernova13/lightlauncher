﻿using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using SharpDX.XInput;
using System.Windows.Input;

namespace lightlauncher
{
    public partial class MainWindow : Window
    {
        //Object Of Controller
        private Controller usersController;
        //Instance Of Thread
        private Thread controllerThread;
        //Boolean To Check If Thread Is Running
        //Note: Volatile keyword ensures that field may be modified by multiple threads at the same time
        private volatile bool running = true;

        public MainWindow()
        {
            InitializeComponent();
            //Map the controller index to Port 1
            usersController = new Controller(UserIndex.One);
            
            if (!usersController.IsConnected)
            {
                MessageBox.Show("No controller is not detected!\nPlease make sure you are using an Xbox or XInput Compatible Controller.");
                Application.Current.Shutdown();
                return;
            }
            // Start the thread for polling controller state
            controllerThread = new Thread(pollControllerState);
            //Make sure the program will still read input when the window is not in focus
            controllerThread.IsBackground = true; 
            controllerThread.Start();
        }

        private void pollControllerState()
        {
            while (running)
            {
                //Get state of controller 
                State state = usersController.GetState();
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X))
                {
                    //If button is pressed, call the method, but do it safely via the dispatcher. The reason we
                    //do this is because the thread is not the same as the UI thread, and we cannot modify UI elements
                    //from a non-UI thread
                    Dispatcher.Invoke(ChangeBackgroundColor);
                }
                //This means that there will be a 150ms gap until another button press is registered
                Thread.Sleep(150); 
            }
        }

        private void ChangeBackgroundColor()
        {
            var random = new Random();
            var color = Color.FromRgb((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
            Background = new SolidColorBrush(color);
        }

        protected override void OnClosed(EventArgs e)
        {
            running = false; // Indicate that the thread should no longer run.
            controllerThread.Join(); // Wait for the thread to finish executing to avoid any potential issues.
            base.OnClosed(e);
        }
    }
}