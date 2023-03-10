# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [18.2.0] - 2023-02-13
### Changed
 - eConnectionType: Added AudioVideo value for convenience

## [18.1.0] - 2022-12-02
### Added
 - SPlusVolumeDevice - volume control only device, with proxy and shim
 
### Changed
 - Refactored/reorganized SPlus Controls and EventArgs

## [18.0.0] - 2022-07-01
### Added
 - IDmNvxSwitcherControl for Nvx Switcher Controls
 - DmNvxD3XSwitcherControl for Nvx D3x's
 - DmNvxE3XSwitcherControl for Nvx E3x's

### Changed
 - DmNvxStreamSwitchers now use IDmNvxSwitcherControl
 - NvxEndpointInfo now uses IDmNvxSwitcherControl
 - DmNvxBaseCalssSwitcherControl better handles NVX endpoints that don't support all features
 - AbstractDmNvxBaseClassAdapter has a virtual method to get switcher control
 - NVX E3x and D3x adapters override GetSwitcherControl to provide their correct switcher controls
 - Updated Crestron SDK to 2.18.96

## [17.1.1] - 2021-10-04
### Changed
 - Fixed issues with Tx active transmission state in switcher and auto switch modes
 - Fixed issue where DMPS3-4k couldn't unroute an audio output if no mixer is assigned

### Changed
 - Replaced RoutingGraph console PrintX commands with regular console collection nodes

## [17.1.0] - 2021-08-18
### Added
 - Added generic relay switcher device for 2x1 switching
 - Added Extron SW2USB and SW4USB drivers

### Changed
 - Fixed bad check when getting digital input ports from ControlSystem devices

## [17.0.0] - 2021-05-14
### Added
 - IN1804 device driver
 - IN1804DI device driver
 - IN1804DO device driver
 - IN1804DI/DO device driver
 - HdMd8x84kzE device driver
 - HdMd8x44kzE device driver
 - HdMd4x44kzE device driver
 - HdMd4x24kzE device driver
 - HdMd4x14kzE device driver
 - DmTx4Kz100C1G device driver
 - IStreamSourceDevice interface
 - RoutingGraphConnectionComponent for Source, Destination, and Midpoint to pull connections from routing graph

### Changed
 - Crestron DM Transmitters now support switcher functionality
 - Better configure Mock Midpoint's input for output on start up
 - Changed mock routing controls to use RoutingGraphConnectionComponents
 - Fixed a bug where the first connection during pathfinding would not be validated

## [16.2.2] - 2021-08-02
### Changed
 - Crestron DmRmc4kzScalerC - added code to work around initial state Crestron bugs, additional console status and commands

## [16.2.1] - 2021-02-04
### Changed
 - ControlSystem - Changed Uptime to StartTime for Program/System

## [16.2.0] - 2021-01-14
### Added
 - Added test routing console command

### Changed
 - ControlSystem - Moved applicable external telemetry into Monitored Device Telemetry, using Telemetry Component

## [16.1.2] - 2020-09-24
### Changed
 - Fixed a bug where default crestron activities were not being initialized

## [16.1.1] - 2020-08-13
### Changed
 - Telemetry namespace change

## [16.1.0] - 2020-07-14
### Changed
 - ControlSystem uptime telemetry is raised as original TimeSpan data
 - Simplified external telemetry providers
 - Fixed system name telemetry

## [16.0.0] - 2020-06-19
### Added
 - Added DestinationGroupString to Destinations, to be used to create DestinationGroups at runtime
 - Added EnableWhenOffline property/event to SourceDestinationBaseCommon
 - Added AVPro AUHD switchers
 - Added Crestron NVX encoders and decoders
 - Control UUIDs are loaded from DSP configs

### Changed
 - MockDestinationDevice now inherits from AbstractMockDevice
 - MockMidpointDevice now inherits from AbstractMockDevice
 - MockSplitterDevice now inherits from AbstractMockDevice
 - MockSourceDevice now inherits from AbstractMockDevice
 - MockSwitcherDevice now inherits from AbstractMockDevice
 - Using new logging context
 - Fixed a bug where switcher telemetry would fail to build due to missing members

## [15.6.1] - 2020-11-16
### Changed
 - Set inputs on 4kzScalerC to always detected, since they don't work well from Crestron
 - Inputs on DmBaseT endpoints are always detected

## [15.6.0] - 2020-10-06
### Changed
 - AtUhdHdvs300,Dmps300CDevice (2 Series), ExtronSwitchers, ExtronDtpComPort - Implemeted StartSettings to start communications with device

## [15.5.0] - 2020-09-23
### Added
 - Added support for DM-RMC-4KZ-SCALER-C as a switcher device

