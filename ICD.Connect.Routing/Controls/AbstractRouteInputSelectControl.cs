using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
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

		private readonly Dictionary<eConnectionType, int?> m_ActiveInputs;
		private readonly SafeCriticalSection m_ActiveInputsSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractRouteInputSelectControl(TParent parent, int id)
			: base(parent, id)
		{
			m_ActiveInputs = new Dictionary<eConnectionType, int?>();
			m_ActiveInputsSection = new SafeCriticalSection();
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

		#region Methods

		/// <summary>
		/// Gets the current active input.
		/// </summary>
		public virtual int? GetActiveInput(eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentOutOfRangeException("flag");

			m_ActiveInputsSection.Enter();

			try
			{
				int? input;
				return m_ActiveInputs.TryGetValue(flag, out input) ? input : null;
			}
			finally
			{
				m_ActiveInputsSection.Leave();
			}
		}

		/// <summary>
		/// Sets the current active input.
		/// </summary>
		public abstract void SetActiveInput(int? input, eConnectionType type);

		#endregion

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public sealed override bool GetInputActiveState(int input, eConnectionType type)
		{
			return EnumUtils.GetFlagsExceptNone(type)
			                .All(f => GetActiveInput(f) == input);
		}

		/// <summary>
		/// Updates the internal active input cache.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		protected void SetCachedActiveInput(int? input, eConnectionType type)
		{
			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				SetCachedActiveInputSingle(input, flag);
		}

		/// <summary>
		/// Sets the active input for a single flag.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="flag"></param>
		private void SetCachedActiveInputSingle(int? input, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentOutOfRangeException("flag");

			int? old;

			m_ActiveInputsSection.Enter();

			try
			{
				m_ActiveInputs.TryGetValue(flag, out old);

				// No change
				if (input == old)
					return;

				m_ActiveInputs[flag] = input;
			}
			finally
			{
				m_ActiveInputsSection.Leave();
			}

			// Stopped using the old input
			if (old.HasValue)
				OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(old.Value, flag, false));

			// Started using the new input
			if (input.HasValue)
				OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(input.Value, flag, true));
		}
	}
}
