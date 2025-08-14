# Changelog
Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/). Adheres to [Semantic Versioning](https://semver.org/).
## [2.0.0] - 2025-08-13
### Added
- Support for Cartel update (gamme v0.4.0 beta) -- ensures methods only grab player dealers and not the cartel's.
- Separate data per save file.
### Fixed
- Better cleanup of data when game unloads, prevent potential issue when switching saves without exiting game.
## [1.2.0] - 2025-08-01
### Fixed
- Fixed jars and bricks counting as 1. Dealers now report same total product count as shown in Dealer app.
### Added
- Master Settings to configure all dealers at once. Optional per-deal override available.
- Option to notify on new navigation destination set.
- Track each deal location by deals started / success / failure, customer usage, and dealer usage.
- Seamlessly save internal data when game is saved, auto load at startup.
- Automatically save running log of all deals to disk.
- +All features from 1.1.0 (not released)
### Changed
- Notification settings removed; message types changed from On/Off bools to Notify/Silent/Disabled choices.
## [1.1.0] - 2025-07-28
_Not publicly released_
### Fixed
- Now accounts for dealer cut, and quick delivery bonus when applicable.
### Added
- Provide specific reason for deal failures, see list.
- Option to alert if dealer or customer is injured or dies.
- Option to alert if dealer appears stuck for prolonged time.
## [1.0.0] - 2025-07-26
- Initial Release
- Send text on deal started, success, failure. Optional notification setting.
- Send text when product drops below threshold, or cash reaches above threshold.
- Send daily summary of deals completed and failed, customer log, and location log.
- Option to configure settings on a per-dealer basis.