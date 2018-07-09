using System;
using System.Text.RegularExpressions;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Routing.Extron.Devices.Switchers;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public class ExtronVolumeDeviceControl : AbstractExtronVolumeDeviceControl, IVolumeMuteFeedbackDeviceControl
	{
		private const string VOLUME_FEEDBACK_REGEX = @"Ds(?:G|H)(\d{5})\*(-?\d+)";
		private const string MUTE_FEEDBACK_REGEX = @"Ds(\d{5})\*(-?\d+)";
		
		private readonly eExtronVolumeObject m_VolumeObject;

		public ExtronVolumeDeviceControl(IDtpCrosspointDevice parent, int id, string name, eExtronVolumeObject volumeObject)
			: base(parent, id, name, ExtronVolumeUtils.GetVolumeTypeForObject(volumeObject))
		{
			m_VolumeObject = volumeObject;
			Subscribe(parent);
		}

		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			if (Parent != null)
				Unsubscribe(Parent);
		}

		#region Methods

		public override void SetVolumeRaw(float volume)
		{
			int volumeParam = (int) (volume * 10);
			int objectId = (int) m_VolumeObject;

			Parent.SendCommand("WG{0}*{1}AU", objectId, volumeParam);

			// Output analog volume needs to control left and right channel
			if(m_VolumeObject >= eExtronVolumeObject.Output1AnalogVolume && m_VolumeObject <= eExtronVolumeObject.Output4AnalogVolume)
				Parent.SendCommand("WG{0}*{1}AU", objectId + 1, volumeParam);
		}

		public override void SetVolumeMute(bool mute)
		{
			Parent.SendCommand("WM{0}*{1}AU", (int) m_VolumeObject, mute ? 1 : 0);
		}

		protected override void Initialize()
		{
			int volumeObjectId = (int) m_VolumeObject;
			Parent.SendCommand("WG{0}AU", volumeObjectId);
			Parent.SendCommand("WM{0}AU", volumeObjectId);
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
			var match = Regex.Match(args.Data, VOLUME_FEEDBACK_REGEX);
			if (match.Success)
			{
				int objectId = int.Parse(match.Groups[1].Value);
				if (objectId == (int) m_VolumeObject)
					SetVolumeRawProperty(int.Parse(match.Groups[2].Value) / 10f);
				return;
			}

			match = Regex.Match(args.Data, MUTE_FEEDBACK_REGEX);
			if(match.Success)
			{
				int objectId = int.Parse(match.Groups[1].Value);
				if (objectId == (int) m_VolumeObject)
					VolumeIsMuted = match.Groups[2].Value == "1";
			}
		}

		#endregion
	}
}
