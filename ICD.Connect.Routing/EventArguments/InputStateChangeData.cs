using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.EventArguments
{
	[Serializable]
	public sealed class InputStateChangeData
	{
		[UsedImplicitly]
		public int Input { get; set; }
		
		[UsedImplicitly]
		public eConnectionType Type { get; set; }
		
		[UsedImplicitly]
		public bool State { get; set; }

		public InputStateChangeData()
		{
		}

		public InputStateChangeData(int input, eConnectionType type, bool state)
		{
			Input = input;
			Type = type;
			State = state;
		}
	}
}