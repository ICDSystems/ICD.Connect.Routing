using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume
{
	public sealed class Dmps3CrosspointVolumeControl : AbstractVolumeDeviceControl<ControlSystemDevice>
	{
		private readonly string m_Name;
		private readonly IDmps3Crosspoint m_Crosspoint;

		#region Properties

		/// <summary>
		/// Gets the human readable name for this control.
		/// </summary>
		public override string Name { get { return m_Name; } }

		/// <summary>
		/// Absolute Minimum the raw volume can be
		/// Used as a last resort for position caculation
		/// </summary>
		public override float VolumeLevelMin { get { return m_Crosspoint.VolumeLevelMin / 10.0f; } }

		/// <summary>
		/// Absolute Maximum the raw volume can be
		/// Used as a last resport for position caculation
		/// </summary>
		public override float VolumeLevelMax { get { return m_Crosspoint.VolumeLevelMax / 10.0f; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="uuid"></param>
		/// <param name="name"></param>
		/// <param name="crosspoint"></param>
		public Dmps3CrosspointVolumeControl(ControlSystemDevice parent, int id, Guid uuid, string name, IDmps3Crosspoint crosspoint)
			: base(parent, id, uuid)
		{
			m_Name = name;
			m_Crosspoint = crosspoint;

			SupportedVolumeFeatures = eVolumeFeatures.Mute |
			                          eVolumeFeatures.MuteAssignment |
			                          eVolumeFeatures.MuteFeedback |
			                          eVolumeFeatures.Volume |
			                          eVolumeFeatures.VolumeAssignment |
			                          eVolumeFeatures.VolumeFeedback;

			Subscribe(m_Crosspoint);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(m_Crosspoint);
		}

		#region Methods

		/// <summary>
		/// Sets the mute state.
		/// </summary>
		/// <param name="mute"></param>
		public override void SetIsMuted(bool mute)
		{
			m_Crosspoint.SetVolumeMute(mute);
		}

		/// <summary>
		/// Toggles the current mute state.
		/// </summary>
		public override void ToggleIsMuted()
		{
			m_Crosspoint.VolumeMuteToggle();
		}

		/// <summary>
		/// Sets the raw volume. This will be clamped to the min/max and safety min/max.
		/// </summary>
		/// <param name="level"></param>
		public override void SetVolumeLevel(float level)
		{
			m_Crosspoint.SetVolumeLevel((short)(level * 10));
		}

		/// <summary>
		/// Raises the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeIncrement()
		{
			SetVolumeLevel(VolumeLevel + 1);
		}

		/// <summary>
		/// Lowers the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeDecrement()
		{
			SetVolumeLevel(VolumeLevel - 1);
		}

		/// <summary>
		/// Starts ramping the volume, and continues until stop is called or the timeout is reached.
		/// If already ramping the current timeout is updated to the new timeout duration.
		/// </summary>
		/// <param name="increment">Increments the volume if true, otherwise decrements.</param>
		/// <param name="timeout"></param>
		public override void VolumeRamp(bool increment, long timeout)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Stops any current ramp up/down in progress.
		/// </summary>
		public override void VolumeRampStop()
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Crosspoint Callbacks

		/// <summary>
		/// Subscribe to the crosspoint events.
		/// </summary>
		/// <param name="crosspoint"></param>
		private void Subscribe(IDmps3Crosspoint crosspoint)
		{
			crosspoint.OnVolumeLevelChanged += CrosspointOnVolumeLevelChanged;
			crosspoint.OnMuteStateChanged += CrosspointOnMuteStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the crosspoint events.
		/// </summary>
		/// <param name="crosspoint"></param>
		private void Unsubscribe(IDmps3Crosspoint crosspoint)
		{
			crosspoint.OnVolumeLevelChanged -= CrosspointOnVolumeLevelChanged;
			crosspoint.OnMuteStateChanged -= CrosspointOnMuteStateChanged;
		}

		/// <summary>
		/// Called when the crosspoint volume level changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CrosspointOnVolumeLevelChanged(object sender, GenericEventArgs<short> e)
		{
			VolumeLevel = m_Crosspoint.VolumeLevel / 10.0f;
		}

		/// <summary>
		/// Called when the crosspoint mute state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CrosspointOnMuteStateChanged(object sender, BoolEventArgs e)
		{
			IsMuted = m_Crosspoint.VolumeIsMuted;
		}

		#endregion
	}
}