### Changed
 - Changed GetPort methods for DM Receiver endpoints to be more universal (except CEC)
 - Fixed inheritance issues with DM-RMC-4K-SCALER-C and DM-RMC-4K-SCALER-C-DSP

## [15.4.1] - 2020-09-02
### Changed
 - Fixed a bug where DMPS3 microphone and volume controls would throw an exception on disposal

## [15.4.0] - 2020-06-30
### Added
 - Added stream switching support - StreamSwitcherDevice, StreamSourceDevice, MockStreamDestination and associated controls

## [15.3.0] - 2020-03-20
### Added
 - Added Dmc4kzHdoAdapter
 - Added DmcCatoHdAdapter
 - Added DmcFoAdapter
 - Added DmcS2oHdAdapter
 - Added DmcStroAdapter
 - Added misc Crestron input cards
 - Added IO switcher device that is toggled by a digital output port
 - Added mixer configuration options for DMPS3-4k devices, with auto option

### Changed
 - Reworked volume controls to fit new interfaces
 - Fixed Crestron Switchers, so online/offline updates are subscribed to
 - Routing cache fails gracefully when a connection is invalid

## [15.2.2] - 2020-02-06
### Changed
 - Fixed a bug preventing the instantiation of Crestron HDBaseT DM Transmitters and Receivers

## [15.2.1] - 2019-12-04
### Changed
 - Fixes for potential null refs in Extron DTP devices

## [15.2.0] - 2019-11-19
### Added
 - Added method to Source and Destination collections for getting items by device id

## [15.1.1] - 2020-08-13
### Changed
 - Order and Disabled settings are managed by the Originator abstractions

## [15.1.0] - 2019-09-16
### Added
 - Added Source/Destination groups, Source/Destination group collections, and settings

### Changed
 - Using new GenericBaseUtils to standardize crestron device setup and teardown
 - Updated IPowerDeviceControls to use PowerState
 
### Removed
 - No longer including control ID in EndpointInfo string representation when 0

## [15.0.0] - 2019-08-15
### Changed
 - Substantial changes to facillitate Multi-Krang switchers

## [14.3.0] - 2019-08-15
### Added
 - Added Overload method to unroute connections based on their endpoint info
 - Added features for efficiently determining if there is a path from a given source to destination
 
### Changed
 - If an Extron Switcher's port is not connected initialized property is set to false.

### Removed
 - Removed error logging from the DefaultPathfinder when failing to find a path; should be handled by consumers

## [14.2.2] - 2019-08-28
### Changed
 - Routing caches are rebuilt when children are changed

## [14.2.1] - 2019-05-30
### Changed
 - RoutingCache fix for potential infinite recursion

## [14.2.0] - 2019-02-14
### Added
 - Added DM-TX-4KZ-202-C and DM-TX-4KZ-302-C transmitters

## [14.1.0] - 2019-01-29
### Added
 - DMC-SO-HD DM 8G Fiber Output Card
 - DM-RMC-200-S DM 8G Fiber Receiver
 - CEC Port support for all DM endpoints
 - CEC Port support for DMC-HDO

### Changed
 - DM-NVX changed "EthernetId" to "IPID" in settings
 - Significant optimizations to routing pre-caching on startup

## [14.0.0] - 2019-01-10
### Added
 - Added port configuration features to routing devices

## [13.14.2] - 2020-09-11
### Changed
 - Fixed issue with DMPS3-4k-150-C volume crosspoint controls
 - Fixed issue where DMPS3 volume crosspoints didn't get the current state initially
 - Fixed issue instantiating DM 4K 1G endpoints on some switchers

## [13.14.1] - 2020-08-06
### Changed
 - Moved ControlSystemExternalTelemetryNames from CrestronPro to Routing to fix downstream dependency issues

## [13.14.0] - 2020-04-30
### Changed
 - ControlSystemExternalTelemetry - Fixed DHCP state telemetry to properly parse DHCP Off state
 - ControlSystemExternalTelemetry - Removed telemetry names for control system network info (using Device network info instead)
 - ControlSystemExternalTelemetry - Changed telemetry names for network info to use Device telemetry names
 - ControlSystemExternalTelemetry - DateTime fields are converted to ISO-8601 strings
 - ControlSystem - Only add switcher control if control system has inputs/outputs
 - ControlSystemSwitcherControl - Update video source detection on input port only for video detection changed
 - ControlSystemSwitcherControl - Breakaway routing check feedback bools for supported instead of set bools

