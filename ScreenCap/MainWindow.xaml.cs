using System;
using System.Collections.Generic;
using System.Diagnostics;
using Drawing = System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScreenCap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Data Members

        /// <summary>
        /// Set to 'true' when the left mouse-button is down.
        /// </summary>
        private bool isLeftMouseButtonDownOnWindow = false;

        /// <summary>
        /// Set to 'true' when dragging the 'selection rectangle'.
        /// Dragging of the selection rectangle only starts when the left mouse-button is held down and the mouse-cursor
        /// is moved more than a threshold distance.
        /// </summary>
        private bool isDraggingSelectionRect = false;

        /// <summary>
        /// Records the location of the mouse (relative to the window) when the left-mouse button has pressed down.
        /// </summary>
        private Point origMouseDownPoint;

        /// <summary>
        /// The threshold distance the mouse-cursor must move before drag-selection begins.
        /// </summary>
        private static readonly double DragThreshold = 5;

        private ImgurClient imgurClient = new ImgurClient();

        #endregion Data Members

        public MainWindow()
        {
            InitializeComponent();
            CommandBindings.Add(Commands.CloseWindowDefaultBinding);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isLeftMouseButtonDownOnWindow = true;
                origMouseDownPoint = e.GetPosition(this);

                this.CaptureMouse();

                e.Handled = true;
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (isDraggingSelectionRect)
                {
                    //
                    // Drag selection has ended, apply the 'selection rectangle'.
                    //

                    isDraggingSelectionRect = false;                    
                    Point curMouseDownPoint = e.GetPosition(this);
                    ApplyDragSelectionRect(new Rect(PointToScreen(origMouseDownPoint), PointToScreen(curMouseDownPoint)));
                    Grid1.Visibility = System.Windows.Visibility.Hidden;
                    e.Handled = true;
                }

                if (isLeftMouseButtonDownOnWindow)
                {
                    isLeftMouseButtonDownOnWindow = false;
                    this.ReleaseMouseCapture();

                    e.Handled = true;
                }
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingSelectionRect)
            {
                //
                // Drag selection is in progress.
                //
                Point curMouseDownPoint = e.GetPosition(this);
                UpdateDragSelectionRect(new Rect(origMouseDownPoint, curMouseDownPoint));

                e.Handled = true;
            }
            else if (isLeftMouseButtonDownOnWindow)
            {
                //
                // The user is left-dragging the mouse,
                // but don't initiate drag selection until
                // they have dragged past the threshold value.
                //
                Point curMouseDownPoint = e.GetPosition(this);
                var dragDelta = curMouseDownPoint - origMouseDownPoint;
                double dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance > DragThreshold)
                {
                    isDraggingSelectionRect = true;
                    InitDragSelectionRect(new Rect(origMouseDownPoint, curMouseDownPoint));
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void InitDragSelectionRect(Rect rect)
        {
            UpdateDragSelectionRect(rect);
        }

        private void UpdateDragSelectionRect(Rect rect)
        {
            leftColumn.Width = new GridLength(rect.X);
            topRow.Height = new GridLength(rect.Y);
            middleColumn.Width = new GridLength(rect.Width);
            middleRow.Height = new GridLength(rect.Height);            
        }

        /// <summary>
        /// Select all nodes that are in the drag selection rectangle.
        /// </summary>
        private async Task ApplyDragSelectionRect(Rect rect)
        {            
            var screenshot = new Drawing.Bitmap((int)rect.Width, (int)rect.Height);
            var graphicsObject = Drawing.Graphics.FromImage(screenshot);
            graphicsObject.CopyFromScreen((int)rect.X, (int)rect.Y, 0, 0, new Drawing.Size((int)rect.Width, (int)rect.Height), Drawing.CopyPixelOperation.SourceCopy);

            // save to disk
            var imageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + "." + imageFormat.ToString();
            var fs = File.Open(fileName, FileMode.OpenOrCreate);
            screenshot.Save(fs, imageFormat);
            fs.Close();

            System.Windows.Clipboard.SetText(fileName);

            using (var memoryStream = new MemoryStream())
            {
                screenshot.Save(memoryStream, imageFormat);
                await this.imgurClient.UploadImage(memoryStream);                
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowsInteropHelper.AddExStyle(ExtendedWindowStyles.WS_EX_TOOLWINDOW);
        }
    }
}
