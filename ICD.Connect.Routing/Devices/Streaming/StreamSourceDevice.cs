using System;
using System.Collections.Generic;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Devices.Streaming
{
    public sealed class StreamSourceDevice : AbstractDevice<StreamSourceDeviceSettings>, IStreamSourceDevice
    {
	    public event EventHandler<GenericEventArgs<Uri>> OnStreamUriChanged; 

		[CanBeNull]
	    private Uri m_StreamUri;

	    [CanBeNull]
	    public Uri StreamUri
	    {
		    get { return m_StreamUri; }
		    private set
		    {
			    if (m_StreamUri == value)
				    return;

			    m_StreamUri = value;
			    Logger.LogSetTo(eSeverity.Informational, "Stream Uri", m_StreamUri);
			    OnStreamUriChanged.Raise(this, new GenericEventArgs<Uri>(m_StreamUri));
			}
	    }

	    public void SetStreamUri([CanBeNull] Uri value)
	    {
		    StreamUri = value;
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
			    SetStreamUri(new Uri(settings.StreamUri));
		    }
		    catch (Exception e)
		    {
				Logger.Log(eSeverity.Error, "Failed to parse Stream Uri - {0}", e.Message);
				SetStreamUri(null);
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

		    SetStreamUri(null);
	    }

	    /// <summary>
	    /// Override to add controls to the device.
	    /// </summary>
	    /// <param name="settings"></param>
	    /// <param name="factory"></param>
	    /// <param name="addControl"></param>
	    protected override void AddControls(StreamSourceDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
	    {
		    base.AddControls(settings, factory, addControl);

			addControl(new StreamSourceDeviceRoutingControl(this, 0));
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
		                                                   s => SetStreamUri(new Uri(s)));
	    }

	    private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
	    {
		    return base.GetConsoleCommands();
	    }
    }
}
