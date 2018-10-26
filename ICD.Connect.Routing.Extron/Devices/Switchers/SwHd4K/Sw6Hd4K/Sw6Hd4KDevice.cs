using ICD.Connect.Routing.Extron.Controls;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.SwHd4K.Sw6Hd4K
{
	public sealed class Sw6Hd4KDevice : AbstractSwHd4KDevice<Sw6Hd4KSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Sw6Hd4KDevice()
		{
			Controls.Add(new ExtronSwitcherControl(this, 0, 6, 1));
		}
	}
}
