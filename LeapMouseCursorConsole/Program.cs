using Leap;
using LeapMouseCursorConsole.Properties;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace LeapMouseCursorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Controllerの初期化
            var controller = new Controller();
            controller.SetPolicyFlags(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);

            // 生データ
            var raw = Observable.Interval(TimeSpan.FromSeconds(1.0 / Settings.Default.FPS))
                .Select(_ => controller.Frame().Fingers
                    .Where(f => f.Type == Finger.FingerType.TYPE_INDEX)
                    .SingleOrDefault())
                .Where(f => f != null)
                .Select(f => f.TipPosition);
            //raw.Subscribe(p => Debug.WriteLine("x: {0}, y: {1}, z: {2}", p.x, p.y, p.z));

            // カーソル移動
            raw.Select(p => new
                {
                    X = Settings.Default.ScreenX / 2 + Settings.Default.Scale * p.x,
                    Y = Settings.Default.ScreenY + Settings.Default.SupplementY - Settings.Default.Scale * p.y,
                })
                //.Do(a => Debug.WriteLine("X: {0}, Y: {1}", a.X, a.Y))
                .Select(a => new { X = Round(a.X), Y = Round(a.Y) })
                //.Do(a => Debug.WriteLine("RoundX: {0}, RoundY: {1}", a.X, a.Y))
                .DistinctUntilChanged()
                //.Do(_ => Debug.WriteLine("Set cursor position."))
                .Subscribe(a =>
                {
                    SetCursorPos(a.X, a.Y);
                });

            // クリック
            raw
                .Scan(new { IsHit = false, Vector = new Vector() }, (previous, current) =>
                {
                    return previous.IsHit
                        ? new { IsHit = current.z < Settings.Default.RangeOutZ, Vector = current }
                        : new { IsHit = current.z < Settings.Default.RangeInZ, Vector = current };
                })
                .Do(a => Debug.WriteLine("a: {0}", a))
                .DistinctUntilChanged(a => a.IsHit)
                .Where(a => !a.IsHit)
                .Subscribe(_ =>
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                });

            Console.ReadKey();
        }

        static int Round(double floatValue)
        {
            var n = Settings.Default.RoundCoefficient;
            return ((int)floatValue / n) * n;
        }

        const int MOUSEEVENTF_LEFTDOWN = 0x2;
        const int MOUSEEVENTF_LEFTUP = 0x4;

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("User32.dll")]
        static extern bool SetCursorPos(int X, int Y);
    }
}
