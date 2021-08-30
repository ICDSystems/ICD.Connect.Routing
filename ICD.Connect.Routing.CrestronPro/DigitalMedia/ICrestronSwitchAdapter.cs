#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Common.Properties;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
#if !NETSTANDARD
	public delegate void DmSwitcherChangeCallback(ICrestronSwitchAdapter sender, Switch switcher);
#endif

	public interface ICrestronSwitchAdapter : IDevice, IDmParent
	{
#if !NETSTANDARD
		event DmSwitcherChangeCallback OnSwitcherChanged;

		/// <summary>
		/// Gets the wrapped switch instance.
		/// </summary>
		[CanBeNull]
		Switch Switcher { get; }
#endif
	}
}
