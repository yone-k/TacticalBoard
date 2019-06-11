using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        //コマのリストとレイヤー
        List<Thumb> thumbs = new List<Thumb>();
        String[] thumbsLayer = new string[10];

        //コマの初期配置座標リスト
        private PointCollection thumbDefaultPoints = new PointCollection();

        //スタンプ押してる最中判定用
        bool IsStamp;

        //スタンプのリスト
        List<Image> stampImages = new List<Image>();

        //スタンプリストのIndex
        int stampIndex = 0;

        //現在のレイヤーのインクキャンバス
        InkCanvas nowLayerInk;

        //現在のレイヤーのスタンプ用キャンバス
        Canvas nowLayerStamp;

        //現在のレイヤー
        String nowLayer = "Layer1";

        //現在のインク色
        Button nowInkButton;


        public MainWindow()
        {
            InitializeComponent();

            nowLayerInk = inkCanvasLayer1;
            nowLayerStamp = StampCanvasLayer1;
            nowInkButton = YellowInk;

            thumbsLayer = new string[]{
                "Layer1","Layer1","Layer1","Layer1","Layer1","Layer1","Layer1","Layer1","Layer1","Layer1"
            };

        }


        //Thumbドラッグ開始
        private void Thumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            var thumb = sender as Thumb;
            lineName = thumb.Name + "Line";
            int i;

            //直線を消す
            Line line = PeaceCanvas.FindName(lineName) as Line;
            line.Visibility = Visibility.Collapsed;

            //透過設定
            for (i = 0; i < 10; i++)
            {
                if (thumbs[i].Equals(thumb))
                {
                    thumbsLayer[i] = nowLayer;
                    thumbs[i].Opacity = 1;
                    line.Opacity = 1;
                }
            }
            
            //ドラッグ関連
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
                nowLayerInk.Background = ib;
            }
        }

        //コマから右ドラッグで直線を引くためのメソッド(右クリック押し込み動作)
        private void ThumbRightDown(object sender, MouseButtonEventArgs e)
        {
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            nowLayerInk.EditingMode = InkCanvasEditingMode.None;
            var Thumb = sender as Thumb;
            Point thumbPoint = Thumb.TranslatePoint(new Point(0, 0), PeaceCanvas);
            Point mousePoint = Mouse.GetPosition(PeaceCanvas);
            lineName = Thumb.Name + "Line";
            Line line = PeaceCanvas.FindName(lineName) as Line;
            if (line != null)
            {
                line.X1 = thumbPoint.X + 14;
                line.Y1 = thumbPoint.Y + 14;
            }

            if (IsDraw)
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
            Point mousePoint = Mouse.GetPosition(PeaceCanvas);
            //直線描画部分
            Line line = PeaceCanvas.FindName(lineName) as Line;
            if (IsDraw)
            {

                line.X2 = mousePoint.X;
                line.Y2 = mousePoint.Y;
                line.Visibility = Visibility.Visible;
            }

            //Stamp押す部分
            if (IsStamp)
            {
                stampImages[stampIndex - 1].Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);
            }
        }

        //thumbから右クリックを離した場合
        private void peaceMouseRightUp(object sender, MouseButtonEventArgs e)
        {
            Line line = PeaceCanvas.FindName(lineName) as Line;
            Point mousePoint = Mouse.GetPosition(PeaceCanvas);
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
            nowLayerInk.EditingMode = InkCanvasEditingMode.None;

            //コマのリセット
            for (i = 0; i < 10; i++)
            {
                Canvas.SetLeft(thumbs[i], thumbDefaultPoints[i].X);
                Canvas.SetTop(thumbs[i], thumbDefaultPoints[i].Y);
                lineName = thumbs[i].Name + "Line";
                line = PeaceCanvas.FindName(lineName) as Line;
                line.Visibility = Visibility.Collapsed;
                thumbsLayer[i] = nowLayer;
                thumbs[i].Opacity = 1;
                line.Opacity = 1;
            }

            //インクのリセット
            inkCanvasLayer1.Strokes.Clear();
            inkCanvasLayer2.Strokes.Clear();
            inkCanvasLayer3.Strokes.Clear();
            inkCanvasLayer4.Strokes.Clear();

            //スタンプのリセット
            for (; stampIndex > 0; stampIndex--)
            {
                stampImages[stampIndex - 1].Visibility = Visibility.Collapsed;
            }
            stampImages.Clear();
        }

        //Thumbをロードしたらリストに入れて管理しやすくする
        private void thumbLoaded(object sender, RoutedEventArgs e)
        {
            var thumb = sender as Thumb;
            thumbs.Add((Thumb)sender);
            thumbDefaultPoints.Add(thumb.TranslatePoint(new Point(0, 0), PeaceCanvas));
        }

        //ペンモードの切り替え
        private void PenButton_Checked(object sender, RoutedEventArgs e)
        {
            if (PenButton == null)
            {
                return;
            }

            if (PenButton.IsChecked == true)
            {
                EraseButton.IsChecked = false;
                nowLayerInk.EditingMode = InkCanvasEditingMode.Ink;
            }
            else
            {
                EraseButton.IsChecked = false;
                nowLayerInk.EditingMode = InkCanvasEditingMode.None;
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
                nowLayerInk.EditingMode = InkCanvasEditingMode.EraseByStroke;
            }
            else
            {
                if (PenButton.IsChecked == true)
                {
                    nowLayerInk.EditingMode = InkCanvasEditingMode.Ink;
                }
                else
                {
                    nowLayerInk.EditingMode = InkCanvasEditingMode.None;
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

        //スタンプ押下用メソッド
        private void PressStamp(String StampType)
        {
            Image stamp = new Image
            {
                Source = new BitmapImage(new Uri(StampType, UriKind.RelativeOrAbsolute)),
                Width = 50
            };
            IsStamp = true;
            stampIndex++;

            //右クリックで削除できるようにイベントハンドラを用意する
            stamp.MouseRightButtonDown += new MouseButtonEventHandler(StampClear);
            Point mousePoint = Mouse.GetPosition(nowLayerInk);

            //マウスカーソルの位置に画像をセットする
            stamp.Margin = new Thickness(mousePoint.X, mousePoint.Y, 0, 0);

            //スタンプを配置
            nowLayerStamp.Children.Add(stamp);
            stampImages.Add(stamp);
        }

        //グレスタンプボタン
        private void fragButtonClick(object sender, RoutedEventArgs e)
        {
            //ペン関係をオフにする
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            nowLayerInk.EditingMode = InkCanvasEditingMode.None;

            //スタンプ押下メソッドを呼び出す
            PressStamp("Resources/frag.png");
        }

        //スモークスタンプボタン
        private void smokeButtonClick(object sender, RoutedEventArgs e)
        {
            //ペン関係をオフにする
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            nowLayerInk.EditingMode = InkCanvasEditingMode.None;

            //スタンプ押下メソッドを呼び出す。
            PressStamp("Resources/smoke.png");
        }

        //スタンスタンプボタン
        private void stunButtonClick(object sender, RoutedEventArgs e)
        {
            //ペン関係をオフにする
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            nowLayerInk.EditingMode = InkCanvasEditingMode.None;

            //スタンプ押下メソッドを呼び出す。
            PressStamp("Resources/stun.png");
        }

        //その他スタンプボタン
        private void stampButtonClick(object sender, RoutedEventArgs e)
        {
            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "イメージファイル (*.png, *.jpg)|*.png;*.jpg";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                //ダイアログから選択された画像でスタンプを押下する。
                PressStamp(dialog.FileName);
            }
        }

        //レイヤー関連
        private void layerButtonClick(object sender, RoutedEventArgs e)
        {
            //現在のレイヤー関連を保持
            Button beforeLayerButton = FindName(nowLayer + "Button") as Button;
            InkCanvas beforeLayerInk = nowLayerInk;
            Canvas beforeLayerStamp = nowLayerStamp;

            //クリックされたレイヤー情報
            Button nowLayerButton = sender as Button;
            String clickedLayer = nowLayerButton.Content.ToString();
            nowLayer = clickedLayer;
            nowLayerInk = FindName("inkCanvas" + clickedLayer) as InkCanvas;
            nowLayerStamp = FindName("StampCanvas" + clickedLayer) as Canvas;

            //表示系の切り替え
            nowLayerInk.Visibility = Visibility.Visible;
            nowLayerStamp.Visibility = Visibility.Visible;
            beforeLayerInk.Visibility = Visibility.Collapsed;
            beforeLayerInk.EditingMode = InkCanvasEditingMode.None;
            PenButton.IsChecked = false;
            EraseButton.IsChecked = false;
            beforeLayerStamp.Visibility = Visibility.Collapsed;
            nowLayerButton.IsEnabled = false;
            beforeLayerButton.IsEnabled = true;

            //コマの透過関連
            int i;
            Line line;
            for (i = 0; i < 10; i++)
            {
                line = FindName(thumbs[i].Name + "Line") as Line;
                if (thumbsLayer[i].Equals(nowLayer))
                {
                    thumbs[i].Opacity = 1;
                    line.Opacity = 1;
                }
                else
                {
                    thumbs[i].Opacity = 0.4;
                    line.Opacity = 0.4;
                }
            }
        }

        //スタンプを右クリックしたときの動作(右クリックしたスタンプを消す)
        private void StampClear(object sender, RoutedEventArgs e)
        {
            var Stamp = sender as Image;
            Stamp.Visibility = Visibility.Collapsed;
        }

        //インクのカラー設定
        private void ColorButton(object sender, RoutedEventArgs e)
        {
            //現在のインクと入れ替えたりする
            Button beforeInkButton = nowInkButton;
            nowInkButton = sender as Button;
            beforeInkButton.BorderBrush = new SolidColorBrush(Colors.LightGray);
            nowInkButton.BorderBrush = new SolidColorBrush(Colors.Black);

            //ボタンの背景色をインク色にする
            SolidColorBrush colorBrush = nowInkButton.Background as SolidColorBrush;
            inkCanvasLayer1.DefaultDrawingAttributes.Color = colorBrush.Color;
            inkCanvasLayer2.DefaultDrawingAttributes.Color = colorBrush.Color;
            inkCanvasLayer3.DefaultDrawingAttributes.Color = colorBrush.Color;
            inkCanvasLayer4.DefaultDrawingAttributes.Color = colorBrush.Color;
        }
    }
}
