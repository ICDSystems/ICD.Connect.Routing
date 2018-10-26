using ICD.Connect.Routing.Extron.Controls;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.SwHd4K.Sw4Hd4K
{
	public sealed class Sw4Hd4KDevice : AbstractSwHd4KDevice<Sw4Hd4KSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Sw4Hd4KDevice()
		{
			Controls.Add(new ExtronSwitcherControl(this, 0, 4, 1, false));
		}
	}
}
