# CharEncodingTool — guidance for Claude Code

A Windows desktop tool that converts strings to byte arrays across every common
text encoding, side-by-side, for debugging device API calls. Also intended as a
hands-on learning aid for character encodings.

## Stack and conventions

- **.NET 10 LTS**, WPF, MVVM via `CommunityToolkit.Mvvm` (8.x).
- The solution file is `CharEncodingTool.slnx` (the new XML format introduced
  in .NET 9/10). The legacy `.sln` does not exist — don't try to build against it.
- Three projects:
  - `src/CharEncodingTool.Core` — UI-free class library. All encoding logic
    lives here. **Add no UI / WPF / framework-specific dependencies.**
  - `src/CharEncodingTool.App` — WPF app. Views in `MainWindow.xaml`, view
    models in `ViewModels/`, value converters in `Converters/`.
  - `tests/CharEncodingTool.Core.Tests` — xUnit tests. Every Core service has
    a paired test class.
- **MVVM:** use the `[ObservableProperty] public partial T Prop { get; set; }`
  source-generator pattern. Commands use `[RelayCommand]`. Avoid hand-rolled
  `INotifyPropertyChanged`.
- **Code style:** file-scoped namespaces, nullable enabled, implicit usings on.
  Default to no comments — only add one when the *why* is non-obvious (e.g.,
  surrogate pair handling, BOM preamble emission).

## Build, test, run

```powershell
dotnet build CharEncodingTool.slnx
dotnet test  CharEncodingTool.slnx
dotnet run --project src/CharEncodingTool.App
```

The build output exe lives at
`src/CharEncodingTool.App/bin/Debug/net10.0-windows/CharEncodingTool.App.exe`.

When you change UI, always actually launch the exe and verify the window opens
and the tabs render — type-checking is not enough. Build success ≠ feature
success.

## Project structure rationale

The Core/App split exists deliberately so the encoding logic can be reused
later (CLI, MAUI, web). When tempted to add helpers in the App project, ask
whether they belong in Core instead — anything that operates on strings/bytes
without WPF dependencies should be in Core.

## Where the encoding logic lives

- `EncodingCatalog` — the registry of supported encodings. Add new encodings
  here; do **not** instantiate `Encoding` objects directly elsewhere.
- `EncodingService` — encode/decode operations. Owns BOM preamble emission
  (BOM is not part of `Encoding.GetBytes` output in .NET; we prepend it manually
  to match the configured `byteOrderMark` flag).
- `ByteFormatter` — hex / Base64 / percent-encoded output, plus a permissive
  hex parser that accepts common formats (`48 65`, `0x48,0x65`, `\x48\x65`, `48-65`).
- `EscapeSequenceParser` — C-style escapes in user input (`\0`, `\n`, `\xNN`,
  `\uXXXX`, `\UXXXXXXXX`).
- `ControlCharRenderer` — maps C0 controls to Unicode Control Pictures so the
  UI can display invisible characters visibly.
- `CodePointAnalyzer` — splits a string into Unicode code points (handling
  surrogate pairs correctly) and returns the bytes each one produces in every
  encoding.

## UI conventions

- One `ScrollViewer` + `StackPanel` for the Learn tab — keep wording tight and
  prefer monospace blocks for byte tables.
- All hex/byte text uses the `MonoCell` or `MonoBox` styles (Cascadia Mono → Consolas → Courier fallback).
- `DataGrid` is preferred over hand-rolled `ItemsControl` for tabular data —
  the comparison view and code-point breakdown both use `ResultGrid` style.

## Things to avoid

- Don't add backwards-compat shims, feature flags, or "TODO" placeholders. If
  a feature is replaced, delete the old code outright.
- Don't suppress build warnings without addressing them.
- Don't push to `origin/main` without asking the user first.
- Don't commit `.claude/` (gitignored). Don't commit `bin/` or `obj/`.

## Git

Default branch is `main`. The local `master` was renamed to `main` after the
initial commit. Origin: https://github.com/epdsn/CharEncodingTool.git