### Removed
 - SwitcherTelemetry - AudioBreakawayEnabled and UsbBreakawayEnabled removed
 - Removed network adapter telemetery from SwitcherTelemetry
 - SwitcherTelemetry - Removed methods for Output Mute/Unmute/Volume that weren't implemented

## [13.13.2] - 2020-03-05
### Changed
 - Fixed Sync Detection on DmTx4kX02C and DmTx4kzX02C devices
 - Changed Base Event ID check for VideoSourceFeedback to use correct const for DmTx4kX02C and DmTx4kzX02C devices

## [13.13.1] - 2019-12-31
### Changed
 - Fixed issue where DM HDBaseT endpoints wouldn't instantiate in a point-to-point systems
 - Removed USB from the DM HDBaseT endpoints, since they don't support USB

## [13.13.0] - 2019-10-22
### Changed
 - Added relay ports to DmRmc4kScalerC's
 - Refactored Scaler abstracts to not be called "Base"

## [13.12.2] - 2019-10-09
### Changed
 - Fixed a bug where HdMdMXN Switcher Controls did not properly support 4x1 and 6x2 switchers

## [13.12.1] - 2019-08-05
### Changed
 - Fixed NVX IPID serialization

## [13.12.0] - 2019-08-02
### Added
 - Adding event to ISourceDestinationBaseCollection for when a source/destination enable/disable state changes
 - IRoutingGraph exposes FindActivePaths overload

## [13.11.0] - 2019-07-25
### Added
 - Added HDBaseT TX and RX devices
 - Added IDmEndpoint interface to TX and RX devices
 - Added OriginatorIdSettingsProperty to XiO Director settings property

## [13.10.0] - 2019-07-17
### Added
 - Added Dm-Tx-4kz-X02C Abstract and interfaces
 - Added Dm-Tx-4kz-202C and Dm-Tx-4kz-302C transmitters

## [13.9.2] - 2019-07-16
### Added
 - Added ControlPortParentSettingsProperty attribute to Dmps300CComPort property

## [13.9.1] - 2019-07-10
### Changed
 - Fixed a bug that was causing Crestron DM frame switchers to throw an exception on program stop

## [13.9.0] - 2019-07-09
### Added
 - Added DMC-HD-DSP input card adapter, routing control, and settings

## [13.8.1] - 2019-06-10
### Changed
 - Better handling cases where a DMPS3 crosspoint may return a null mixer

## [13.8.0] - 2019-06-07
### Added
 - RoutingCache initializes source transmission, source detection and destination active states

## [13.7.3] - 2019-06-05
### Changed
 - Program/Processor uptimes are formatted for telemetry

## [13.7.2] - 2019-05-30
### Changed
 - Fixed exceptions on program stop due to clearing switchers

## [13.7.1] - 2019-05-30
### Changed
 - Fixed bug where Input/Output ports were being initialized too early in the switcher lifespan

## [13.7.0] - 2019-05-24
### Added
 - Added constructors for Input/Output ports that take ConnectorInfo

## [13.6.0] - 2019-05-17
### Added
 - Added telemetry features for switchers
 - Added telemetry features for CrestronControlSystem
 - Added features for inferring Crestron Input/Output information

## [13.5.2] - 2019-05-24
### Added
 - Added Card Parent and Card Address attributes to Crestron input/output card settings

## [13.5.1] - 2019-05-16
### Changed
 - Failing more gracefully when unable to instantiate a crestron switcher

## [13.5.0] - 2019-05-10
### Changed
 - Logging when Crestron input cards, output cards and streamers fail to instantiate
 - Mock route source controls default to transmitting
 - OnRouteChanged event moved from IRouteSwitcherControl/Device to IRouteMidpointControl/Device

## [13.4.1] - 2019-04-19
### Added
 - Added RoutingCache methods for source/destination state lookup

### Changed
 - Fixed RoutingCache issues with destination active inputs

## [13.4.0] - 2019-04-05
### Added
 - Front panel lockout enabled by default for switcher control systems
 - Console Commands for lockout enable/disable and status
 - IRoutingGraph exposes FindActivePaths overload for source endpoints

## [13.3.0] - 2019-03-21
### Added
 - Added DMC-4K-HD and DMC-4KZ-HD input cards
 - Added DMC-4KZ-C-DSP input card
 - Added DMC-4KZ-C input card
 - Added DMC-4KZ-HD-DSP input card
 - Added DMC-4KZ-CO-HD output card

## [13.2.0] - 2019-02-13
### Added
 - Added DmcCoHd output card variant

## [13.1.1] - 2019-01-22
### Changed
 - Forcing NVX to use auto-initiation, disabling auto routing

