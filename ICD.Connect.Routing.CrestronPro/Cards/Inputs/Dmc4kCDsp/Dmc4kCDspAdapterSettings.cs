﻿using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCDsp
{
	public sealed class Dmc4kCDspAdapterSettings : AbstractInputCardSettings
	{
		private const string FACTORY_NAME = "Dmc4kCDsp";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Dmc4kCDspAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static Dmc4kCDspAdapterSettings FromXml(string xml)
		{
			Dmc4kCDspAdapterSettings output = new Dmc4kCDspAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
