using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerCDsp
{
	/// <summary>
	/// Settings for the DmRmc4kScalerCDspAdapter.
	/// </summary>
// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kScalerCDspAdapterSettings : AbstractDmRmcScalerCAdapterSettings
	{
		private const string FACTORY_NAME = "DmRmc4kScalerCDsp";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.C:\Workspace\ICD\ICD.MetLife.RoomOS\ICD.Connect.Routing\ICD.Connect.Routing.CrestronPro\DigitalMedia\
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmRmc4kScalerCDspAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static DmRmc4kScalerCDspAdapterSettings FromXml(string xml)
		{
			DmRmc4kScalerCDspAdapterSettings output = new DmRmc4kScalerCDspAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
