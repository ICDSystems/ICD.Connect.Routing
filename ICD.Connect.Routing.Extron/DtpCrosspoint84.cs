using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ICD.Connect.Routing.Extron
{
	public sealed class DtpCrosspoint84 : AbstractDtpCrosspointDevice<DtpCrosspoint84Settings>
	{
		public DtpCrosspoint84()
		{
			Controls.Add(new DtpCrosspoint84SwitcherControl(this, 0));
		}
	}
}