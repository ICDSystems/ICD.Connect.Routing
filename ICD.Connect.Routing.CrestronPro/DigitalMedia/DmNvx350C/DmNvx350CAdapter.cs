using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx35X;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx350C
{
#if SIMPLSHARP
	public sealed class DmNvx350CAdapter :
		AbstractDmNvx35XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvx350C, DmNvx350CAdapterSettings>
#else
	public sealed class DmNvx350CAdapter : AbstractDmNvx35XAdapter<DmNvx350CAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmNvx350C InstantiateSwitcher(uint ethernetId,
		                                                                                    CrestronControlSystem
			                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx350C(ethernetId, controlSystem);
		}
#endif
	}

	[KrangSettings(FACTORY_NAME)]
	public sealed class DmNvx350CAdapterSettings : AbstractDmNvx35XAdapterSettings
	{
		private const string FACTORY_NAME = "DmNvx350C";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmNvx350CAdapter); } }
	}
}
