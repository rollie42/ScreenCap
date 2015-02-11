using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Text;
using System.Threading.Tasks;

using NHotkey;

namespace ScreenCap
{
    class Commands
    {
        static Commands()
        {
            var gestures = new InputGestureCollection();
            gestures.Add(new KeyGesture(Key.Escape));
            CloseWindow = new RoutedUICommand("Close Window", "CloseWindow", typeof(Commands), gestures);
            CloseWindowDefaultBinding = new CommandBinding(CloseWindow,
                CloseWindowExecute, CloseWindowCanExecute);
            NHotkey.Wpf.HotkeyManager.Current.AddOrReplace("Decrement", Key.Subtract, ModifierKeys.Control | ModifierKeys.Alt, ChooseRegion);
        }

        public static CommandBinding CloseWindowDefaultBinding { get; private set; }
        public static RoutedUICommand CloseWindow { get; private set; }

        static void CloseWindowCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = sender != null && sender is System.Windows.Window;
            e.Handled = true;
        }
        static void CloseWindowExecute(object sender, ExecutedRoutedEventArgs e)
        {
             ((System.Windows.Window)sender).Close();
        }

        static void ChooseRegion(object sender, HotkeyEventArgs e)
        {
            if(Application.Current.MainWindow.IsVisible)
            {
                Application.Current.MainWindow.Hide();
            }
            else
            {
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.Activate();
            }
        }
    }
}
