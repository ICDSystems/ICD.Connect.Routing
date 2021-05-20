using System;

namespace ICD.Connect.Routing.Extron.Controls.Volume
{
	public enum eExtronVolumeType
	{
		[ExtronObjectIdRange(20000, 29999)]
		[ExtronVolumeRange(-35, 25)]
		MixPoint,

		[ExtronObjectIdRange(30000, 30099)]
		[ExtronVolumeRange(-18, 24)]
		LineInputGain,

		[ExtronObjectIdRange(30100, 30199)]
		[ExtronVolumeRange(-12, 12)]
		LineInputTrim,

		[ExtronObjectIdRange(40000, 40099)]
		[ExtronVolumeRange(-18, 80)]
		MicInputGain,

		[ExtronObjectIdRange(40100, 40199)]
		[ExtronVolumeRange(-100, 12)]
		MicPreMixerGain,

		[ExtronObjectIdRange(50000, 50099)]
		[ExtronVolumeRange(-100, 12)]
		LineOutputPostSwitcherGain,

		[ExtronObjectIdRange(50100, 50299)]
		[ExtronVolumeRange(-100, 12)]
		PreMixerGain,

		[ExtronObjectIdRange(60000, 60099)]
		[ExtronObjectIdRange(60200, 60399)]
		[ExtronVolumeRange(-100, 0)]
		LineOutputGain,

		[ExtronObjectIdRange(60100, 60199)]
		[ExtronVolumeRange(-12, 12)]
		LineOutputTrim
	}

	/// <summary>
	/// Specifies the valid range of IDs for a volume type
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
	internal sealed class ExtronObjectIdRangeAttribute : Attribute
	{
		public int RangeMin { get; private set; }
		public int RangeMax { get; private set; }

		public ExtronObjectIdRangeAttribute(int rangeMin, int rangeMax)
		{
			RangeMin = rangeMin;
			RangeMax = rangeMax;
		}
	}

	/// <summary>
	/// Specifies the valid range of values for this volume type in dB
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class ExtronVolumeRangeAttribute : Attribute
	{
		public float VolumeMin { get; private set; }
		public float VolumeMax { get; private set; }

		public ExtronVolumeRangeAttribute(float volumeMin, float volumeMax)
		{
			VolumeMin = volumeMin;
			VolumeMax = volumeMax;
		}
	}
}
