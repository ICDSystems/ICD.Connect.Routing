using System;
using System.Collections.Generic;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Routing.Extron.Devices.Switchers;

namespace ICD.Connect.Routing.Extron.Controls
{
	public class ExtronVolumeControl : AbstractVolumeRawLevelDeviceControl<IDtpCrosspointDevice>
	{
		private const string SET_VOLUME_REGEX = @"Ds(G|H)(\d{5})*(\d+)";
		private readonly eExtronDspObject m_VolumeObjectId ;

		public ExtronVolumeControl(IDtpCrosspointDevice parent, int id, eExtronDspObject volumeObjectId) : base(parent, id)
		{
			m_VolumeObjectId = volumeObjectId;
			Subscribe(parent);
		}

		#region Properties

		private float m_VolumeRaw;
		public override float VolumeRaw
		{
			get { return m_VolumeRaw; }
		}

		protected override float VolumeRawMinAbsolute
		{
			get
			{
				if (m_VolumeObjectId >= eExtronDspObject.Mic1InputGain &&
				    m_VolumeObjectId <= eExtronDspObject.Mic4InputGain)
					return -18;

				if (m_VolumeObjectId >= eExtronDspObject.VirtualReturnAGain &&
				    m_VolumeObjectId <= eExtronDspObject.VirtualReturnHGain)
					return -18;

				if (m_VolumeObjectId >= eExtronDspObject.Output1AnalogVolume &&
				    m_VolumeObjectId <= eExtronDspObject.Output4AnalogVolume)
					return -100;

				return 0;
			}
		}

		protected override float VolumeRawMaxAbsolute
		{
			get
			{
				if (m_VolumeObjectId >= eExtronDspObject.Mic1InputGain &&
				    m_VolumeObjectId <= eExtronDspObject.Mic4InputGain)
					return 80;

				if (m_VolumeObjectId >= eExtronDspObject.VirtualReturnAGain &&
				    m_VolumeObjectId <= eExtronDspObject.VirtualReturnHGain)
					return 24;

				if (m_VolumeObjectId >= eExtronDspObject.Output1AnalogVolume &&
				    m_VolumeObjectId <= eExtronDspObject.Output4AnalogVolume)
					return 0;

				return 0;
			}
		}

		#endregion

		#region Methods

		public override void SetVolumeRaw(float volume)
		{
			int volumeParam = (int) (volume * 10);
			int objectId = (int) m_VolumeObjectId;

			Parent.SendCommand("WG{0}*{1}AU", objectId, volumeParam);

			// Output analog volume needs to control left and right channel
			if(m_VolumeObjectId >= eExtronDspObject.Output1AnalogVolume && m_VolumeObjectId <= eExtronDspObject.Output4AnalogVolume)
				Parent.SendCommand("WG{0}*{1}AU", objectId + 1, volumeParam);
		}

		#endregion

		#region Parent Callbacks

		private void Subscribe(IDtpCrosspointDevice parent)
		{
			parent.OnResponseReceived += ParentOnResponseReceived;
		}

		private void Unsubscribe(IDtpCrosspointDevice parent)
		{
			parent.OnResponseReceived -= ParentOnResponseReceived;
		}

		private void ParentOnResponseReceived(object sender, StringEventArgs args)
		{
			
		}

		#endregion
	}
}
