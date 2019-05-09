using ICD.Connect.API.EventArguments;
using ICD.Connect.API.Info;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs
{
	public sealed class ResendActiveInputApiEventArgs : AbstractApiEventArgs
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ResendActiveInputApiEventArgs() : base(SPlusDestinationApi.EVENT_RESEND_ACTIVE_INPUT)
		{
		}

		/// <summary>
		/// Builds an API result for the args.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override void BuildResult(object sender, ApiResult result)
		{
		}
	}
}