# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
 - Using new GenericBaseUtils to standardize crestron device setup and teardown

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
 