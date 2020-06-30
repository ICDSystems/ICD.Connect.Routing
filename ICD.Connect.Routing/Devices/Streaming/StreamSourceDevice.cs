using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Devices.Streaming
{
    public sealed class StreamSourceDevice : AbstractDevice<StreamSourceDeviceSettings>
    {
	    public event EventHandler<GenericEventArgs<Uri>> OnStreamUriChanged; 

		[CanBeNull]
	    private Uri m_StreamUri;

		[CanBeNull]
	    public Uri StreamUri
	    {
		    get { return m_StreamUri; }
		    set
		    {
				if (m_StreamUri == value)
					return;

				m_StreamUri = value;
				Log(eSeverity.Informational, "Stream Uri changed to - {0}", m_StreamUri);
				OnStreamUriChanged.Raise(this, new GenericEventArgs<Uri>(m_StreamUri));
		    }
	    }

	    public StreamSourceDevice()
	    {
			Controls.Add(new StreamSourceDeviceRoutingControl(this, 0));
	    }


	    protected override bool GetIsOnlineStatus()
	    {
		    return true;
	    }

	    #region Settings

	    protected override void ApplySettingsFinal(StreamSourceDeviceSettings settings, IDeviceFactory factory)
	    {
		    base.ApplySettingsFinal(settings, factory);
		    try
		    {
			    StreamUri = new Uri(settings.StreamUri);
		    }
		    catch (Exception e)
		    {
				Log(eSeverity.Error, "Failed to parse Stream Uri - {0}", e.Message);
				StreamUri = null;
		    }
	    }

	    protected override void CopySettingsFinal(StreamSourceDeviceSettings settings)
	    {
		    base.CopySettingsFinal(settings);

		    settings.StreamUri = StreamUri != null ? StreamUri.ToString() : null;
	    }

	    protected override void ClearSettingsFinal()
	    {
		    base.ClearSettingsFinal();

		    StreamUri = null;
	    }

	    #endregion

	    public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
	    {
		    base.BuildConsoleStatus(addRow);

		    addRow("StreamUri", StreamUri);
	    }

	    public override IEnumerable<IConsoleCommand> GetConsoleCommands()
	    {
		    foreach (IConsoleCommand baseConsoleCommand in GetBaseConsoleCommands())
			    yield return baseConsoleCommand;

		    yield return new GenericConsoleCommand<string>("SetStreamUri", "Changes the stream uri",
		                                                   s => StreamUri = new Uri(s));
	    }

	    private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
	    {
		    return base.GetConsoleCommands();
	    }
    }
}
