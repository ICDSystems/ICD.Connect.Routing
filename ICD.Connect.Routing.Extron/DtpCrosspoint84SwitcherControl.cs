using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace ICD.Connect.Routing.Extron
{
	public sealed class DtpCrosspoint84SwitcherControl : AbstractDtpCrosspointSwitcherControl<DtpCrosspoint84, DtpCrosspoint84Settings>
	{
		public DtpCrosspoint84SwitcherControl(DtpCrosspoint84 parent, int id) : base(parent, id)
		{
		}

		protected override int NumberOfInputs
		{
			get { return 8; }
		}

		protected override int NumberOfOutputs
		{
			get { return 4; }
		}
	}
}