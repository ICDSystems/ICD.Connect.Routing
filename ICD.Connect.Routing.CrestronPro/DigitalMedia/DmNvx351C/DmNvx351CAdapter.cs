using System;
using Crestron.SimplSharpPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx35X;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx351C
{
#if SIMPLSHARP
	public sealed class DmNvx351CAdapter :
		AbstractDmNvx35XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvx351C, DmNvx351CAdapterSettings>
#else
	public sealed class DmNvx351CAdapter : AbstractDmNvx35XAdapter<DmNvx351CAdapterSettings>
#endif
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmNvx351C InstantiateSwitcher(uint ethernetId,
		                                                                                    CrestronControlSystem
			                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx351C(ethernetId, controlSystem);
		}
	}

	[KrangSettings(FACTORY_NAME)]
	public sealed class DmNvx351CAdapterSettings : AbstractDmNvx35XAdapterSettings
	{
		private const string FACTORY_NAME = "DmNvx351C";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmNvx351CAdapter); } }
	}
}
