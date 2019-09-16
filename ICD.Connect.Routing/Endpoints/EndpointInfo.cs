using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using Newtonsoft.Json;

namespace ICD.Connect.Routing.Endpoints
{
	/// <summary>
	/// Simple struct defining the connection address on a routing control.
	/// </summary>
	public struct EndpointInfo : IComparable<EndpointInfo>, IEquatable<EndpointInfo>
	{
		private readonly int m_DeviceId;
		private readonly int m_ControlId;
		private readonly int m_Address;

		#region Properties

		/// <summary>
		/// Gets the endpoint device.
		/// </summary>
		public int Device { get { return m_DeviceId; } }

		/// <summary>
		/// Gets the endpoint device control.
		/// </summary>
		public int Control { get { return m_ControlId; } }

		/// <summary>
		/// Gets the endpoint connector address.
		/// </summary>
		public int Address { get { return m_Address; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="control"></param>
		/// <param name="address"></param>
		[JsonConstructor]
		public EndpointInfo(int device, int control, int address)
		{
			m_DeviceId = device;
			m_ControlId = control;
			m_Address = address;
		}

		/// <summary>
		/// Gets the string representation of this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Device", m_DeviceId);

			if (m_ControlId != 0)
				builder.AppendProperty("Control", m_ControlId);

			builder.AppendProperty("Address", m_Address);

			return builder.ToString();
		}

		/// <summary>
		/// Reduces a sequence of endpoints into a human readable string.
		/// </summary>
		/// <param name="endpoints"></param>
		/// <returns></returns>
		public static string ArrayRangeFormat(IEnumerable<EndpointInfo> endpoints)
		{
			if (endpoints == null)
				throw new ArgumentNullException("endpoints");

			IcdOrderedDictionary<DeviceControlInfo, List<int>> deviceControlAddresses =
				new IcdOrderedDictionary<DeviceControlInfo, List<int>>();

			foreach (EndpointInfo endpoint in endpoints)
			{
				DeviceControlInfo deviceControl = endpoint.GetDeviceControlInfo();

				List<int> addresses;
				if (!deviceControlAddresses.TryGetValue(deviceControl, out addresses))
				{
					addresses = new List<int>();
					deviceControlAddresses.Add(deviceControl, addresses);
				}

				if (addresses.BinarySearch(endpoint.m_Address) < 0)
					addresses.AddSorted(endpoint.m_Address);
			}

			if (deviceControlAddresses.Count == 0)
				return null;

			// EndpointInfo(Device=x, Control=y, Addresses=[1-10])
			if (deviceControlAddresses.Count == 1)
			{
				KeyValuePair<DeviceControlInfo, List<int>> kvp = deviceControlAddresses.First();
				return ArrayRangeFormat(kvp.Key, kvp.Value);
			}

			// [EndpointInfo(Device=x, Control=y, Addresses=[1-10]), EndpointInfo(Device=z, Control=w, Addresses=[1-10])]
			string[] formats = deviceControlAddresses.Select(kvp => ArrayRangeFormat(kvp.Key, kvp.Value)).ToArray();
			return StringUtils.ArrayFormat(formats);
		}

		/// <summary>
		/// Reduces a sequence of addresses into a human readable string.
		/// </summary>
		/// <param name="deviceControl"></param>
		/// <param name="addresses"></param>
		/// <returns></returns>
		private static string ArrayRangeFormat(DeviceControlInfo deviceControl, IList<int> addresses)
		{
			ReprBuilder builder = new ReprBuilder(new EndpointInfo());

			builder.AppendProperty("Device", deviceControl.DeviceId);
			builder.AppendProperty("Control", deviceControl.ControlId);

			if (addresses.Count == 1)
				builder.AppendProperty("Address", addresses[0]);
			else
				builder.AppendPropertyRaw("Addresses", StringUtils.ArrayRangeFormat(addresses));

			return builder.ToString();
		}

		/// <summary>
		/// Gets the DeviceControlInfo for this endpoint.
		/// </summary>
		/// <returns></returns>
		public DeviceControlInfo GetDeviceControlInfo()
		{
			return new DeviceControlInfo(Device, Control);
		}

		#region Equality

		/// <summary>
		/// Implementing default equality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator ==(EndpointInfo a1, EndpointInfo a2)
		{
			return a1.Equals(a2);
		}

		/// <summary>
		/// Implementing default inequality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator !=(EndpointInfo a1, EndpointInfo a2)
		{
			return !a1.Equals(a2);
		}

		/// <summary>
		/// Returns true if this instance is equal to the given object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			return other is EndpointInfo && Equals((EndpointInfo)other);
		}

		/// <summary>
		/// Returns true if this instance is equal to the given endpoint.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		[Pure]
		public bool Equals(EndpointInfo other)
		{
			return m_DeviceId == other.m_DeviceId &&
			       m_ControlId == other.m_ControlId &&
			       m_Address == other.m_Address;
		}

		/// <summary>
		/// Gets the hashcode for this instance.
		/// </summary>
		/// <returns></returns>
		[Pure]
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + m_DeviceId;
				hash = hash * 23 + m_ControlId;
				hash = hash * 23 + m_Address;
				return hash;
			}
		}

		public int CompareTo(EndpointInfo other)
		{
			int result = Device.CompareTo(other.Device);
			if (result != 0)
				return result;

			result = Control.CompareTo(other.Control);
			if (result != 0)
				return result;

			return Address.CompareTo(other.Address);
		}

		#endregion
	}
}