## [13.1.0] - 2019-01-02
### Added
 - Added Volume and Microphone controls for DMPS3 control systems
 - Added XML configuration for DMPS3 volume and microphone controls

## [13.0.0] - 2018-11-20
### Added
 - Added console features for configuring NVX device mode

### Changed
 - Fixed KeyNotFoundException on program stop when using cards
 - Better support for input active state by connection type
 - Routing graph populates each type of originator before loading the next batch

## [12.0.0] - 2018-11-08
### Changed
 - Improved clarity in console readouts
 - Improved routing feedback in systems with multiple async switchers
 - Performance improvements
 - Fixed null refs in NVX switchers

### Removed
 - Removed DestinationGroups

## [11.0.0] - 2018-10-30
### Added
 - Added Extron SWHD4K devices
 - Added DmNvx 350 and 351 streaming devices
 - Added DmNvx 350C and 351C streaming cards
 - Added DmXioDirector 80, 160 and Enterprise devices
 - Added DmNvx Primary and Secondary stream switchers

### Changed
 - Fixed StackOverflow when getting active source and there is a loop in the path
 - Significant optimizations to RoutingCache feedback

## [10.2.0] - 2018-10-18
### Added
 - Added console command to 2 series dmps for logging when the dmps makes a route

### Changed
 - Pathfinding optimizations

## [10.1.0] - 2018-10-04
### Added
 - IRoutingGraph exposes method for unrouting a destination endpoint information
 
### Changed
 - Routing graph table builder only shows active input info for active inputs

## [10.0.0] - 2018-09-25
### Added
 - Implemented touchscreen occupancy features

### Changed
 - Volume control refactoring
 - Fixed bug where Extron switcher was not unrouting audio correctly
 - Small performance improvements to routing

## [9.0.0] - 2018-09-14
### Added
 - DMPS3 4K support
 - Added new pathfinding workflow
 - ControlSystemDevice touchscreen controls

### Changed
 - Significant routing performance improvements
 - Switcher operations are aggregated into as few operations as possible

## [8.0.0] - 2018-07-19
### Added
 - Added routing cache for reducing number of events and optimizing routing queries 

### Changed
 - HdMdxxxCE switchers support routing audio to the aux audio output
 - Enabling Audio-Breakaway in 6XN Crestron switchers
 - Added Sig type bool checks
 - Updated how GetOutputs and GetInputs reads SwitcherOutputs/SwitcherInputs
 - Fixes for audio routing on DMPS 4K variants

## [7.2.0] - 2018-07-02
### Added
 - Added MockRouteSplitter device and control

## [7.1.0] - 2018-07-02
### Added
 - Added receiver endpoint variants (100C, 100F, 100S, etc)
 - Adding DmTx4K202CAdapter
 - Added Extron devices (DTP Crosspoint 84, DTP HDMI 330 TX, DTP HDMI 330 RX)
 - Added method to ISourceDestinationBaseCollection for looking up sources/destinations by endpoint
 - Added HdMdxxxCE switcher adapters

## [7.0.1] - 2018-06-19
### Changed
 - Fixed null ref in 6XN switcher instantiation

## [7.0.0] - 2018-06-04
### Added
 - Adding new input and old input properties to route change event args
 - Serial devices use ConnectionStateManager for maintaining connection to remote endpoints
 - Added methods for getting output connector information from source controls

### Changed
 - Pathfinding uses a prebuilt cache to narrow search results

## [6.0.0] - 2018-05-24
### Added
 - Relocated SPlusSwitcherShim to this project.

### Removed
 - Removed element property from settings

## [5.2.1] - 2018-05-09
### Changed
 - Fixed bug where cards were being instantiated in the wrong order
 - Fixed bug where pathfinding would fail if a single address fails
 - Fixed bug where we would try to unregister cards that would error

## [5.2.0] - 2018-05-02
### Added
 - Added interface and abstraction for routing destinations that can have 1 active input at a time (e.g. displays, receivers)
 - Added abstractions and interfaces for BladeSwitch adapters
 - Adding DmMd64X64 and DmMd128X128 adapters

## [5.1.0] - 2018-04-27
### Added
 - Adding extension method to TryGet routing graph from core
 - Adding extension method to get active destination endpoints for a given source
 - Additional routing graph methods for better supporting sources/destinations with multiple addresses

## [5.0.0] - 2018-04-23
### Added
 - Added abstractions for DmMd6XN switchers
 - Adding DmMd6X1 adapter
 - Adding DmMd6X4 adapter
 - Adding DmMd6X6 adapter
 - API proxies for routing controls
 
### Changed
 - Using API event args
 - Sources and destinations have multiple addresses
 