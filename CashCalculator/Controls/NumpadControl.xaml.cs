using System;
using System.Windows;
using System.Windows.Controls;

namespace CashCalculator.Controls
{
    /// <summary>
    /// UserControl для ввода цифр через нумпад.
    /// Поднимает событие EnterPressed с введённым значением.
    /// </summary>
    public partial class NumpadControl : UserControl
    {
        private string _buffer = string.Empty;

        /// <summary>
        /// Срабатывает при нажатии кнопки «✓».
        /// Аргумент — финальное значение буфера.
        /// </summary>
        public event EventHandler<string>? EnterPressed;

        public NumpadControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Показывает контрол и устанавливает начальное значение.
        /// </summary>
        public void Show(string initial)
        {
            _buffer = initial;
            Display.Text = initial;
            Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Скрывает контрол и сбрасывает буфер.
        /// </summary>
        public void Hide()
        {
            Visibility = Visibility.Collapsed;
            _buffer = string.Empty;
            Display.Text = string.Empty;
        }

        private void OnDigitClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                _buffer += btn.Content;
                Display.Text = _buffer;
            }
        }

        private void OnBackspaceClick(object sender, RoutedEventArgs e)
        {
            if (_buffer.Length > 0)
            {
                _buffer = _buffer[..^1];
                Display.Text = _buffer;
            }
        }

        private void OnEnterClick(object sender, RoutedEventArgs e)
        {
            EnterPressed?.Invoke(this, _buffer);
            Hide();
        }
    }
}