using ICD.Connect.Routing.Extron.Controls;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.SwHd4K.Sw2Hd4K
{
	public sealed class Sw2Hd4KDevice : AbstractSwHd4KDevice<Sw2Hd4KSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Sw2Hd4KDevice()
		{
			Controls.Add(new ExtronSwitcherControl(this, 0, 2, 1, false));
		}
	}
}
