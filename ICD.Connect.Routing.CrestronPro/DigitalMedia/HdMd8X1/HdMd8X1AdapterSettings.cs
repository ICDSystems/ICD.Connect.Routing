using System;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8XN;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8X1
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class HdMd8X1AdapterSettings : AbstractHdMd8XNAdapterSettings
	{
		private const string FACTORY_NAME = "HdMd8X1";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(HdMd8X1Adapter); } }
	}
}