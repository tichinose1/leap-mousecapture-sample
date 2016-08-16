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
                .Do(p => Debug.WriteLine("x: {0}, y: {1}, z: {2}", p.x, p.y, p.z))
                .Subscribe(p =>
                {
                    var x = Settings.Default.Width / 2 + p.x * Settings.Default.ScaleX;
                    var y = Settings.Default.Height - p.y * Settings.Default.ScaleY;

                    SetCursorPos((int)x, (int)y);
                });

            Console.ReadKey();
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
    }
}
