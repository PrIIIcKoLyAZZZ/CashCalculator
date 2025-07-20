using System;
using System.Windows;
using System.Windows.Controls;

namespace CashCalculator.Views
{
    public partial class ToolbarView : UserControl
    {
        public event EventHandler? CopyRequested;
        public event EventHandler? ClearRequested;
        public event EventHandler<bool>? TouchModeToggled;

        public ToolbarView() => InitializeComponent();

        private void OnCopyClick(object s, RoutedEventArgs e)   => CopyRequested?.Invoke(this, EventArgs.Empty);
        private void OnClearClick(object s, RoutedEventArgs e)  => ClearRequested?.Invoke(this, EventArgs.Empty);
        private void OnTouchChecked(object s, RoutedEventArgs e)=> TouchModeToggled?.Invoke(this, true);
        private void OnTouchUnchecked(object s, RoutedEventArgs e)=> TouchModeToggled?.Invoke(this, false);
    }
}