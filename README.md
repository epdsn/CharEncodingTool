# CharEncodingTool

A Windows desktop tool for debugging string ↔ byte conversions when talking to
hardware/devices over an API. Built as a hands-on deep dive into how text
encodings actually lay bytes on the wire.

## Stack

- .NET 10 (LTS) — WPF
- MVVM via [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- xUnit tests on a separated `Core` library (UI-free encoding logic)

## Features

**Compare tab.** Type any string and see, side-by-side, what bytes every
supported encoding produces — ASCII, UTF-8 (with and without BOM),
UTF-16 LE/BE (with and without BOM), UTF-32 LE/BE, plus the legacy single-byte
code pages Windows-1252, ISO-8859-1, and IBM CP437. Each row shows byte count,
hex, Base64, and a short note describing the encoding's behaviour.

Toggles:
- **Interpret escapes** — type `\0`, `\n`, `\xFF`, `é`, `\U0001F389` and have
  them converted to their actual characters before encoding.
- **Show control characters** — render invisible C0 controls and DEL as
  Unicode Control Pictures (`␀ ␊ ␉ ␡ …`).
- **Append null terminator** — append U+0000 to the input, so the per-encoding
  table shows the null-terminator width per encoding (1 byte for UTF-8/ASCII,
  2 for UTF-16, 4 for UTF-32).

A **per-code-point breakdown** below the per-encoding table shows each Unicode
code point with its bytes in UTF-8 / UTF-16 LE/BE / UTF-32 LE side-by-side.
Right-click any row to copy.

**Convert tab.** Pick an encoding, then go either direction:
- String → Bytes: type text, get hex output.
- Bytes → String: paste hex (any common format — `48 65 6C`, `0x48,0x65,0x6C`,
  `48-65-6C`, `48656C`, `\x48\x65\x6C`) or Base64, get the decoded string.

**Inspect tab.** Strict-validate a byte sequence under a given encoding. Tells
you whether the bytes are well-formed and, if not, points at the exact byte
that broke. Seed buttons load common failure cases: invalid UTF-8 continuation
(`C3 28`), Modified-UTF-8 NUL (`C0 80`), lone UTF-16 surrogate.

**Learn tab.** In-app reference covering vocabulary (code point vs code unit
vs byte), each encoding's algorithm, BOMs, surrogate pairs, endianness, the
U+0000 / null-termination gotcha, and Modified UTF-8.

## Project layout

```
src/
  CharEncodingTool.Core/      class library — encoding logic, no UI deps
    Models/
    Services/
  CharEncodingTool.App/       WPF application
    ViewModels/
    Converters/               value converters for XAML bindings
tests/
  CharEncodingTool.Core.Tests/   xUnit tests on the core library
```

The `Core` project is intentionally independent of WPF so the same logic could
later be hosted in a MAUI app, a CLI, or a web frontend.

## Build and run

```powershell
dotnet build CharEncodingTool.slnx
dotnet test  CharEncodingTool.slnx
dotnet run --project src/CharEncodingTool.App
```

## Encoding notes

These are the gotchas this tool exists to make visible:

- **ASCII** is 7-bit. Anything > `U+007F` is replaced with `?` (`0x3F`). Useful
  for spotting that your device API only accepts ASCII when you're sending
  Latin characters.
- **UTF-8** is variable-length 1–4 bytes per code point. ASCII characters
  round-trip unchanged, which is why "Hello" produces identical bytes under
  ASCII and UTF-8. Non-ASCII characters take 2–4 bytes:
  - `é` (U+00E9) → `C3 A9` (2 bytes)
  - `世` (U+4E16) → `E4 B8 96` (3 bytes)
  - `🎉` (U+1F389) → `F0 9F 8E 89` (4 bytes)
- **BOM** (byte order mark, U+FEFF). Some tools require it, most modern HTTP
  APIs reject it. UTF-8's BOM is `EF BB BF`. UTF-16 LE BOM is `FF FE`, UTF-16
  BE BOM is `FE FF`.
- **UTF-16** stores BMP code points (U+0000–U+FFFF) in 2 bytes and uses a
  surrogate pair (4 bytes total) for anything above. `🎉` becomes the surrogate
  pair `D83C DF89`.
- **UTF-32** always uses 4 bytes per code point. Trivial to index but wasteful
  on the wire.
- **Endianness** matters for UTF-16 and UTF-32. `A` (U+0041) in UTF-16:
  - LE: `41 00` (low byte first)
  - BE: `00 41` (high byte first — network byte order)

## Why this exists

When debugging device API calls, the question "what bytes does this string
actually become?" comes up constantly. The compare view lets you eyeball a few
candidate encodings at once instead of writing a throwaway script every time.
