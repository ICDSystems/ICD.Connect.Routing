using System;
using System.Text.RegularExpressions;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public sealed class ExtronVolumeDeviceControl : AbstractExtronVolumeDeviceControl
	{
		private const string VOLUME_FEEDBACK_REGEX = @"Ds(?:G|H)(\d{5})\*(-?\d+)";
		private const string MUTE_FEEDBACK_REGEX = @"Ds(\d{5})\*(-?\d+)";

		private readonly eExtronVolumeObject m_VolumeObject;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="uuid"></param>
		/// <param name="name"></param>
		/// <param name="volumeObject"></param>
		public ExtronVolumeDeviceControl(IDtpCrosspointDevice parent, int id, Guid uuid, string name, eExtronVolumeObject volumeObject)
			: base(parent, id, uuid, name, ExtronVolumeUtils.GetVolumeTypeForObject(volumeObject))
		{
			m_VolumeObject = volumeObject;

			SupportedVolumeFeatures = eVolumeFeatures.Mute |
			                          eVolumeFeatures.MuteAssignment |
			                          eVolumeFeatures.MuteFeedback |
			                          eVolumeFeatures.Volume |
			                          eVolumeFeatures.VolumeAssignment |
			                          eVolumeFeatures.VolumeFeedback;
		}

		#region Methods

		/// <summary>
		/// Sets the raw volume. This will be clamped to the min/max and safety min/max.
		/// </summary>
		/// <param name="level"></param>
		public override void SetVolumeLevel(float level)
		{
			int volumeParam = (int)(level * 10);
			int objectId = (int)m_VolumeObject;

			Parent.SendCommand("WG{0}*{1}AU", objectId, volumeParam);

			// Output analog volume needs to control left and right channel
			if (m_VolumeObject >= eExtronVolumeObject.Output1AnalogVolume &&
			    m_VolumeObject <= eExtronVolumeObject.Output4AnalogVolume)
				Parent.SendCommand("WG{0}*{1}AU", objectId + 1, volumeParam);
		}

		/// <summary>
		/// Sets the mute state.
		/// </summary>
		/// <param name="mute"></param>
		public override void SetIsMuted(bool mute)
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

		protected override void Subscribe(IDtpCrosspointDevice parent)
		{
			base.Subscribe(parent);

			parent.OnResponseReceived += ParentOnResponseReceived;
		}

		protected override void Unsubscribe(IDtpCrosspointDevice parent)
		{
			base.Unsubscribe(parent);

			parent.OnResponseReceived -= ParentOnResponseReceived;
		}

		private void ParentOnResponseReceived(object sender, StringEventArgs args)
		{
			// Volume feedback
			var match = Regex.Match(args.Data, VOLUME_FEEDBACK_REGEX);
			if (match.Success)
			{
				int objectId = int.Parse(match.Groups[1].Value);
				if (objectId == (int) m_VolumeObject)
					VolumeLevel = int.Parse(match.Groups[2].Value) / 10f;
				return;
			}

			// Mute feedback
			match = Regex.Match(args.Data, MUTE_FEEDBACK_REGEX);
			if (match.Success)
			{
				int objectId = int.Parse(match.Groups[1].Value);
				if (objectId == (int) m_VolumeObject)
					IsMuted = match.Groups[2].Value == "1";
			}
		}

		#endregion
	}
}
