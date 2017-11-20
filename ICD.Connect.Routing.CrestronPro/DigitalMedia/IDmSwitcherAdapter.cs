#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Common.Properties;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
#if SIMPLSHARP
	public delegate void DmSwitcherChangeCallback(IDmSwitcherAdapter sender, Switch switcher);
#endif

	public interface IDmSwitcherAdapter : IDevice, IDmParent
    {
#if SIMPLSHARP
		event DmSwitcherChangeCallback OnSwitcherChanged;

		/// <summary>
		/// Gets the wrapped switch instance.
		/// </summary>
		[CanBeNull]
		Switch Switcher { get; }
#endif
	}
}
