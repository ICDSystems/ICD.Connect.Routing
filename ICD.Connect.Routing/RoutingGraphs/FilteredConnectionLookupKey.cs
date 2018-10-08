using System;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.RoutingGraphs
{
	public struct FilteredConnectionLookupKey : IEquatable<FilteredConnectionLookupKey>, IComparable<FilteredConnectionLookupKey>
	{
		private readonly EndpointInfo m_Source;
		private readonly EndpointInfo m_FinalDestination;
		private readonly eConnectionType m_Flag;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="finalDestination"></param>
		/// <param name="flag"></param>
		public FilteredConnectionLookupKey(EndpointInfo source, EndpointInfo finalDestination, eConnectionType flag)
		{
			m_Source = source;
			m_FinalDestination = finalDestination;
			m_Flag = flag;
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(FilteredConnectionLookupKey other)
		{
			return m_Source.Equals(other.m_Source) &&
			       m_FinalDestination.Equals(other.m_FinalDestination) &&
			       m_Flag == other.m_Flag;
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is FilteredConnectionLookupKey && Equals((FilteredConnectionLookupKey)obj);
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = m_Source.GetHashCode();
				hashCode = (hashCode * 397) ^ m_FinalDestination.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)m_Flag;
				return hashCode;
			}
		}

		public int CompareTo(FilteredConnectionLookupKey other)
		{
			int sourceComparison = m_Source.CompareTo(other.m_Source);
			if (sourceComparison != 0)
				return sourceComparison;

			int finalDestinationComparison = m_FinalDestination.CompareTo(other.m_FinalDestination);
			if (finalDestinationComparison != 0)
				return finalDestinationComparison;

			return m_Flag.CompareTo(other.m_Flag);
		}
	}
}
