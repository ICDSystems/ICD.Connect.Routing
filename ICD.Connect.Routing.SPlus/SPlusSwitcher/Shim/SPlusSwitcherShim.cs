using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.SPlusShims;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.EventArgs;
using ICD.Connect.Routing.SPlus.SPlusSwitcher.State;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.Shim
{

	public delegate void SetRouteDelegate(ushort output, ushort input, ushort layer);

	public delegate ushort GetInputSyncDelegate(ushort input);

	public delegate ushort GetInputForOutputDelegate(ushort output, ushort layer);

	[PublicAPI("S+")]
	public sealed class SPlusSwitcherShim : AbstractSPlusDeviceShim<ISPlusSwitcher>
	{

		private const ushort LAYER_AUDIO_USHORT = 1;
		private const ushort LAYER_VIDEO_USHORT = 2;
		private const ushort LATER_USB_USHORT = 3;

		#region SPlus

		#region Delegates to S+

		[PublicAPI("S+")]
		public SetRouteDelegate SetRoute { get; set; }

		[PublicAPI("S+")]
		public GetInputSyncDelegate GetInputSync { get; set; }

		[PublicAPI("S+")]
		public GetInputForOutputDelegate GetInputForOutput { get; set; }

		#endregion

		#region Methods from S+

		[PublicAPI("S+")]
		public void SetSignalDetectedStateFeedback(ushort input, ushort state)
		{
			if (Originator != null)
				Originator.SetSignalDetectedStateFeedback(input, state.ToBool());
		}

		[PublicAPI("S+")]
		public void SetInputForOutputFeedback(ushort output, ushort input, ushort type)
		{
			int? inputNullable = input == 0 ? (int?)null : input;

			if (Originator != null)
				Originator.SetInputForOutputFeedback(output, inputNullable, ConvertUshortToLayer(type));
		}

		[PublicAPI("S+")]
		public void ClearCache()
		{
			if (Originator != null)
				Originator.ClearCache();
		}

		[PublicAPI("S+")]
		public void SetState(ushort[] inputsDetected, ushort[] audioOutFeedback, ushort[] videoOutFeedbak, ushort[] usbOutFeedback)
		{
			if (Originator == null)
				return;

			SPlusSwitcherState state = new SPlusSwitcherState(inputsDetected, audioOutFeedback, videoOutFeedbak, usbOutFeedback);
			Originator.SetState(state);
		}

		#endregion

		#endregion

		#region Originator Callbacks
		/// <summary>
		/// Subscribes to the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Subscribe(ISPlusSwitcher originator)
		{
			base.Subscribe(originator);

			if (originator == null)
				return;

			originator.OnSetRoute += OriginatorOnSetRoute;
			originator.OnClearRoute += OriginatorOnClearRoute;
		}

		/// <summary>
		/// Unsubscribes from the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Unsubscribe(ISPlusSwitcher originator)
		{
			base.Unsubscribe(originator);

			if (originator == null)
				return;

			originator.OnSetRoute -= OriginatorOnSetRoute;
			originator.OnClearRoute -= OriginatorOnClearRoute;
		}

		private void OriginatorOnSetRoute(object sender, SetRouteApiEventArgs args)
		{
			SetRouteDelegate callback = SetRoute;
			if (callback == null)
				return;

			if (EnumUtils.HasMultipleFlags(args.Type))
			{
				EnumUtils.GetFlagsExceptNone(args.Type).ForEach(t => OriginatorOnSetRoute(sender, new SetRouteApiEventArgs(args.Output, args.Input, t)));
				return;
			}

			callback((ushort)args.Output, (ushort)args.Input, ConvertLayerToUshort(args.Type));
		}

		private void OriginatorOnClearRoute(object sender, ClearRouteApiEventArgs args)
		{
			SetRouteDelegate callback = SetRoute;
			if (callback == null)
				return;

			if (EnumUtils.HasMultipleFlags(args.Type))
			{
				EnumUtils.GetFlagsExceptNone(args.Type).ForEach(t => OriginatorOnClearRoute(sender, new ClearRouteApiEventArgs(args.Output, t)));
				return;
			}

			callback((ushort)args.Output, 0, ConvertLayerToUshort(args.Type));
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Converts the layer eConnectionType to a ushort for use with S+
		/// Must be a single flag
		/// </summary>
		/// <param name="layer"></param>
		/// <returns></returns>
		private static ushort ConvertLayerToUshort(eConnectionType layer)
		{
			if (!EnumUtils.HasSingleFlag(layer))
				throw new InvalidOperationException("layer must be a single flag");
			switch (layer)
			{
				case eConnectionType.Audio:
					return LAYER_AUDIO_USHORT;
				case eConnectionType.Video:
					return LAYER_VIDEO_USHORT;
				case eConnectionType.Usb:
					return LATER_USB_USHORT;
				default:
					throw new ArgumentOutOfRangeException("layer is not a recognized value");
			}
		}

		private static eConnectionType ConvertUshortToLayer(ushort layer)
		{
			switch (layer)
			{
				case LAYER_AUDIO_USHORT:
					return eConnectionType.Audio;
				case LAYER_VIDEO_USHORT:
					return eConnectionType.Video;
				case LATER_USB_USHORT:
					return eConnectionType.Usb;
				default:
					throw new ArgumentOutOfRangeException("layer");
			}
		}

		#endregion
	}
}
