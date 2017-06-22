using ICD.Connect.Devices;

namespace ICD.Connect.Routing.SPlus
{
	public sealed class SPlusSwitcher : AbstractDevice<SPlusSwitcherSettings>
	{
		public SPlusSwitcher()
		{
			Controls.Add(new SPlusSwitcherControl(this, 0));
		}

		protected override bool GetIsOnlineStatus()
		{
			return true;
		}
	}
}
