using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Routing.Extron.Devices.Switchers;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public class ExtronGroupVolumeDeviceControl : AbstractVolumeRawLevelDeviceControl<IDtpCrosspointDevice>
	{
		private const string SET_VOLUME_REGEX = @"Ds(G|H)(\d{5})\*(-?\d+)";
		private readonly eExtronVolumeObject m_VolumeObjectId;

		public ExtronGroupVolumeDeviceControl(IDtpCrosspointDevice parent, int id, eExtronVolumeObject volumeObjectId) : base(parent, id)
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
				if (m_VolumeObjectId >= eExtronVolumeObject.Mic1InputGain &&
				    m_VolumeObjectId <= eExtronVolumeObject.Mic4InputGain)
					return -18;

				if (m_VolumeObjectId >= eExtronVolumeObject.VirtualReturnAGain &&
				    m_VolumeObjectId <= eExtronVolumeObject.VirtualReturnHGain)
					return -18;

				if (m_VolumeObjectId >= eExtronVolumeObject.Output1AnalogVolume &&
				    m_VolumeObjectId <= eExtronVolumeObject.Output4AnalogVolume)
					return -100;

				return 0;
			}
		}

		protected override float VolumeRawMaxAbsolute
		{
			get
			{
				if (m_VolumeObjectId >= eExtronVolumeObject.Mic1InputGain &&
				    m_VolumeObjectId <= eExtronVolumeObject.Mic4InputGain)
					return 80;

				if (m_VolumeObjectId >= eExtronVolumeObject.VirtualReturnAGain &&
				    m_VolumeObjectId <= eExtronVolumeObject.VirtualReturnHGain)
					return 24;

				if (m_VolumeObjectId >= eExtronVolumeObject.Output1AnalogVolume &&
				    m_VolumeObjectId <= eExtronVolumeObject.Output4AnalogVolume)
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
			if(m_VolumeObjectId >= eExtronVolumeObject.Output1AnalogVolume && m_VolumeObjectId <= eExtronVolumeObject.Output4AnalogVolume)
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
			var match = Regex.Match(args.Data, SET_VOLUME_REGEX);
			if (match.Success)
			{
				int objectId = int.Parse(match.Groups[2].Value);
				if (objectId == (int) m_VolumeObjectId)
				{
					m_VolumeRaw = int.Parse(match.Groups[3].Value) / 10f;
					VolumeFeedback(m_VolumeRaw);
				}
			}
		}

		#endregion
	}
}
