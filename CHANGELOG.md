# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2025-02-25

### Fixed

- Bulk buying broken/disabled by [Item Extensions](https://www.nexusmods.com/stardewvalley/mods/20357) 1.14.0.

## [0.2.0] - 2024-11-11

### Fixed

- Rebuilt against current game version to resolve `MissingFieldException` on `ShopMenu.itemPriceAndStock`.

## [0.1.1] - 2024-08-09

### Fixed

- Acceleration wasn't working after dismissing and reopening shop menu due to instability of game's `lastCursorMotionWasMouse` state. Instead, check explicitly that the action/tool buttons are coming from the controller, which should always work.

## [0.1.0] - 2024-08-08

### Added

- Initial release.
- Control acceleration/speed with controller A + X buttons.
- Speed configuration via GMCM.

[Unreleased]: https://github.com/focustense/StardewBulkBuy/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/focustense/StardewBulkBuy/compare/v0.2.0...v1.0.0
[0.2.0]: https://github.com/focustense/StardewBulkBuy/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/focustense/StardewBulkBuy/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/focustense/StardewBulkBuy/tree/v0.1.0