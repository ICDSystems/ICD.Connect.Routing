using System;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8XN;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd8X14kC
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class DmMd8X14kCAdapterSettings : AbstractHdMd8XNAdapterSettings
	{
		private const string FACTORY_NAME = "DmMd8X14kC";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmMd8X14kCAdapter); } }
	}
}
