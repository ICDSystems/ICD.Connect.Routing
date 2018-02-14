using System;
using Crestron.SimplSharpPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx35X;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx350
{
#if SIMPLSHARP
	public sealed class DmNvx350Adapter :
		AbstractDmNvx35XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvx350, DmNvx350AdapterSettings>
#else
	public sealed class DmNvx350Adapter : AbstractDmNvx35XAdapter<DmNvx350AdapterSettings>
#endif
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmNvx350 InstantiateSwitcher(uint ethernetId,
		                                                                                    CrestronControlSystem
			                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx350(ethernetId, controlSystem);
		}
	}

	[KrangSettings(FACTORY_NAME)]
	public sealed class DmNvx350AdapterSettings : AbstractDmNvx35XAdapterSettings
	{
		private const string FACTORY_NAME = "DmNvx350";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmNvx350Adapter); } }
	}
}
