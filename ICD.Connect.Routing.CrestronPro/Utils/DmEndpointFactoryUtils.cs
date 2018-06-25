using ICD.Connect.Devices.Extensions;
using ICD.Connect.Routing.CrestronPro.Receivers;
using ICD.Connect.Routing.CrestronPro.Transmitters;
using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
#endif
using ICD.Common.Properties;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Utils
{
	/// <summary>
	/// Provides a centralized collection of tools for instantiating DM devices.
	/// </summary>
	public static class DmEndpointFactoryUtils
	{
#if SIMPLSHARP
		/// <summary>
		/// Determines the best way to instantiate a card based on the available information.
		/// Instantiates via parent Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <typeparam name="TCard"></typeparam>
		/// <param name="cresnetId"></param>
		/// <param name="cardNumber"></param>
		/// <param name="switcherId"></param>
		/// <param name="factory"></param>
		/// <param name="instantiateExternal"></param>
		/// <param name="instantiateInternal"></param>
		/// <returns></returns>
		[NotNull]
		public static TCard InstantiateCard<TCard>(byte? cresnetId, int? cardNumber, int? switcherId,
		                                           IDeviceFactory factory,
		                                           Func<byte, CrestronControlSystem, TCard> instantiateExternal,
		                                           Func<uint, Switch, TCard> instantiateInternal)
		{
			if (switcherId == null)
			{
				if (cresnetId == null)
					throw new ArgumentNullException("cresnetId", "Can't instantiate external card without Cresnet ID");
				return instantiateExternal((byte)cresnetId, ProgramInfo.ControlSystem);
			}

			if (cardNumber == null)
				throw new ArgumentNullException("cardNumber", "Can't instantiate internal card without card number");

			ICrestronSwitchAdapter switcher = factory.GetOriginatorById<ICrestronSwitchAdapter>((int)switcherId);
			return instantiateInternal((uint)cardNumber, switcher.Switcher);
		}

		/// <summary>
		/// Determines the best way to instantiate a card based on the available information.
		/// Instantiates via parent Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <typeparam name="TCard"></typeparam>
		/// <param name="cardNumber"></param>
		/// <param name="switcherId"></param>
		/// <param name="factory"></param>
		/// <param name="instantiateInternal"></param>
		/// <returns></returns>
		[NotNull]
		public static TCard InstantiateCard<TCard>(int? cardNumber, int? switcherId,
		                                           IDeviceFactory factory,
		                                           Func<uint, Switch, TCard> instantiateInternal)
		{
			return InstantiateCard(null, cardNumber, switcherId, factory, null, instantiateInternal);
		}

		/// <summary>
		/// Determines the best way to instantiate the endpoint based on the available information.
		/// Instantiates via parent DM Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="adapter"></param>
		/// <returns></returns>
		[NotNull]
		public static TTransmitter InstantiateTransmitter<TTransmitter>(IEndpointTransmitterBaseAdapterSettings settings,
		                                                                IDeviceFactory factory,
		                                                                IEndpointTransmitterBaseAdapter<TTransmitter> adapter)
			where TTransmitter : EndpointTransmitterBase
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			if (factory == null)
				throw new ArgumentNullException("factory");

			if (adapter == null)
				throw new ArgumentNullException("adapter");

			if (settings.DmSwitch == null)
			{
				if (settings.Ipid == null)
					throw new InvalidOperationException("Can't instantiate ControlSystem endpoint without IPID");
				return adapter.InstantiateTransmitter((byte)settings.Ipid, ProgramInfo.ControlSystem);
			}

			if (settings.DmInputAddress == null)
				throw new InvalidOperationException("Can't instantiate DM endpoint without DM address");

			IDmParent provider = factory.GetDeviceById((int)settings.DmSwitch) as IDmParent;
			if (provider == null)
				throw new InvalidOperationException(string.Format("Device {0} is not a {1}", settings.DmSwitch,
				                                                  typeof(IDmParent).Name));

			DMInput input = provider.GetDmInput((int)settings.DmInputAddress);

			return settings.Ipid == null
				       ? adapter.InstantiateTransmitter(input)
				       : adapter.InstantiateTransmitter((byte)settings.Ipid, input);
		}

		/// <summary>
		/// Determines the best way to instantiate a DMEndpoint based on the available information.
		/// Instantiates via parent DM Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="adapter"></param>
		/// <returns></returns>
		[NotNull]
		public static TReceiver InstantiateReceiver<TReceiver>(IEndpointReceiverBaseAdapterSettings settings,
		                                                       IDeviceFactory factory,
		                                                       IEndpointReceiverBaseAdapter<TReceiver> adapter)
			where TReceiver : EndpointReceiverBase
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			if (factory == null)
				throw new ArgumentNullException("factory");

			if (adapter == null)
				throw new ArgumentNullException("adapter");

			if (settings.DmSwitch == null)
			{
				if (settings.Ipid == null)
					throw new InvalidOperationException("Can't instantiate ControlSystem endpoint without IPID");
				return adapter.InstantiateReceiver((byte)settings.Ipid, ProgramInfo.ControlSystem);
			}

			if (settings.DmOutputAddress == null)
				throw new InvalidOperationException("Can't instantiate DM endpoint without DM address");

			IDmParent provider = factory.GetDeviceById((int)settings.DmSwitch) as IDmParent;
			if (provider == null)
				throw new InvalidOperationException(string.Format("Device {0} is not a {1}", settings.DmSwitch,
				                                                  typeof(IDmParent).Name));

			DMOutput output = provider.GetDmOutput((int)settings.DmOutputAddress);

			return settings.Ipid == null
				       ? adapter.InstantiateReceiver(output)
				       : adapter.InstantiateReceiver((byte)settings.Ipid, output);
		}
#endif
	}
}
