using ICD.Common.Properties;
using ICD.Connect.Devices.Simpl;

namespace ICD.Connect.Routing.SPlus
{
	public sealed class SPlusSwitcher : AbstractSimplDevice<SPlusSwitcherSettings>
	{
		private bool m_OnlineStatus;

		public SPlusSwitcher()
		{
			Controls.Add(new SPlusSwitcherControl(this, 0));
		}
	}
}
