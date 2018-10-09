using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmRmc100Str
{
	[KrangSettings("DmRmc100Str", typeof(DmRmc100StrAdapter))]
	public sealed class DmRmc100StrAdapterSettings : AbstractDm100XStrBaseAdapterSettings
	{
		/// <summary>
		/// Returns true if the settings have been configured for receive.
		/// </summary>
		public override bool IsReceiver { get { return true; } }
	}
}
