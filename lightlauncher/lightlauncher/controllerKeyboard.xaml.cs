﻿//By Sauraav Jayrajh
using SharpDX.XInput;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
        public TextBox publicTextBox;
        public bool isCaps = true;
        public bool isSpecialChars = false;
        public ListBoxItem activeItem;
        private bool previousDPadLeft = false;
        private bool previousDPadRight = false;
        private bool previousDPadUp = false;
        private bool previousDPadDown = false;
        private bool previousA = false;
        private bool previousB = false;
        private bool previousX = false;
        private bool previousY = false;
        public controllerKeyboard(TextBox textBox)
        {
            InitializeComponent();
            this.Topmost = true;
            publicTextBox = textBox;
            usersController = new Controller(UserIndex.One);
            controllerThread = new Thread(pollControllerState);
            Thread.Sleep(200);
            windowMovementThread = new Thread(moveWindow);
            windowMovementThread.IsBackground = true;
            windowMovementThread.Start();
            controllerThread.IsBackground = true;
            controllerThread.Start();
            activeItem = focusKey(currentRow, currentColumn);
        }
        private ListBoxItem focusKey(int row, int column)
        {
            clearAllListBoxSelections();
            ListBox currentListBox = getListBoxForRow(row);
            if (column < 0)
            {
                column = 0;
            }
            else if (column >= currentListBox.Items.Count)
            {
                column = currentListBox.Items.Count - 1;
            }

            ListBoxItem temp = ((ListBoxItem)currentListBox.Items[column]);
            temp.Focus();
            currentListBox.SelectedIndex = column;
            return temp;
        }
        private void pollControllerState()
        {
            while (running)
            {
                State state = usersController.GetState();
                //Debouncing logic to ensure only one keypress per button press
                bool dPadLeft = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || state.Gamepad.LeftThumbX < -9000;
                bool dPadRight = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || state.Gamepad.LeftThumbX > 9000;
                bool dPadUp = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) || state.Gamepad.LeftThumbY > 9000;
                bool dPadDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) || state.Gamepad.LeftThumbY < -9000;
                bool aPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
                bool bPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                bool xPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X);
                bool yPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);
                if (dPadLeft && !previousDPadLeft)
                {
                    Dispatcher.Invoke(moveCursorLeft);
                }
                if (dPadRight && !previousDPadRight)
                {
                    Dispatcher.Invoke(moveCursorRight);
                }
                if (dPadUp && !previousDPadUp)
                {
                    Dispatcher.Invoke(moveCursorUp);
                }
                if (dPadDown && !previousDPadDown)
                {
                    Dispatcher.Invoke(moveCursorDown);
                }
                if (aPressed && !previousA)
                {
                    Dispatcher.Invoke(() =>
                    {
                        PerformKeyPress(currentRow, currentColumn);
                    });
                }
                if (bPressed && !previousB)
                {
                    Dispatcher.Invoke(this.Close);
                }
                if (xPressed && !previousX)
                {
                    Dispatcher.Invoke(backspaceChar);
                }
                if (state.Gamepad.LeftTrigger == 255)
                {
                    Dispatcher.Invoke(() =>
                    {
                        key_shift_PreviewMouseLeftButtonDown(null, null);
                    });
                }
                if (state.Gamepad.RightTrigger == 255)
                {
                    Dispatcher.Invoke(() =>
                    {
                        key_specialChars_PreviewMouseLeftButtonDown(null, null);
                    });
                }
                if (yPressed && !previousY)
                {
                    Dispatcher.Invoke(() =>
                    {
                        key_spacebar_PreviewMouseLeftButtonDown(null, null);
                    });
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    Dispatcher.Invoke(() =>
                    {
                        key_clear_PreviewMouseLeftButtonDown(null, null);
                    });
                }
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start))
                {
                    Dispatcher.Invoke(() =>
                    {
                        key_enter_PreviewMouseLeftButtonDown(null, null);
                    });
                }
                previousDPadLeft = dPadLeft;
                previousDPadRight = dPadRight;
                previousDPadUp = dPadUp;
                previousDPadDown = dPadDown;
                previousA = aPressed;
                previousB = bPressed;
                previousX = xPressed;
                previousY = yPressed;
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
            switch (activeItem.Name.ToString())
            {
                case "key_shift":
                    key_shift_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_q":
                    key_q_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_w":
                    key_w_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_e":
                    key_e_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_r":
                    key_r_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_t":
                    key_t_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_y":
                    key_y_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_u":
                    key_u_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_i":
                    key_i_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_o":
                    key_o_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_p":
                    key_p_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_a":
                    key_a_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_s":
                    key_s_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_d":
                    key_d_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_f":
                    key_f_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_g":
                    key_g_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_h":
                    key_h_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_j":
                    key_j_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_k":
                    key_k_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_l":
                    key_l_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_z":
                    key_z_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_x":
                    key_x_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_c":
                    key_c_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_v":
                    key_v_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_b":
                    key_b_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_n":
                    key_n_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_m":
                    key_m_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_apostrophe":
                    key_apostrophe_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_comma":
                    key_comma_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_period":
                    key_period_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_spacebar":
                    key_spacebar_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_question":
                    key_question_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_clear":
                    key_clear_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_enter":
                    key_enter_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_backspace":
                    key_backspace_PreviewMouseLeftButtonDown(null, null);
                    break;
                case "key_specialChars":
                    key_specialChars_PreviewMouseLeftButtonDown(null, null);
                    break;
                default:
                    break;
            }
        }
        public void backspaceChar()
        {
            if (publicTextBox.Text.Length > 0)
            {
                publicTextBox.Text = publicTextBox.Text.Substring(0, publicTextBox.Text.Length - 1);
            }
        }
        public void moveCursorUp()
        {
            if (currentRow > 0)
            {
                currentRow--;
                currentColumn = Math.Min(currentColumn, getListBoxForRow(currentRow).Items.Count - 1);
                activeItem = focusKey(currentRow, currentColumn);
            }
        }
        public void moveCursorDown()
        {
            if (currentRow < 4)
            {
                int nextRow = currentRow + 1;
                currentColumn = Math.Min(currentColumn, getListBoxForRow(nextRow).Items.Count - 1);
                currentRow = nextRow;
                activeItem = focusKey(currentRow, currentColumn);
            }
        }
        public void moveCursorLeft()
        {
            if (currentColumn > 0)
            {
                currentColumn--;
                activeItem = focusKey(currentRow, currentColumn);
            }
        }
        public void moveCursorRight()
        {
            ListBox currentListBox = getListBoxForRow(currentRow);
            if (currentListBox != null && currentColumn < currentListBox.Items.Count - 1)
            {
                currentColumn++;
                activeItem = focusKey(currentRow, currentColumn);
            }
        }
        public void moveWindow()
        {
            const int threshold = 5000;
            while (running)
            {
                State state = usersController.GetState();
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
        private void key_q_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_q.Content;
        }
        private void key_w_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_w.Content;
        }
        private void key_e_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_e.Content;
        }
        private void key_r_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_r.Content;
        }
        private void key_t_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_t.Content;
        }
        private void key_y_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_y.Content;
        }
        private void key_u_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_u.Content;
        }
        private void key_i_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_i.Content;
        }
        private void key_o_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_o.Content;
        }
        private void key_p_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_p.Content;
        }
        private void key_a_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_a.Content;
        }
        private void key_s_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_s.Content;
        }
        private void key_d_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_d.Content;
        }
        private void key_f_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_f.Content;
        }
        private void key_g_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_g.Content;
        }
        private void key_h_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_h.Content;
        }
        private void key_j_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_j.Content;
        }
        private void key_k_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_k.Content;
        }
        private void key_l_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_l.Content;
        }
        private void key_apostrophe_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += "'";
        }
        private void key_z_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_z.Content;
        }
        private void key_x_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_x.Content;
        }
        private void key_c_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_c.Content;
        }
        private void key_v_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_v.Content;
        }
        private void key_b_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_b.Content;
        }
        private void key_n_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_n.Content;
        }
        private void key_m_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += label_key_m.Content;
        }
        private void key_comma_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += ",";
        }
        private void key_period_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += ".";
        }
        private void key_question_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += "?";
        }
        private void key_shift_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isCaps)
            {
                isCaps = false;
                label_key_q.Content = "q";
                label_key_w.Content = "w";
                label_key_e.Content = "e";
                label_key_r.Content = "r";
                label_key_t.Content = "t";
                label_key_y.Content = "y";
                label_key_u.Content = "u";
                label_key_i.Content = "i";
                label_key_o.Content = "o";
                label_key_p.Content = "p";
                label_key_a.Content = "a";
                label_key_s.Content = "s";
                label_key_d.Content = "d";
                label_key_f.Content = "f";
                label_key_g.Content = "g";
                label_key_h.Content = "h";
                label_key_j.Content = "j";
                label_key_k.Content = "k";
                label_key_l.Content = "l";
                label_key_z.Content = "z";
                label_key_x.Content = "x";
                label_key_c.Content = "c";
                label_key_v.Content = "v";
                label_key_b.Content = "b";
                label_key_n.Content = "n";
                label_key_m.Content = "m";
            }
            else
            {
                isCaps = true;
                label_key_q.Content = "Q";
                label_key_w.Content = "W";
                label_key_e.Content = "E";
                label_key_r.Content = "R";
                label_key_t.Content = "T";
                label_key_y.Content = "Y";
                label_key_u.Content = "U";
                label_key_i.Content = "I";
                label_key_o.Content = "O";
                label_key_p.Content = "P";
                label_key_a.Content = "A";
                label_key_s.Content = "S";
                label_key_d.Content = "D";
                label_key_f.Content = "F";
                label_key_g.Content = "G";
                label_key_h.Content = "H";
                label_key_j.Content = "J";
                label_key_k.Content = "K";
                label_key_l.Content = "L";
                label_key_z.Content = "Z";
                label_key_x.Content = "X";
                label_key_c.Content = "C";
                label_key_v.Content = "V";
                label_key_b.Content = "B";
                label_key_n.Content = "N";
                label_key_m.Content = "M";
            }
        }
        private void key_specialChars_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!isSpecialChars)
            {
                isSpecialChars = true;
                label_key_q.Content = "1";
                label_key_w.Content = "2";
                label_key_e.Content = "3";
                label_key_r.Content = "4";
                label_key_t.Content = "5";
                label_key_y.Content = "6";
                label_key_u.Content = "7";
                label_key_i.Content = "8";
                label_key_o.Content = "9";
                label_key_p.Content = "0";
                label_key_a.Content = "!";
                label_key_s.Content = "@";
                label_key_d.Content = "#";
                label_key_f.Content = "$";
                label_key_g.Content = "%";
                label_key_h.Content = "^";
                label_key_j.Content = "&";
                label_key_k.Content = "*";
                label_key_l.Content = "(";
                label_key_z.Content = ")";
                label_key_x.Content = "-";
                label_key_c.Content = "+";
                label_key_v.Content = "[";
                label_key_b.Content = "]";
                label_key_n.Content = "{";
                label_key_m.Content = "}";
            }
            else
            {
                isSpecialChars = false;
                isCaps = false;
                label_key_q.Content = "q";
                label_key_w.Content = "w";
                label_key_e.Content = "e";
                label_key_r.Content = "r";
                label_key_t.Content = "t";
                label_key_y.Content = "y";
                label_key_u.Content = "u";
                label_key_i.Content = "i";
                label_key_o.Content = "o";
                label_key_p.Content = "p";
                label_key_a.Content = "a";
                label_key_s.Content = "s";
                label_key_d.Content = "d";
                label_key_f.Content = "f";
                label_key_g.Content = "g";
                label_key_h.Content = "h";
                label_key_j.Content = "j";
                label_key_k.Content = "k";
                label_key_l.Content = "l";
                label_key_z.Content = "z";
                label_key_x.Content = "x";
                label_key_c.Content = "c";
                label_key_v.Content = "v";
                label_key_b.Content = "b";
                label_key_n.Content = "n";
                label_key_m.Content = "m";
            }
        }
        private void key_spacebar_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text += " ";
        }
        private void key_backspace_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (publicTextBox.Text.Length > 0)
            {
                publicTextBox.Text = publicTextBox.Text.Remove(publicTextBox.Text.Length - 1);
            }
        }
        private void key_enter_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }
        private void key_clear_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            publicTextBox.Text = string.Empty;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            running = false;
        }
    }
}
