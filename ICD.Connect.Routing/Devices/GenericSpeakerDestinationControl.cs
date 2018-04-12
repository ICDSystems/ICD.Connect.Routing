using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.RoutingGraphs;

namespace ICD.Connect.Routing.Devices
{
	public sealed class GenericSpeakerDestinationControl : AbstractRouteDestinationControl<GenericSpeakerDevice>
	{
		/// <summary>
		/// Called when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public GenericSpeakerDestinationControl(GenericSpeakerDevice parent, int id) :
			base(parent, id)
		{
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;

			base.DisposeFinal(disposing);
		}

		#region Methods

		/// <summary>
		/// Returns true if video is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
			{
				return EnumUtils.GetFlagsExceptNone(type)
				                .Select(t => GetSignalDetectedState(input, t))
				                .Unanimous(false);
			}

			if (type == eConnectionType.Audio)
				return true;

			throw new ArgumentException("type");
		}

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public override bool GetInputActiveState(int input, eConnectionType type)
		{
			return true;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			return
				ServiceProvider.GetService<IRoutingGraph>()
				               .Connections
				               .GetChildren()
				               .Where(c => c.ConnectionType.HasFlag(eConnectionType.Audio) &&
				                           c.Destination.Device == Parent.Id &&
				                           c.Destination.Control == Id)
				               .Select(c => new ConnectorInfo(c.Destination.Address, eConnectionType.Audio));
		}

		#endregion
	}
}