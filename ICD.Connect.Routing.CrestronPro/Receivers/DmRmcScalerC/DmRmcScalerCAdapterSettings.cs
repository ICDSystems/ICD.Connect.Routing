using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerC
{
	/// <summary>
	/// Settings for the DmRmcScalerCAdapter.
	/// </summary>
	public sealed class DmRmcScalerCAdapterSettings : AbstractDmRmcScalerCAdapterSettings
	{
		private const string FACTORY_NAME = "DmRmcScalerC";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmRmcScalerCAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static DmRmcScalerCAdapterSettings FromXml(string xml)
		{
			DmRmcScalerCAdapterSettings output = new DmRmcScalerCAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
