using ICD.Connect.Routing.CrestronPro.Receivers.AbstractDmRmc4kScalerC;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerC
{
	/// <summary>
	/// Settings for the DmRmcScalerCAdapter.
	/// </summary>
	[KrangSettings("DmRmc4kScalerC", typeof(DmRmc4kScalerCAdapter))]
// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kScalerCAdapterSettings : AbstractDmRmc4KScalerCAdapterSettings
	{
	}
}
