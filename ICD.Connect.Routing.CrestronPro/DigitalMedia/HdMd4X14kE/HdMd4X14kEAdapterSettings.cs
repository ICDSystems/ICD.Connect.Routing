using System;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMdNXM;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd4X14kE
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class HdMd4X14kEAdapterSettings : AbstractHdMdNXMAdapterSettings
	{
		private const string FACTORY_NAME = "HdMd4X14kE";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(HdMd4X14kEAdapter); } }
	}
}
