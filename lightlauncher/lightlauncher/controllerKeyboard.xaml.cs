using SharpDX.XInput;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.AxHost;

namespace lightlauncher
{
    public partial class controllerKeyboard : Window
    {
        private Controller usersController;
        private Thread windowMovementThread;
        private Thread controllerThread;
        private volatile bool running = true;
        private int currentRow = 0;
        private int currentColumn = 0;

        public controllerKeyboard()
        {
            InitializeComponent();
            usersController = new Controller(UserIndex.One);
            controllerThread = new Thread(pollControllerState);
            windowMovementThread = new Thread(moveWindow);
            windowMovementThread.IsBackground = true;
            windowMovementThread.Start();
            controllerThread.IsBackground = true;
            controllerThread.Start();
            focusKey(currentRow, currentColumn);
        }
        private void focusKey(int row, int column)
        {
            clearAllListBoxSelections();
            ListBox currentListBox = getListBoxForRow(row);
            if (currentListBox != null && currentListBox.Items.Count > column)
            { 
                ((ListBoxItem)currentListBox.Items[column]).Focus();
                currentListBox.SelectedIndex = column;
            }
        }
        private void clearAllListBoxSelections()
        {
            keyboardListBox_Row_1.UnselectAll();
            keyboardListBox_Row_2.UnselectAll();
            keyboardListBox_Row_3.UnselectAll();
            keyboardListBox_Row_4.UnselectAll();
            keyboardListBox_Row_5.UnselectAll();
        }
        private ListBox getListBoxForRow(int row)
        {
            switch (row)
            {
                case 0:
                    return keyboardListBox_Row_1;
                case 1:
                    return keyboardListBox_Row_2;
                case 2:
                    return keyboardListBox_Row_3;
                case 3:
                    return keyboardListBox_Row_4;
                case 4:
                    return keyboardListBox_Row_5;
                default:
                    return null;
            }
        }
        private void pollControllerState()
        {
            while (running)
            {
                SharpDX.XInput.State state = usersController.GetState();
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft))
                {
                    Dispatcher.Invoke(moveCursorLeft);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight))
                {
                    Dispatcher.Invoke(moveCursorRight);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp))
                {
                    Dispatcher.Invoke(moveCursorUp);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown))
                {
                    Dispatcher.Invoke(moveCursorDown);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Simulate a key press using the Enter or Space key, etc.
                        // For example:
                        // PerformKeyPress(currentRow, currentColumn);
                    });
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
                {
                    Dispatcher.Invoke(this.Close);
                }
                Thread.Sleep(120);
            }
        }
        public void moveCursorUp()
        {
            if (currentRow > 0)
            {
                currentRow--;
                focusKey(currentRow, currentColumn);
            }
        }
        public void moveCursorDown()
        {
            if (currentRow < 4)
            {
                currentRow++;
                focusKey(currentRow, currentColumn);
            }
        }
        public void moveCursorLeft()
        {
            if (currentColumn > 0)
            {
                currentColumn--;
                focusKey(currentRow, currentColumn);
            }
        }
        public void moveCursorRight()
        {
            ListBox currentListBox = getListBoxForRow(currentRow);
            if (currentListBox != null && currentColumn < currentListBox.Items.Count - 1)
            {
                currentColumn++;
                focusKey(currentRow, currentColumn);
            }
        }
        private void keyboardListBox_Row_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void keyboardListBox_Row_2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void keyboardListBox_Row_3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void keyboardListBox_Row_4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void keyboardListBox_Row_5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        public void moveWindow()
        { const int threshold = 5000;
            while (running)
            {
                SharpDX.XInput.State state = usersController.GetState();
                float rightThumbX = state.Gamepad.RightThumbX;
                float rightThumbY = state.Gamepad.RightThumbY;
                if (Math.Abs(rightThumbX) > threshold || Math.Abs(rightThumbY) > threshold)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Point currentPosition = new Point(Left, Top);
                        double dx = rightThumbX / 32768.0 * 10;
                        double dy = -rightThumbY / 32768.0 * 10;
                        Left = currentPosition.X + dx;
                        Top = currentPosition.Y + dy;
                    });
                }

                Thread.Sleep(10);
            } 
        }
    }
}
