using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.CrestronSPlus.SPlusShims;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Shim
{
	[PublicAPI("S+")]
	public sealed class SPlusDestinationShim : AbstractSPlusDeviceShim<ISPlusDestinationDevice>
	{
		#region Delegates for S+

		public delegate void GenericDelegate();
		
		public delegate void GenericUshortDelegate(ushort data);

		public delegate ushort GetInputSyncDelegate(ushort input);

		#endregion

		#region Properties
		
		[PublicAPI("S+")]
		public GenericUshortDelegate SetPowerState { get; set; }

		[PublicAPI("S+")]
		public GenericUshortDelegate SetActiveInput { get; set; }

		[PublicAPI("S+")]
		public GenericUshortDelegate SetVolumeLevel { get; set; }

		[PublicAPI("S+")]
		public GenericUshortDelegate SetVolumeMuteState { get; set; }

		[PublicAPI("S+")]
		public GenericDelegate VolumeMuteToggle { get; set; }

		[PublicAPI("S+")]
		public GetInputSyncDelegate GetInputSync { get; set; }

		[PublicAPI("S+")]
		public GenericDelegate ResendActiveInput { get; set; }

		#endregion

		#region Methods from S+

		[PublicAPI("S+")]
		public void SetPowerStateFeedback(ushort state)
		{
			if (Originator != null)
				Originator.SetPowerStateFeedback(state.ToBool() ? ePowerState.PowerOn : ePowerState.PowerOff);
		}

		[PublicAPI("S+")]
		public void SetActiveInputFeedback(ushort input)
		{
			if (Originator == null)
				return;

			if (input == 0)
				Originator.SetActiveInputFeedback(null);
			else
				Originator.SetActiveInputFeedback(input);
		}

		[PublicAPI("S+")]
		public void SetInputDetectedFeedback(ushort input, ushort state)
		{
			if (Originator != null)
				Originator.SetInputDetectedFeedback(input, state.ToBool());
		}

		[PublicAPI("S+")]
		public void SetVolumeLevelFeedback(ushort volume)
		{
			if (Originator != null)
				Originator.SetVolumeLevelFeedback(volume);
		}

		[PublicAPI("S+")]
		public void SetVolumeMuteStateFeedback(ushort state)
		{
			if (Originator != null)
				Originator.SetVolumeMuteStateFeedback(state.ToBool());
		}

		#endregion

		#region Originator Callbacks

		/// <summary>
		/// Subscribes to the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Subscribe(ISPlusDestinationDevice originator)
		{
			base.Subscribe(originator);

			if (originator == null)
				return;

			originator.OnSetPowerState += OriginatorOnSetPowerState;
			originator.OnSetActiveInput += OriginatorOnSetActiveInput;
			originator.OnSetVolumeLevel += OriginatorOnSetVolumeLevel;
			originator.OnSetVolumeMuteState += OriginatorOnSetVolumeMuteState;
			originator.OnVolumeMuteToggle += OriginatorOnVolumeMuteToggle;
			originator.OnResendActiveInput += OriginatorOnResendActiveInput;
		}

		/// <summary>
		/// Unsubscribes from the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Unsubscribe(ISPlusDestinationDevice originator)
		{
			base.Unsubscribe(originator);

			if (originator == null)
				return;

			originator.OnSetPowerState -= OriginatorOnSetPowerState;
			originator.OnSetActiveInput -= OriginatorOnSetActiveInput;
			originator.OnSetVolumeLevel -= OriginatorOnSetVolumeLevel;
			originator.OnSetVolumeMuteState -= OriginatorOnSetVolumeMuteState;
			originator.OnVolumeMuteToggle -= OriginatorOnVolumeMuteToggle;
			originator.OnResendActiveInput -= OriginatorOnResendActiveInput;
		}

		private void OriginatorOnSetPowerState(object sender, PowerControlEventArgs args)
		{
			var callback = SetPowerState;
			if (callback != null)
				callback(args.Data.ToUShort());
		}

		private void OriginatorOnSetActiveInput(object sender, SetActiveInputEventArgs args)
		{
			var callback = SetActiveInput;
			if (callback == null)
				return;

			if (args.Data.HasValue)
				callback((ushort)args.Data.Value);
			else
				callback(0);
		}

		private void OriginatorOnSetVolumeLevel(object sender, SetVolumeLevelEventArgs args)
		{
			var callback = SetVolumeLevel;
			if (callback != null)
				callback(args.Data);
		}

		private void OriginatorOnSetVolumeMuteState(object sender, SetVolumeMuteStateEventArgs args)
		{
			var callback = SetVolumeMuteState;
			if (callback != null)
				callback(args.Data.ToUShort());
		}

		private void OriginatorOnVolumeMuteToggle(object sender, VolumeMuteToggleEventArgs args)
		{
			var callback = VolumeMuteToggle;
			if (callback != null)
				callback();
		}

		private void OriginatorOnResendActiveInput(object sender, ResendActiveInputEventArgs resendActiveInputEventArgs)
		{
			var callback = ResendActiveInput;
			if (callback != null)
				callback();
		}

		#endregion

		protected override void RequestResync()
		{
			base.RequestResync();

			ResetInputDetectedFeedback();
		}

		private void ResetInputDetectedFeedback()
		{
			if (Originator == null || Originator.InputCount == null)
				return;

			GetInputSyncDelegate callback = GetInputSync;

			if (callback == null)
			{
				// If no callback registered, assume no inputs have sync
				Originator.ResetInputDetectedFeedback(new List<int>());
				return;
			}

			int inputCount = Originator.InputCount.Value;

			List<int> inputsWithSync = new List<int>();

			//GetInputSync for all inputs
			for (int i = 1; i <= inputCount; i++)
				if (callback((ushort)i).ToBool())
					inputsWithSync.Add(i);

			//Reset sync data on originator
			Originator.ResetInputDetectedFeedback(inputsWithSync);
		}
	}
}