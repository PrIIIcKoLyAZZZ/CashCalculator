using System;
using System.Windows;
using System.Windows.Controls;

namespace CashCalculator.Views
{
    public partial class NumpadView : UserControl
    {
        public event EventHandler<char>? DigitPressed;
        public event EventHandler? BackspacePressed;
        public event EventHandler? EnterPressed;

        public NumpadView()
        {
            InitializeComponent();
        }

        public void Show(string initial)
        {
            Display.Text = initial;
            Popup.IsOpen = true;
        }

        private void Digit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string s && s.Length == 1)
            {
                DigitPressed?.Invoke(this, s[0]);
                Display.Text += s;
            }
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (Display.Text.Length > 0)
                Display.Text = Display.Text[..^1];
            BackspacePressed?.Invoke(this, EventArgs.Empty);
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            EnterPressed?.Invoke(this, EventArgs.Empty);
            Popup.IsOpen = false;
        }
    }
}