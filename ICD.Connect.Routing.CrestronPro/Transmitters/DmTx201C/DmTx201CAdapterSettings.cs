﻿using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201C
{
	public sealed class DmTx201CAdapterSettings : AbstractDmTx200BaseAdapterSettings
	{
		private const string FACTORY_NAME = "DmTx201C";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Creates a new originator instance from the settings.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public override IOriginator ToOriginator(IDeviceFactory factory)
		{
			DmTx201CAdapter output = new DmTx201CAdapter();
			output.ApplySettings(this, factory);
			return output;
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmTx201CAdapterSettings FromXml(string xml)
		{
			DmTx201CAdapterSettings output = new DmTx201CAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
