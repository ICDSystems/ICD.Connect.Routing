using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerC
{
	/// <summary>
	/// Settings for the DmRmcScalerCAdapter.
	/// </summary>
	[KrangSettings("DmRmcScalerC", typeof(DmRmcScalerCAdapter))]
	public sealed class DmRmcScalerCAdapterSettings : AbstractDmRmcScalerCBaseAdapterSettings
	{
	}
}
