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
            raw.Do(p => Debug.WriteLine("x: {0}, y: {1}, z: {2}", p.x, p.y, p.z));

            // カーソル移動
            raw.Select(p => new
                {
                    X = Settings.Default.Width / 2 + p.x * Settings.Default.Scale,
                    Y = Settings.Default.Height - p.y * Settings.Default.Scale,
                })
                .Do(a => Debug.WriteLine("X: {0}, Y: {1}", a.X, a.Y))
                .Select(a => new { X = Round(a.X), Y = Round(a.Y) })
                .Do(a => Debug.WriteLine("RoundX: {0}, RoundY: {1}", a.X, a.Y))
                .DistinctUntilChanged()
                .Do(_ => Debug.WriteLine("Set cursor position."))
                .Subscribe(a =>
                {
                    SetCursorPos(a.X, a.Y);
                });

            // TODO: クリック

            Console.ReadKey();
        }

        static int Round(float floatValue)
        {
            var n = Settings.Default.RoundCoefficient;
            return ((int)floatValue / n) * n;
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
    }
}
