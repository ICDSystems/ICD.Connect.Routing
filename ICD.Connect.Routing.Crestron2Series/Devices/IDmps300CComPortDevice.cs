using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ICD.Connect.Routing.Crestron2Series.Devices
{
	public interface IDmps300CComPortDevice : IDmps300CDevice
	{
		ushort ComSpecJoin { get; }
	}
}