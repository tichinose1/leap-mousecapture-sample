using Leap;
using LeapMouseCursorConsole.Properties;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace LeapMouseCursorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            OperateCursorByLeap();

            Console.ReadKey();
        }

        private static void OperateCursorByLeap()
        {
            // 画面サイズ（ピクセル）
            var primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
            var primaryScreenHeight = SystemParameters.PrimaryScreenHeight;

            // Controllerの初期化
            var controller = new Controller();
            controller.SetPolicyFlags(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);

            // 生データ（人差し指の座標）
            var raw = Observable.Interval(TimeSpan.FromSeconds(1.0 / Settings.Default.FPS))
                .Select(_ => controller.Frame().Fingers
                    .Where(f => f.Type == Finger.FingerType.TYPE_INDEX)
                    .FirstOrDefault())
                .Where(f => f != null)
                .Select(f => f.TipPosition);
            raw.Subscribe(p => Debug.WriteLine("x: {0}, y: {1}, z: {2}", p.x, p.y, p.z));

            // カーソル移動
            // 座標変換後、指定の数値の倍数に丸める
            raw
                .Select(p => new
                {
                    X = primaryScreenWidth / 2 + Settings.Default.Scale * p.x,
                    Y = primaryScreenHeight - Settings.Default.Scale * p.y + Settings.Default.CorrectionY,
                })
                .Select(a => new
                {
                    X = Round(a.X),
                    Y = Round(a.Y),
                })
                .DistinctUntilChanged()
                .Subscribe(a =>
                {
                    PInvoke.PerformMoveCursor(a.X, a.Y);
                });

            // 長押しによるクリック
            // 一定期間内に指先が常に指定範囲内にいるかどうかを監視
            // クリック後は手前に引く必要あり
            raw
                .Select(p => p.z < Settings.Default.RangeInZ)
                .Buffer(TimeSpan.FromSeconds(Settings.Default.ClickThreshold))
                .Select(bs => bs.All(b => b))
                .DistinctUntilChanged()
                .Where(b => b)
                .Subscribe(_ =>
                {
                    PInvoke.PerformClick();
                });

            // クリック
            // Hit範囲を定義し、In/Outの切り替わるタイミングを監視
            // Inは厳しく、Outは余裕を持たせて違う値にする
            // 精度悪し
            //raw
            //    .Scan(new { IsHit = false, Vector = new Vector() }, (previous, current) =>
            //    {
            //        return previous.IsHit
            //            ? new { IsHit = current.z < Settings.Default.RangeOutZ, Vector = current }
            //            : new { IsHit = current.z < Settings.Default.RangeInZ, Vector = current };
            //    })
            //    //.Do(a => Debug.WriteLine("a: {0}", a))
            //    .DistinctUntilChanged(a => a.IsHit)
            //    .Where(a => !a.IsHit)
            //    .Subscribe(_ =>
            //    {
            //        PInvoke.PerformClick();
            //    });
        }

        static int Round(double originalValue)
        {
            var n = Settings.Default.RoundCoefficient;
            return ((int)originalValue / n) * n;
        }
    }
}
