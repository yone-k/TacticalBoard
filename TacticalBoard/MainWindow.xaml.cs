using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TacticalBoard
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        bool IsDraw;
        String lineName;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Thumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            var thumb = sender as Thumb;
            lineName = thumb.Name + "Line";
            Line line = Peace.FindName(lineName) as Line;
            line.Visibility = Visibility.Collapsed;
            if (null != thumb)
            {
                var border = thumb.Template.FindName("Thumb_Border", thumb) as Border;
                if (null != border)
                {
                    border.BorderThickness = new Thickness(1);
                }
            }
        }

        private void Thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var thumb = sender as Thumb;
            if (null != thumb)
            {
                var border = thumb.Template.FindName("Thumb_Border", thumb) as Border;
                if (null != border)
                {
                    border.BorderThickness = new Thickness(0);
                }
            }
        }

        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var thumb = sender as Thumb;
            if (null != thumb)
            {
                var x = Canvas.GetLeft(thumb) + e.HorizontalChange;
                var y = Canvas.GetTop(thumb) + e.VerticalChange;

                var canvas = thumb.Parent as Canvas;
                if (null != canvas)
                {
                    x = Math.Max(x, 0);
                    y = Math.Max(y, 0);
                    x = Math.Min(x, canvas.ActualWidth - thumb.ActualWidth);
                    y = Math.Min(y, canvas.ActualHeight - thumb.ActualHeight);
                }

                Canvas.SetLeft(thumb, x);
                Canvas.SetTop(thumb, y);
            }
        }

        private void MAPButton_Click(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "イメージファイル (*.png, *.jpg)|*.png;*.jpg";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(new Uri(dialog.FileName, UriKind.Relative));
                ib.Stretch = Stretch.Uniform;
                inkCanvas.Background = ib;
            }
        }

        private void ThumbRightDown(object sender, MouseButtonEventArgs e)
        {
            //inkCanvas.EditingMode = InkCanvasEditingMode.None;
            var Thumb = sender as Thumb;
            Point thumbPoint = Thumb.TranslatePoint(new Point(0, 0), Peace);
            Point mousePoint = Mouse.GetPosition(Peace);
            lineName = Thumb.Name +"Line";
            Line line = Peace.FindName(lineName) as Line;
            if ( line != null)
            {
                line.X1 = thumbPoint.X + 14;
                line.Y1 = thumbPoint.Y + 14;
            }
            
            if(IsDraw == true)
            {
                IsDraw = false;
                line.X2 = mousePoint.X;
                line.Y2 = mousePoint.Y;
            }
            else
            {
                IsDraw = true;
            }
            
        }

        private void peaceMouseMove(object sender, MouseEventArgs e)
        {
            Line line = Peace.FindName(lineName) as Line;
            if (IsDraw == true)
            {
                Point mousePoint = Mouse.GetPosition(Peace);
                line.X2 = mousePoint.X;
                line.Y2 = mousePoint.Y;
                line.Visibility = Visibility.Visible;
            }
        }

        private void peaceMouseRightUp(object sender, MouseButtonEventArgs e)
        {
            Line line = Peace.FindName(lineName) as Line;
            Point mousePoint = Mouse.GetPosition(Peace);
            if (IsDraw == true)
            {
                IsDraw = false;
                line.X2 = mousePoint.X;
                line.Y2 = mousePoint.Y;
            }
        }
    }
    }
