using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerC
{
	/// <summary>
	/// Settings for the DmRmcScalerCAdapter.
	/// </summary>
	public sealed class DmRmc4kScalerCAdapterSettings : AbstractDmRmcScalerCAdapterSettings
	{
		private const string FACTORY_NAME = "DmRmc4kScalerC";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmRmc4kScalerCAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmRmc4kScalerCAdapterSettings FromXml(string xml)
		{
			DmRmc4kScalerCAdapterSettings output = new DmRmc4kScalerCAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
