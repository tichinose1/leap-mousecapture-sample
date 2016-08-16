using Leap;
using LeapCursorWpf.Properties;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LeapCursorWpf
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller controller = null;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;

            // Controllerの初期化
            controller = new Controller();
            controller.SetPolicyFlags(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);

            // 生データ
            var raw = Observable.Interval(TimeSpan.FromSeconds(1.0 / Settings.Default.FPS))
                .Select(_ => controller.Frame().Fingers
                    .Where(f => f.Type == Finger.FingerType.TYPE_INDEX)
                    .FirstOrDefault())
                .Where(f => f != null)
                //.Do(f => Debug.WriteLine("x: {0}, y: {1}, z: {2}", f.TipPosition.x, f.TipPosition.y, f.TipPosition.z))
                .Select(f => f.TipPosition);

            // カーソル移動
            // 座標変換後、指定の数値の倍数に丸める
            raw
                .Select(p => new
                {
                    X = Settings.Default.ScreenX / 2 + Settings.Default.Scale * p.x,
                    Y = Settings.Default.ScreenY + Settings.Default.SupplementY - Settings.Default.Scale * p.y,
                })
                .Select(a => new { X = Round(a.X), Y = Round(a.Y) })
                .DistinctUntilChanged()
                .Subscribe(a =>
                {
                    PInvoke.PerformMoveCursor(a.X, a.Y);
                });

            // 長押し
            // 一定期間内に指先が常に指定範囲内にいるかどうかを監視
            // クリック後は手前に引く必要あり
            raw
                .Select(p => p.z < Settings.Default.RangeInZ)
                //.Do(b => Debug.WriteLine("b: {0}", b))
                .Buffer(TimeSpan.FromSeconds(Settings.Default.LongClickTime))
                .Select(bs => bs.All(b => b))
                .DistinctUntilChanged()
                .Where(b => b)
                .Subscribe(_ =>
                {
                    PInvoke.PerformClick();
                });

            // カーソル画像変更
            raw
                .Select(p => p.z < Settings.Default.RangeInZ)
                .DistinctUntilChanged()
                .Where(b => b)
                .Subscribe(_ =>
                {
                    SetCursor();
                });
        }

        void SetCursor()
        {
            Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
            });
        }

        static int Round(double originalValue)
        {
            var n = Settings.Default.RoundCoefficient;
            return ((int)originalValue / n) * n;
        }
    }
}
