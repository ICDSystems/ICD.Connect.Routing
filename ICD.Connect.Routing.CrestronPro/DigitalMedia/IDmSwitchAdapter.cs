using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
	public interface IDmSwitchAdapter
	{
		/// <summary>
		/// Gets the wrapped switch instance.
		/// </summary>
		[CanBeNull]
		Switch Switch { get; }
	}
}
