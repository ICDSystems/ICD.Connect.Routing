using ICD.Connect.Routing.Connections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ICD.Connect.Routing
{
	public struct ConnectorInfo
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
			return string.Format("{0}(Address={1}, ConnectionType={2})", GetType().Name, Address, ConnectionType);
		}

		#endregion

		#region Equality

		/// <summary>
		///     Implementing default equality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator ==(ConnectorInfo a1, ConnectorInfo a2)
		{
			return a1.Equals(a2);
		}

		/// <summary>
		///     Implementing default inequality.
		/// </summary>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <returns></returns>
		public static bool operator !=(ConnectorInfo a1, ConnectorInfo a2)
		{
			return !(a1 == a2);
		}

		/// <summary>
		///     Returns true if this instance is equal to the given object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			if (other == null || GetType() != other.GetType())
				return false;

			return GetHashCode() == ((ConnectorInfo)other).GetHashCode();
		}

		/// <summary>
		///     Gets the hashcode for this instance.
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

		#endregion
	}
}
