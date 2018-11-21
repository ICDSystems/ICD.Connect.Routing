using ICD.Connect.Devices.Simpl;

namespace ICD.Connect.Routing.SPlus
{
	public sealed class SPlusSwitcher : AbstractSimplDevice<SPlusSwitcherSettings>
	{
		public SPlusSwitcher()
		{
			Controls.Add(new SPlusSwitcherControl(this, 0));
		}
	}
}
