using SharpDX.XInput;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace lightlauncher
{
    public partial class controllerKeyboard : Window
    {
        private bool aButtonPressed = false;
        private bool bButtonPressed = false;
        private bool xButtonPressed = false;
        private Controller usersController;
        private Thread windowMovementThread;
        private Thread controllerThread;
        private volatile bool running = true;
        private int currentRow = 0;
        private int currentColumn = 0;
        public AddGameForm publicAGF;
        public bool isCaps = true;

        public controllerKeyboard(AddGameForm agf)
        {
            InitializeComponent();
            publicAGF = agf;
            usersController = new Controller(UserIndex.One);
            controllerThread = new Thread(pollControllerState);
            Thread.Sleep(200);
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
        private void pollControllerState()
        {
            while (running)
            {
                SharpDX.XInput.State state = usersController.GetState();
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || state.Gamepad.LeftThumbX < -9000)
                {
                    Dispatcher.Invoke(moveCursorLeft);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || state.Gamepad.LeftThumbX > 9000)
                {
                    Dispatcher.Invoke(moveCursorRight);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) || state.Gamepad.LeftThumbY > 9000)
                {
                    Dispatcher.Invoke(moveCursorUp);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) || state.Gamepad.LeftThumbY < -9000)
                {
                    Dispatcher.Invoke(moveCursorDown);
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
                {
                    if (!aButtonPressed)
                    {
                        aButtonPressed = true;
                        Dispatcher.Invoke(() =>
                        {
                            PerformKeyPress(currentRow, currentColumn);
                        });
                    }
                }
                else
                {
                    aButtonPressed = false;
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
                {
                    if (!bButtonPressed)
                    {
                        bButtonPressed = true;
                        Dispatcher.Invoke(this.Close);
                    }
                }
                else
                {
                    bButtonPressed = false;
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X))
                {
                    if (!xButtonPressed)
                    {
                        xButtonPressed = true;
                        Dispatcher.Invoke(backspaceChar);
                    }
                }
                else
                {
                    xButtonPressed = false;
                }
                Thread.Sleep(120);
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
        public void PerformKeyPress(int row, int column)
        {
            ListBox currentListBox = getListBoxForRow(row);
            if (currentListBox != null && currentListBox.Items.Count > column)
            {
                ListBoxItem currentListBoxItem = (ListBoxItem)currentListBox.Items[column];
                string currentKey = currentListBoxItem.Name.ToString();
                switch (currentKey)
                {
                    case "key_shift":
                        if (isCaps)
                        {
                            isCaps = false;
                            label_key_a.Content = "a";
                            label_key_b.Content = "b";
                            label_key_c.Content = "c";
                            label_key_d.Content = "d";
                            label_key_e.Content = "e";
                            label_key_f.Content = "f";
                            label_key_g.Content = "g";
                            label_key_h.Content = "h";
                            label_key_i.Content = "i";
                            label_key_j.Content = "j";
                            label_key_k.Content = "k";
                            label_key_l.Content = "l";
                            label_key_m.Content = "m";
                            label_key_n.Content = "n";
                            label_key_o.Content = "o";
                            label_key_p.Content = "p";
                            label_key_q.Content = "q";
                            label_key_r.Content = "r";
                            label_key_s.Content = "s";
                            label_key_t.Content = "t";
                            label_key_u.Content = "u";
                            label_key_v.Content = "v";
                            label_key_w.Content = "w";
                            label_key_x.Content = "x";
                            label_key_y.Content = "y";
                            label_key_z.Content = "z";
                        }
                        else
                        {
                            isCaps = true;
                            label_key_a.Content = "A";
                            label_key_b.Content = "B";
                            label_key_c.Content = "C";
                            label_key_d.Content = "D";
                            label_key_e.Content = "E";
                            label_key_f.Content = "F";
                            label_key_g.Content = "G";
                            label_key_h.Content = "H";
                            label_key_i.Content = "I";
                            label_key_j.Content = "J";
                            label_key_k.Content = "K";
                            label_key_l.Content = "L";
                            label_key_m.Content = "M";
                            label_key_n.Content = "N";
                            label_key_o.Content = "O";
                            label_key_p.Content = "P";
                            label_key_q.Content = "Q";
                            label_key_r.Content = "R";
                            label_key_s.Content = "S";
                            label_key_t.Content = "T";
                            label_key_u.Content = "U";
                            label_key_v.Content = "V";
                            label_key_w.Content = "W";
                            label_key_x.Content = "X";
                            label_key_y.Content = "Y";
                            label_key_z.Content = "Z";
                        }
                        break;
                    case "key_q":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_q.Content.ToString();
                        }
                        break;
                    case "key_w":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_w.Content.ToString();
                        }
                        break;
                    case "key_e":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_e.Content.ToString();
                        }
                        break;
                    case "key_r":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_r.Content.ToString();
                        }
                        break;
                    case "key_t":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_t.Content.ToString();
                        }
                        break;
                    case "key_y":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_y.Content.ToString();
                        }
                        break;
                    case "key_u":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_u.Content.ToString();
                        }
                        break;
                    case "key_i":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_i.Content.ToString();
                        }
                        break;
                    case "key_o":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_o.Content.ToString();
                        }
                        break;
                    case "key_p":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_p.Content.ToString();
                        }
                        break;
                    case "key_a":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_a.Content.ToString();
                        }
                        break;
                    case "key_s":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_s.Content.ToString();
                        }
                        break;
                    case "key_d":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_d.Content.ToString();
                        }
                        break;
                    case "key_f":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_f.Content.ToString();
                        }
                        break;
                    case "key_g":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_g.Content.ToString();
                        }
                        break;
                    case "key_h":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_h.Content.ToString();
                        }
                        break;
                    case "key_j":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_j.Content.ToString();
                        }
                        break;
                    case "key_k":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_k.Content.ToString();
                        }
                        break;
                    case "key_l":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_l.Content.ToString();
                        }
                        break;
                    case "key_z":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_z.Content.ToString();
                        }
                        break;
                    case "key_x":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_x.Content.ToString();
                        }
                        break;
                    case "key_c":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_c.Content.ToString();
                        }
                        break;
                    case "key_v":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_v.Content.ToString();
                        }
                        break;
                    case "key_b":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_b.Content.ToString();
                        }
                        break;
                    case "key_n":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_n.Content.ToString();
                        }
                        break;
                    case "key_m":
                        if (isCaps)
                        {
                            publicAGF.gameNameTextBox.Text += label_key_m.Content.ToString();
                        }
                        break;
                    case "key_apostrophe":
                        publicAGF.gameNameTextBox.Text += "'";
                        break;
                    case "key_comma":
                        publicAGF.gameNameTextBox.Text += ",";
                        break;
                    case "key_period":
                        publicAGF.gameNameTextBox.Text += ".";
                        break;
                    case "key_space":
                        publicAGF.gameNameTextBox.Text += " ";
                        break;
                    case "key_question":
                        publicAGF.gameNameTextBox.Text += "?";
                        break;
                    case "key_clear":
                        publicAGF.gameNameTextBox.Text = string.Empty;
                        break;
                    case "key_enter":
                        Thread.Sleep(750);
                        Dispatcher.Invoke(this.Close);
                        break;
                    case "key_backspace":
                        backspaceChar();
                        break;
                    default:
                        break;
                }
            }
        }

        public void backspaceChar()
        {
            if (publicAGF.gameNameTextBox.Text.Length > 0)
            {
                publicAGF.gameNameTextBox.Text = publicAGF.gameNameTextBox.Text.Substring(0, publicAGF.gameNameTextBox.Text.Length - 1);
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
        public void moveWindow()
        {
            const int threshold = 5000;
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
