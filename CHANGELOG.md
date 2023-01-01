# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.0-preview] - 2023-01-01

### Added

- `DisplayName` property in `TemplEntryInfoAttribute` to define how custom `TemplEntry` subclasses should be shown in dropdown menu.
- `ExposedAs` property in `TemplInputAttribute` to define how custom input values are exposed to templates.
- Prevent delete of referenced assets in live entries in Unity Editor.
- Improved validation for live entries.
- Public scaffold API.
- Public live entry API.
- Samples showing API usage.
- Sample custom entry for multiple inputs.
- Support for `include` function in templates with asset path or GUID.

### Changed

- Scaffold selection menu now shows a single list of scaffolds by their asset names.
- Scaffold selection is now disabled when editor is compiling.
- Delete scaffold node shortcut is now Ctrl/Cmd + Delete;

### Fixed

- Live entries editor not responding when custom entry class becomes invalid.
- Filename validation failing for some characters in MacOS.
- Changes not saving to disk right after creating and renaming a new Scriban Template.
- Non-deferred entries set to monitor delete changes not rendering when deleting a monitored asset.

## [0.2.0-preview] - 2022-11-06

### Added

- Scaffold editor.
- Dynamic Scaffold editor.
- Scaffold generation.
- Template functions for case transformations.
- Template functions for asset database operations.
- Scaffold samples.

### Changed

- Simplified custom entry types.
- Renamed entries to live entries.
- Renamed namespaces. This breaks settings created with previous versions.

## [0.1.1-preview] - 2022-08-26

### Changed

- Declare custom entry input field using `TemplInputAttribute`.
- Declare custom entry metadata using `TemplEntryInfoAttribute`.
- Declare custom functions using `TemplFunctionAttribute` on static classes.
- Only entry types with a single input field which is subclass of `UnityEngine.Object` are allowed.

## [0.1.0-preview] - 2022-07-16

### Added

- Initial files.
