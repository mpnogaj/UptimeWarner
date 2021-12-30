using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using UptimeWarner.Properties;

namespace UptimeWarner
{
	public class AppContext : ApplicationContext
	{
		private readonly NotifyIcon _notifyIcon;
		private readonly Timer _timer;
		private readonly Timer _iconCycleTimer;
		private bool _isRedIcon;
		public AppContext()
		{
			// ReSharper disable once JoinDeclarationAndInitializer
			// ReSharper disable once RedundantAssignment
			// ReSharper disable once ConvertToConstant.Local
			var isDebug = false;
#if DEBUG
			isDebug = true;
#endif
			_notifyIcon = new NotifyIcon
			{
#if DEBUG
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				ContextMenu = isDebug ? new ContextMenu(new[]
				{
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


					_notifyIcon.Text = CreateMessage(uptime, IsWarning(uptime));

					if (IsCritical(uptime))
					{
						if(!_iconCycleTimer.IsRunning) _iconCycleTimer.Start();
					}
					else
					{
						_notifyIcon.Icon = IsWarning(uptime) ? Resources.RedIcon : Resources.GreenIcon;
					}

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

		private static bool IsWarning(TimeSpan uptime)
		{
			return uptime > ConfigManager.Current.WarningTimeSpan;
		}

		private static bool IsCritical(TimeSpan uptime)
		{
			return uptime > ConfigManager.Current.CriticalDeltaTimeSpan + ConfigManager.Current.WarningTimeSpan;
		}

		private static string TimeSpanToString(TimeSpan ts)
		{
			return $"{(ts.Days * 24 + ts.Hours):D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
		}

		private static string CreateMessage(TimeSpan uptime, bool addWarn)
		{
			var builder = new StringBuilder();
			builder.AppendLine(string.Format(Resources.programName, uptime.Days));
			if (addWarn) builder.AppendLine(Resources.restartRequired);
			return builder.ToString();
		}

		/// <summary>
		/// Gets system uptime from win32 api
		/// </summary>
		/// <returns>Gets time span since the system started.</returns>
		private static TimeSpan GetUptime() => TimeSpan.FromMilliseconds(GetTickCount64());

		[DllImport("kernel32")]
		private static extern ulong GetTickCount64();
	}
}