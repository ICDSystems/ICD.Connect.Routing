using ICD.Common.Properties;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.SPlus
{
	public sealed class SPlusSwitcher : AbstractDevice<SPlusSwitcherSettings>
	{
		private bool m_OnlineStatus;

		public SPlusSwitcher()
		{
			Controls.Add(new SPlusSwitcherControl(this, 0));
		}

		protected override bool GetIsOnlineStatus()
		{
			return m_OnlineStatus;
		}

		[PublicAPI("Splus")]
		public void SetOnlineStatus(bool online)
		{
			if (online == m_OnlineStatus)
				return;

			m_OnlineStatus = online;

			UpdateCachedOnlineStatus();
		}
	}
}
