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
            var controller = new Controller();
            controller.SetPolicyFlags(Controller.PolicyFlag.POLICY_BACKGROUND_FRAMES);

            Observable.Interval(TimeSpan.FromSeconds(1.0 / Settings.Default.FPS))
                .Select(_ => controller.Frame().Fingers
                    .Where(f => f.Type == Finger.FingerType.TYPE_INDEX)
                    .SingleOrDefault())
                .Where(f => f != null)
                .Select(f => f.TipPosition)
                //.Do(p => Debug.WriteLine("x: {0}, y: {1}, z: {2}", p.x, p.y, p.z))
                .Select(p => new
                {
                    X = Settings.Default.Width / 2 + p.x * Settings.Default.ScaleX,
                    Y = Settings.Default.Height - p.y * Settings.Default.ScaleY,
                    Z = Settings.Default.ScaleZ
                })
                //.Do(a => Debug.WriteLine("X: {0}, Y: {1}, Z: {2}", a.X, a.Y, a.Z))
                .Select(a => new { X = Round(a.X), Y = Round(a.Y), Z = Round(a.Z) })
                .Do(a => Debug.WriteLine("RoundX: {0}, RoundY: {1}, RoundZ: {2}", a.X, a.Y, a.Z))
                .DistinctUntilChanged()
                .Subscribe(a =>
                {
                    Debug.WriteLine(DateTime.Now);

                    SetCursorPos(a.X, a.Y);
                });

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
