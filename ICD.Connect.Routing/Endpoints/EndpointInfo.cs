﻿using ICD.Common.Utils;
using ICD.Connect.Devices.Controls;
using Newtonsoft.Json;

namespace ICD.Connect.Routing.Endpoints
{
	/// <summary>
	/// Simple struct defining the connection address on a routing control.
	/// </summary>
	public struct EndpointInfo
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

			builder.AppendProperty("Device", Device);
			builder.AppendProperty("Control", Control);
			builder.AppendProperty("Address", Address);

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
			return !(a1 == a2);
		}

		/// <summary>
		/// Returns true if this instance is equal to the given object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			if (other == null || GetType() != other.GetType())
				return false;

			return GetHashCode() == ((EndpointInfo)other).GetHashCode();
		}

		/// <summary>
		/// Returns true if the endpoints share the same control info without checking address.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool EqualsControl(EndpointInfo other)
		{
			return other.Device == m_DeviceId && other.Control == m_ControlId;
		}

		/// <summary>
		/// Gets the hashcode for this instance.
		/// </summary>
		/// <returns></returns>
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

		#endregion
	}
}
