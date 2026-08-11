# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added (2026-08-11)
- `MudMessageInput`: generic `FooterStart`/`FooterEnd` RenderFragment slots rendering a
  control row beneath the text field (send button moves into the footer row). When both
  are null, the classic single-row layout is preserved unchanged.
- `MudAgentChat` / `MudAgentChatWithHistory`: `ComposerFooterStart`/`ComposerFooterEnd`
  pass-through parameters for the new composer footer slots.
- `MudAgentChat` / `MudAgentChatWithHistory`: `ChatOptionsProvider` (`Func<ChatOptions?>`)
  evaluated at send time and forwarded to `GetStreamingResponseAsync`, enabling consumer-
  supplied per-request model overrides, sampling parameters, and (later) tools.
- `MudMessageList` / `MudAgentChat` / `MudAgentChatWithHistory`: `MessageFooter`
  (`RenderFragment<ChatMessage>`) template rendered beneath each message bubble (e.g.,
  model attribution captions).
- `MudAgentChat`: the response's `ChatResponseUpdate.ModelId` (when reported) is stamped
  onto the final assistant message's `AdditionalProperties["model_id"]`.

### Fixed (2026-08-11)
- `MudAgentChat` now passes `Disabled` to `MudMessageInput` when no agent could be
  resolved (input looked operable but nothing could be sent). Deliberately still enabled
  during streaming so message queueing keeps working.

### Added
- Initial project structure and repository setup
- Multi-targeting support for .NET 8.0 and .NET 9.0
- Core library (`LionFire.AgUi.Blazor`) with Abstractions and Models folders
- MudBlazor component library (`LionFire.AgUi.Blazor.MudBlazor`)
- Server-side library (`LionFire.AgUi.Blazor.Server`)
- WebAssembly library (`LionFire.AgUi.Blazor.Wasm`)
- Testing utilities library (`LionFire.AgUi.Blazor.Testing`)
- Unit test projects with xUnit, FluentAssertions, and Moq
- bUnit support for Blazor component testing
- EditorConfig for consistent code style
- Directory.Build.props for shared build configuration

## [0.1.0] - TBD

### Added
- Initial release (coming soon)

[Unreleased]: https://github.com/lionfire/ag-ui-blazor/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/lionfire/ag-ui-blazor/releases/tag/v0.1.0
