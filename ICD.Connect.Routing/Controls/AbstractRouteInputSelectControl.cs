using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls
{
	public abstract class AbstractRouteInputSelectControl<TParent> : AbstractRouteDestinationControl<TParent>, IRouteInputSelectControl
		where TParent : IDeviceBase
	{
		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		private int? m_ActiveInput;

		/// <summary>
		/// Gets the current active input.
		/// </summary>
		public int? ActiveInput
		{
			get { return m_ActiveInput; }
			protected set
			{
				if (value == m_ActiveInput)
					return;

				if (value.HasValue && !ContainsInput(value.Value))
					throw new InvalidOperationException("Can not set input to unavailable connector.");

				int? old = m_ActiveInput;

				m_ActiveInput = value;

				// Stopped using the old input
				if (old.HasValue)
				{
					ConnectorInfo oldInfo = GetInput(old.Value);
					OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(old.Value, oldInfo.ConnectionType, false));
				}

				// Started using the new input
				if (m_ActiveInput.HasValue)
				{
					ConnectorInfo newInfo = GetInput(m_ActiveInput.Value);
					OnActiveInputsChanged.Raise(this,
					                            new ActiveInputStateChangeEventArgs(m_ActiveInput.Value, newInfo.ConnectionType, true));
				}
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractRouteInputSelectControl(TParent parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnActiveInputsChanged = null;

			base.DisposeFinal(disposing);
		}

		/// <summary>
		/// Sets the current active input.
		/// </summary>
		/// <param name="input"></param>
		public abstract void SetActiveInput(int? input);
	}
}
