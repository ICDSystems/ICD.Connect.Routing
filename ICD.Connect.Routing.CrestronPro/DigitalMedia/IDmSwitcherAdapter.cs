using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
	public delegate void DmSwitcherChangeCallback(IDmSwitcherAdapter sender, Switch switcher);

	public interface IDmSwitcherAdapter : IDevice, IDmParent
	{
		event DmSwitcherChangeCallback OnSwitcherChanged;

		/// <summary>
		/// Gets the wrapped switch instance.
		/// </summary>
		[CanBeNull]
		Switch Switcher { get; }
	}
}
