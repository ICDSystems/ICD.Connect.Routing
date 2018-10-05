using System;
using System.Collections;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;

namespace ICD.Connect.Routing
{
	/// <summary>
	/// Reduces routing operations into as few switcher calls as possible.
	/// </summary>
	public sealed class RouteOperationAggregator : IEnumerable<RouteOperation>
	{
		private readonly IcdOrderedDictionary<RouteOperationAggregatorKey, RouteOperation> m_Operations;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RouteOperationAggregator()
		{
			m_Operations = new IcdOrderedDictionary<RouteOperationAggregatorKey, RouteOperation>();
		}

		/// <summary>
		/// Adds the given operation to the set.
		/// </summary>
		/// <param name="operation"></param>
		public void Add(RouteOperation operation)
		{
			if (operation == null)
				throw new ArgumentNullException("operation");

			RouteOperationAggregatorKey key = new RouteOperationAggregatorKey(operation);

			RouteOperation stored;
			if (!m_Operations.TryGetValue(key, out stored))
			{
				stored = new RouteOperation(operation);
				m_Operations[key] = stored;
			}

			stored.ConnectionType |= operation.ConnectionType;
		}

		public IEnumerator<RouteOperation> GetEnumerator()
		{
			return m_Operations.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private sealed class RouteOperationAggregatorKey : IEquatable<RouteOperationAggregatorKey>,
		                                                   IComparable<RouteOperationAggregatorKey>
		{
			private readonly int m_LocalInput;
			private readonly int m_LocalOutput;
			private readonly int m_LocalDevice;
			private readonly int m_LocalControl;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="op"></param>
			public RouteOperationAggregatorKey(RouteOperation op)
			{
				if (op == null)
					throw new ArgumentNullException("op");

				m_LocalInput = op.LocalInput;
				m_LocalOutput = op.LocalOutput;
				m_LocalDevice = op.LocalDevice;
				m_LocalControl = op.LocalControl;
			}

			public bool Equals(RouteOperationAggregatorKey other)
			{
				if (other == null)
					return false;

				return m_LocalInput == other.m_LocalInput &&
					   m_LocalOutput == other.m_LocalOutput &&
					   m_LocalDevice == other.m_LocalDevice &&
					   m_LocalControl == other.m_LocalControl;

			}

			public int CompareTo(RouteOperationAggregatorKey other)
			{
				if (other == null)
					throw new ArgumentNullException("other");

// ReSharper disable ImpureMethodCallOnReadonlyValueField
				int compare = m_LocalDevice.CompareTo(other.m_LocalDevice);
				if (compare != 0)
					return compare;

				compare = m_LocalControl.CompareTo(other.m_LocalControl);
				if (compare != 0)
					return compare;

				compare = m_LocalOutput.CompareTo(other.m_LocalOutput);
				if (compare != 0)
					return compare;

				return m_LocalInput.CompareTo(other.m_LocalInput);
// ReSharper restore ImpureMethodCallOnReadonlyValueField
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hash = 17;
					hash = hash * 23 + m_LocalDevice;
					hash = hash * 23 + m_LocalControl;
					hash = hash * 23 + m_LocalOutput;
					hash = hash * 23 + m_LocalInput;
					return hash;
				}
			}

			public override bool Equals(object obj)
			{
				return obj == this;
			}
		}
	}
}
