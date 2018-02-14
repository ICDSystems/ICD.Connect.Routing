using System;
using Crestron.SimplSharpPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmRmc100Str
{
#if SIMPLSHARP
	public sealed class DmRmc100StrAdapter :
		AbstractDm100XStrBaseAdapter<Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str, DmRmc100StrAdapterSettings>
#else
	public sealed class DmRmc100StrAdapter : AbstractDm100XStrBaseAdapter<DmRmc100StrAdapterSettings>
#endif
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str InstantiateSwitcher(uint ethernetId,
		                                                                                       CrestronControlSystem
			                                                                                       controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str(ethernetId, controlSystem);
		}
	}

	[KrangSettings(FACTORY_NAME)]
	public sealed class DmRmc100StrAdapterSettings : AbstractDm100XStrBaseAdapterSettings
	{
		private const string FACTORY_NAME = "DmRmc100Str";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmRmc100StrAdapter); } }
	}
}
