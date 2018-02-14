using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx35X;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx351
{
#if SIMPLSHARP
	public sealed class DmNvx351Adapter :
		AbstractDmNvx35XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvx351, DmNvx351AdapterSettings>
#else
	public sealed class DmNvx351Adapter : AbstractDmNvx35XAdapter<DmNvx351AdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmNvx351 InstantiateSwitcher(uint ethernetId,
		                                                                                    CrestronControlSystem
			                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx351(ethernetId, controlSystem);
		}
#endif
	}

	[KrangSettings(FACTORY_NAME)]
	public sealed class DmNvx351AdapterSettings : AbstractDmNvx35XAdapterSettings
	{
		private const string FACTORY_NAME = "DmNvx351";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmNvx351Adapter); } }
	}
}
