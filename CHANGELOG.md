# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
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
 