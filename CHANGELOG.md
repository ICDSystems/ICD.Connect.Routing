# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
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
 