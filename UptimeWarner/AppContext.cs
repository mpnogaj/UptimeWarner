using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TTimer = System.Threading.Timer;
using UptimeWarner.Properties;

namespace UptimeWarner
{
    public class AppContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly TTimer _timer;
        public AppContext()
        {
            // ReSharper disable once JoinDeclarationAndInitializer
            // ReSharper disable once RedundantAssignment
            var isDebug = false;
#if DEBUG
            isDebug = true;
#endif
            _notifyIcon = new NotifyIcon
            {
#if DEBUG
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                ContextMenu = isDebug ? new ContextMenu(new []
                {
                    new MenuItem(""),
                    new MenuItem(Resources.exit, AppExit)
                }) : null,
#endif
                Visible = true,
                Icon = Resources.Icon
            };
            _timer = new TTimer((_) =>
            {
                var uptime = GetUptime();
                Console.WriteLine(TimeSpanToString(uptime));
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (isDebug)
                {
                    _notifyIcon.ContextMenu.MenuItems[0]
                        .Text = string.Format(Resources.uptimeText,
                        TimeSpanToString(uptime));
                }
                _notifyIcon.Icon = ControlCheck(uptime) ? Resources.RedIcon : Resources.GreenIcon;
                _notifyIcon.Text = ControlCheck(uptime) ? Resources.restartRequired : string.Empty;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            }, new object(), 0, isDebug ? 1000 : 1000 * 60 * 60);
        }

        // ReSharper disable once UnusedMember.Local
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void AppExit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            _timer.Dispose();
            Application.Exit();
        }

        private static bool ControlCheck(TimeSpan uptime)
        {
	        return uptime > TimeSpan.FromDays(1);
	        //return uptime > TimeSpan.FromMilliseconds(1);
        }

        private static string TimeSpanToString(TimeSpan ts)
        {
	        return $"{(ts.Days * 24 + ts.Hours):D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        /// <summary>
        /// Gets system uptime from win32 api
        /// </summary>
        /// <returns>Gets time span since the system started.</returns>
        private TimeSpan GetUptime()
        {
            return TimeSpan.FromMilliseconds(GetTickCount64());
        }
        
        [DllImport("kernel32")]
        private static extern ulong GetTickCount64();
    }
}