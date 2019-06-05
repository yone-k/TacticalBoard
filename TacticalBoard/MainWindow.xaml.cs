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

        //直線書いてる最中かどうかの判定
        bool IsDraw;

        //現在選択中のコマの直線の名前
        String lineName;

        //コマのリスト
        List<Thumb> thumbs = new List<Thumb>();

        //コマの初期配置座標リスト
        private PointCollection thumbDefaultPoints = new PointCollection();

        //スタンプ押してる最中判定用
        bool IsStamp;

        //スタンプの判定用(0=frag,1=smoke,2=stan,3=other)
        int StampType;

        //グレの画像リスト
        List<Image> fragImages = new List<Image>();

        //グレの画像リストのIndex
        int fragIndex = 0;

        //煙の画像リスト
        List<Image> smokeImages = new List<Image>();

        //煙リストのIndex
        int smokeIndex = 0;

        //スタンの画像リスト
        List<Image> stunImages = new List<Image>();

        //スタンリストのIndex
        int stunIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            
        }


        //Thumbドラッグ開始
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

        //Thumbドラッグ終了
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

        //Thumbドラッグ中
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
        //背景画像(MAP画像)をファイルから選択
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
        //コマから右ドラッグで直線を引くためのメソッド(右クリック押し込み動作)
        private void ThumbRightDown(object sender, MouseButtonEventArgs e)
        {
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            inkCanvas.EditingMode = InkCanvasEditingMode.None;
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
            
            if(IsDraw)
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

        //キャンバス上のマウス移動関係
        private void peaceMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = Mouse.GetPosition(Peace);
            //直線描画部分
            Line line = Peace.FindName(lineName) as Line;
            if (IsDraw)
            {

                line.X2 = mousePoint.X;
                line.Y2 = mousePoint.Y;
                line.Visibility = Visibility.Visible;
            }

            //Stamp押す部分
            if (IsStamp)
            {
                 switch (StampType)
                {
                    case 0:
                        
                        fragImages[fragIndex-1].Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);
                        break;

                    case 1:
                        smokeImages[smokeIndex - 1].Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);
                        break;

                    case 2:
                        stunImages[stunIndex - 1].Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);
                        break;
                }

            }
        }

        //thumbから右クリックを離した場合
        private void peaceMouseRightUp(object sender, MouseButtonEventArgs e)
        {
            Line line = Peace.FindName(lineName) as Line;
            Point mousePoint = Mouse.GetPosition(Peace);
            if (IsDraw)
            {
                IsDraw = false;
                line.X2 = mousePoint.X;
                line.Y2 = mousePoint.Y;
            }
        }

        //リセットボタンの動作
        private void resetButton(object sender, RoutedEventArgs e)
        {
            Line line;
            int i;

            //ボタン周りのリセット
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            inkCanvas.EditingMode = InkCanvasEditingMode.None;

            //インクのリセット
            for (i = 0; i < 10; i++)
            {
                Canvas.SetLeft(thumbs[i], thumbDefaultPoints[i].X);
                Canvas.SetTop(thumbs[i], thumbDefaultPoints[i].Y);
                lineName = thumbs[i].Name + "Line";
                line = Peace.FindName(lineName) as Line;
                line.Visibility = Visibility.Collapsed;
                inkCanvas.Strokes.Clear();

            }

            //グレスタンプのリセット
            for(;fragIndex>0; fragIndex--)
            {
                fragImages[fragIndex-1].Visibility = Visibility.Collapsed;
            }
            fragImages.Clear();

            //スモークスタンプのリセット
            for (; smokeIndex > 0; smokeIndex--)
            {
                smokeImages[smokeIndex - 1].Visibility = Visibility.Collapsed;
            }
            smokeImages.Clear();

            //スタンスタンプのリセット
            for (; stunIndex > 0; stunIndex--)
            {
                stunImages[stunIndex - 1].Visibility = Visibility.Collapsed;
            }
            stunImages.Clear();
        }

        //Thumbをロードしたらリストに入れて管理しやすくする
        private void thumbLoaded(object sender, RoutedEventArgs e)
        {
            var thumb = sender as Thumb;
            thumbs.Add((Thumb)sender);
            thumbDefaultPoints.Add(thumb.TranslatePoint(new Point(0, 0), Peace));
        }

        //ペンモードの切り替え
        private void PenButton_Checked(object sender, RoutedEventArgs e)
        {
            if(PenButton == null)
            {
                return;
            }

            if(PenButton.IsChecked == true)
            {
                EraseButton.IsChecked = false;
                inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }
            else
            {
                EraseButton.IsChecked = false;
                inkCanvas.EditingMode = InkCanvasEditingMode.None;
            }

        }

        //消しゴムモードの切替
        private void EraseChecked(object sender, RoutedEventArgs e)
        {
            if (EraseButton == null)
            {
                return;
            }

            if (EraseButton.IsChecked == true)
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
            }
            else
            {
                if (PenButton.IsChecked == true)
                {
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                }
                else
                {
                    inkCanvas.EditingMode = InkCanvasEditingMode.None;
                }
            }
        }



        //画面上でのクリック時の動作
        private void peaceMouseClick(object sender, MouseButtonEventArgs e)
        {
            //スタンプ押す状態のとき
            if (IsStamp)
            {
                IsStamp = false;

            }
        }

        //グレスタンプボタン
        private void fragButtonClick(object sender, RoutedEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.None;
            BitmapImage image = new BitmapImage(new Uri("Resources/frag.png", UriKind.Relative));
            Image frag = new Image();
            frag.Source = image;
            frag.Width = 50;
            IsStamp = true;
            fragIndex++;
            StampType = 0;
            Point mousePoint = Mouse.GetPosition(inkCanvas);
            frag.Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);
            inkCanvas.Children.Add(frag);
            fragImages.Add(frag);
        }

        //スモークスタンプボタン
        private void smokeButtonClick(object sender, RoutedEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.None;
            BitmapImage image = new BitmapImage(new Uri("Resources/smoke.png", UriKind.Relative));
            Image smoke = new Image();
            smoke.Source = image;
            smoke.Width = 50;
            IsStamp = true;
            smokeIndex ++;
            StampType = 1;
            Point mousePoint = Mouse.GetPosition(inkCanvas);
            smoke.Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);
            inkCanvas.Children.Add(smoke);
            smokeImages.Add(smoke);
        }

        //スタンスタンプボタン
        private void stunButtonClick(object sender, RoutedEventArgs e)
        {
            inkCanvas.EditingMode = InkCanvasEditingMode.None;
            BitmapImage image = new BitmapImage(new Uri("Resources/stun.png", UriKind.Relative));
            Image stun = new Image();
            stun.Source = image;
            stun.Width = 50;
            IsStamp = true;
            stunIndex++;
            StampType = 2;
            Point mousePoint = Mouse.GetPosition(inkCanvas);
            stun.Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);
            inkCanvas.Children.Add(stun);
            stunImages.Add(stun);
        }
    }
    }
