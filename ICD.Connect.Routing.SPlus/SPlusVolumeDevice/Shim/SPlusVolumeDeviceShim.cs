using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.CrestronSPlus.SPlusShims;
using ICD.Connect.Routing.SPlus.EventArgs;

namespace ICD.Connect.Routing.SPlus.SPlusVolumeDevice.Shim
{
    [PublicAPI("S+")]
    public sealed class SPlusVolumeDeviceShim : AbstractSPlusDeviceShim<ISPlusVolumeDeviceShimmable>
    {		
        #region Delegates for S+

		public delegate void GenericDelegate();
		
		public delegate void GenericUshortDelegate(ushort data);

		#endregion

		#region Properties

		[PublicAPI("S+")]
		public GenericUshortDelegate SetVolumeLevel { get; set; }

		[PublicAPI("S+")]
		public GenericUshortDelegate SetVolumeMuteState { get; set; }

		[PublicAPI("S+")]
		public GenericDelegate VolumeMuteToggle { get; set; }

		#endregion

		#region Methods from S+

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
        protected override void Subscribe(ISPlusVolumeDeviceShimmable originator)
        {
            base.Subscribe(originator);

            if (originator == null)
                return;

            originator.OnSetVolumeLevel += OriginatorOnSetVolumeLevel;
            originator.OnSetVolumeMuteState += OriginatorOnSetVolumeMuteState;
            originator.OnVolumeMuteToggle += OriginatorOnVolumeMuteToggle;
        }

        /// <summary>
        /// Unsubscribes from the originator events.
        /// </summary>
        /// <param name="originator"></param>
        protected override void Unsubscribe(ISPlusVolumeDeviceShimmable originator)
        {
            base.Unsubscribe(originator);

            if (originator == null)
                return;

            originator.OnSetVolumeLevel -= OriginatorOnSetVolumeLevel;
            originator.OnSetVolumeMuteState -= OriginatorOnSetVolumeMuteState;
            originator.OnVolumeMuteToggle -= OriginatorOnVolumeMuteToggle;
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

        #endregion
    }
}