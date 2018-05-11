using ICD.Connect.Settings;
#if SIMPLSHARP
using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;

namespace ICD.Connect.Routing.CrestronPro.Utils
{
	/// <summary>
	/// Provides a centralized collection of tools for instantiating DM devices.
	/// </summary>
	public static class DmEndpointFactoryUtils
	{
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
		/// Determines the best way to instantiate a DMEndpoint based on the available information.
		/// Instantiates via parent DM Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <typeparam name="TEndpoint"></typeparam>
		/// <param name="ipid"></param>
		/// <param name="dmAddress"></param>
		/// <param name="dmSwitchId"></param>
		/// <param name="factory"></param>
		/// <param name="instantiate1">Instantiate via control system with specified IPID</param>
		/// <param name="instantiate2">Instantiate via DMInput with specified IPID</param>
		/// <param name="instantiate3">Instantiate via DMInput</param>
		/// <returns></returns>
		[NotNull]
		public static TEndpoint InstantiateEndpoint<TEndpoint>(byte? ipid, int? dmAddress, int? dmSwitchId,
		                                                       IDeviceFactory factory,
		                                                       Func<byte, CrestronControlSystem, TEndpoint> instantiate1,
		                                                       Func<byte, DMInput, TEndpoint> instantiate2,
		                                                       Func<DMInput, TEndpoint> instantiate3)
		{
			if (dmSwitchId == null)
			{
				if (ipid == null)
					throw new ArgumentNullException("ipid", "Can't instantiate ControlSystem endpoint without IPID");
				return instantiate1((byte)ipid, ProgramInfo.ControlSystem);
			}

			if (dmAddress == null)
				throw new ArgumentNullException("dmAddress", "Can't instantiate DM endpoint without DM address");

			IDmParent provider = factory.GetDeviceById((int)dmSwitchId) as IDmParent;
			if (provider == null)
			{
				throw new ArgumentException(string.Format("Device {0} is not a {1}", dmSwitchId, typeof(IDmParent).Name),
				                            "dmSwitchId");
			}

			DMInput input = provider.GetDmInput((int)dmAddress);
			return ipid == null ? instantiate3(input) : instantiate2((byte)ipid, input);
		}

		/// <summary>
		/// Determines the best way to instantiate a DMEndpoint based on the available information.
		/// Instantiates via parent DM Switch if specified, otherwise uses the ControlSystem.
		/// </summary>
		/// <typeparam name="TEndpoint"></typeparam>
		/// <param name="ipid"></param>
		/// <param name="dmAddress"></param>
		/// <param name="dmSwitchId"></param>
		/// <param name="factory"></param>
		/// <param name="instantiate1">Instantiate via control system with specified IPID</param>
		/// <param name="instantiate2">Instantiate via DMOutput with specified IPID</param>
		/// <param name="instantiate3">Instantiate via DMOutput</param>
		/// <returns></returns>
		[NotNull]
		public static TEndpoint InstantiateEndpoint<TEndpoint>(byte? ipid, int? dmAddress, int? dmSwitchId,
		                                                       IDeviceFactory factory,
		                                                       Func<byte, CrestronControlSystem, TEndpoint> instantiate1,
		                                                       Func<byte, DMOutput, TEndpoint> instantiate2,
		                                                       Func<DMOutput, TEndpoint> instantiate3)
		{
			if (dmSwitchId == null)
			{
				if (ipid == null)
					throw new ArgumentNullException("ipid", "Can't instantiate ControlSystem endpoint without IPID");
				return instantiate1((byte)ipid, ProgramInfo.ControlSystem);
			}

			if (dmAddress == null)
				throw new ArgumentNullException("dmAddress", "Can't instantiate DM endpoint without DM address");

			IDmParent provider = factory.GetDeviceById((int)dmSwitchId) as IDmParent;
			if (provider == null)
			{
				throw new ArgumentException(string.Format("Device {0} is not a {1}", dmSwitchId, typeof(IDmParent).Name),
				                            "dmSwitchId");
			}

			DMOutput output = provider.GetDmOutput((int)dmAddress);
			return ipid == null ? instantiate3(output) : instantiate2((byte)ipid, output);
		}

		public static int GetSwitcherOutputPortNumber(int physicalCardSlot, int outputOnCard)
		{
			return ((physicalCardSlot - 1) * 2) + outputOnCard;
		}
	}
}

#endif
