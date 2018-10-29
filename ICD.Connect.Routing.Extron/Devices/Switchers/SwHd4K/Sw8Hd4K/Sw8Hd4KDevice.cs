using ICD.Connect.Routing.Extron.Controls;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.SwHd4K.Sw8Hd4K
{
	public sealed class Sw8Hd4KDevice : AbstractSwHd4KDevice<Sw8Hd4KSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Sw8Hd4KDevice()
		{
			Controls.Add(new ExtronSwitcherControl(this, 0, 8, 1, false));
		}
	}
}
