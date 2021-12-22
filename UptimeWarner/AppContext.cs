using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UptimeWarner.Properties;

namespace UptimeWarner
{
    public class AppContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Timer _timer;
        private readonly Timer _iconCycleTimer;
        private readonly TimeSpan _warningTime = TimeSpan.FromDays(1);
        private readonly TimeSpan _criticalTime = TimeSpan.FromDays(2);
        private bool _isRedIcon;
        public AppContext()
        {
            Debug.Assert(_warningTime <= _criticalTime);
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

            _iconCycleTimer = new Timer
            {
                Callback = (_) =>
                {
                    _notifyIcon.Icon = _isRedIcon ? Resources.GrayIcon : Resources.RedIcon;
                    _isRedIcon = !_isRedIcon;
                },
                DeltaTime = TimeSpan.FromSeconds(1)
            };
            
            _timer = new Timer
            {
                Callback = (sender) =>
                {
                    var uptime = GetUptime();
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (isDebug) Debug.WriteLine(TimeSpanToString(uptime));
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (isDebug)
                    {
                        _notifyIcon.ContextMenu.MenuItems[0]
                            .Text = string.Format(Resources.uptimeText,
                            TimeSpanToString(uptime));
                    }

                    _notifyIcon.Icon = IsWarning(uptime) ? Resources.RedIcon : Resources.GreenIcon;
                    _notifyIcon.Text = IsWarning(uptime) ? Resources.restartRequired : string.Empty;

                    if (!IsCritical(uptime)) return;
                    sender.Destroy();
                    _iconCycleTimer.Start();
                },
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                DeltaTime = isDebug ? TimeSpan.FromSeconds(1) : TimeSpan.FromHours(1)
            };
            _timer.Start();
        }

        // ReSharper disable once UnusedMember.Local
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void AppExit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            _timer.Destroy();
            _timer.Dispose();
            _iconCycleTimer.Destroy();
            _iconCycleTimer.Destroy();
            Application.Exit();
        }

        private bool IsWarning(TimeSpan uptime)
        {
            return uptime > _warningTime;
        }

        private bool IsCritical(TimeSpan uptime)
        {
            return uptime > _criticalTime;
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