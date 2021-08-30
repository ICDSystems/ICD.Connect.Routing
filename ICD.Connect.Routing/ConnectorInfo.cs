#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
using RealNewtonsoft.Newtonsoft.Json.Converters;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endif
using System;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing
{
	public struct ConnectorInfo : IComparable<ConnectorInfo>, IEquatable<ConnectorInfo>
	{
		private readonly int m_Address;
		private readonly eConnectionType m_ConnectionType;

		#region Properties

		/// <summary>
		/// Gets the address for the connector.
		/// </summary>
		public int Address { get { return m_Address; } }

		/// <summary>
		/// Gets the type/s for the connector.
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public eConnectionType ConnectionType { get { return m_ConnectionType; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="address"></param>
		/// <param name="connectionType"></param>
		[JsonConstructor]
		public ConnectorInfo(int address, eConnectionType connectionType)
		{
			m_Address = address;
			m_ConnectionType = connectionType;
		}

		#region Methods

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Address", Address);
			builder.AppendProperty("ConnectionType", ConnectionType);

			return builder.ToString();
		}

		#endregion

		#region Equality

		/// <summary>
		/// Implementing default equality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator ==(ConnectorInfo a1, ConnectorInfo a2)
		{
			return a1.Equals(a2);
		}

		/// <summary>
		/// Implementing default inequality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator !=(ConnectorInfo a1, ConnectorInfo a2)
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
			return other is ConnectorInfo && Equals((ConnectorInfo)other);
		}

		public bool Equals(ConnectorInfo other)
		{
			return m_Address == other.m_Address &&
			       m_ConnectionType == other.m_ConnectionType;
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
				hash = hash * 23 + m_Address;
				hash = hash * 23 + (int)m_ConnectionType;
				return hash;
			}
		}

		public int CompareTo(ConnectorInfo other)
		{
// ReSharper disable ImpureMethodCallOnReadonlyValueField
			int delta = m_Address.CompareTo(other.m_Address);
			if (delta != 0)
				return delta;

			return m_ConnectionType.CompareTo(other.m_ConnectionType);
// ReSharper restore ImpureMethodCallOnReadonlyValueField
		}

		#endregion
	}
}
