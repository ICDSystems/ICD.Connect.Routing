﻿using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcHd
{
	public sealed class DmcHdAdapterSettings : AbstractInputCardSettings
	{
		private const string FACTORY_NAME = "DmcHd";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmcHdAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static DmcHdAdapterSettings FromXml(string xml)
		{
			DmcHdAdapterSettings output = new DmcHdAdapterSettings();
			output.ParseXml(xml);
			return output;
		}
	}
}
