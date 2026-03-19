# YTS Downloader

> Forked from [jim60105/YoutubeSegmentDownloader](https://github.com/jim60105/YoutubeSegmentDownloader) (archived August 2024). See [Changes from upstream](#changes-from-upstream) below.

A free, open-source media segment downloader and processing utility. Extract precise time-range segments from video content you are authorized to access -- including your own uploads, licensed media, public-domain works, and Creative Commons content -- with frame-accurate cutting powered by FFmpeg.

![Windows](https://img.shields.io/static/v1?style=for-the-badge&message=Windows&color=0078D6&logo=Windows&logoColor=FFFFFF&label=)
![.NET 8.0](https://img.shields.io/static/v1?style=for-the-badge&message=.NET+8.0&color=512BD4&logo=.NET&logoColor=FFFFFF&label=)
![GitHub](https://img.shields.io/github/license/jamie-bear/YTS-Downloader?style=for-the-badge)

> Windows x64 only. Requires .NET 8.0 Runtime.

## Acceptable Use

This software is intended for lawful use only. Users are responsible for complying with applicable copyright law, license terms, and platform terms of service. This project does not endorse or encourage copyright infringement.

Intended use cases include:

- Downloading and trimming **your own uploads** or content you created
- Processing media you have **explicit permission** to use
- Working with **public-domain**, **Creative Commons**, or other openly licensed content
- Offline access for **research, accessibility, archival, or QA** where you hold the necessary rights
- Segment extraction and trimming for **authorized editing workflows**

## Features

- **Precise segment extraction** - Specify a start and end time to extract only the portion you need, with frame-accurate re-encoding
- **Automatic dependency management** - yt-dlp and FFmpeg are downloaded and updated automatically at startup
- **Metadata embedding** - Source metadata (title, creator, date, tags, source URL) is embedded into output MP4 files
- **Customizable file naming** - Tag-based output filename patterns using tokens like `[video creator]`, `[upload date]`, `[video title]`, `[video segment]`, etc.
- **Format selection** - Specify yt-dlp format codes for fine-grained control over source quality
- **Browser session import** - Authenticate with your browser session for accessing content that requires login
- **Multi-site support** - Compatible with any [yt-dlp supported site](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md)
- **Dark theme UI** - Modern, minimalist dark interface
- **I18n** - English and Chinese interfaces

## Install

Download the latest release from [Releases](https://github.com/jamie-bear/YTS-Downloader/releases/latest).

**Option A: Setup installer** (`setup.exe`)
- Installs .NET 8.0 Runtime automatically
- Creates Desktop and Start Menu shortcuts
- Auto-checks for updates on launch

**Option B: Standalone executable** (`YTSDownloader.exe`)
- Requires .NET 8.0 Runtime pre-installed

### Prerequisites

- [Deno](https://deno.com/) runtime must be installed (required by yt-dlp for certain sites)

## How it works

When extracting a segment via stream copy, FFmpeg seeks to the nearest keyframe *before* the requested start time, resulting in imprecise cuts with corrupted leading frames. YTS Downloader solves this with a two-pass approach:

```
yt-dlp → download segment (stream copy)
  ↓
ffmpeg -sseof → re-encode from end offset (frame-accurate)
  ↓
Clean MP4 output with precise start/end times
```

Output is encoded to H.264/AAC MP4 with configurable quality settings.

## Changes from upstream

This fork includes the following changes from the original [jim60105/YoutubeSegmentDownloader](https://github.com/jim60105/YoutubeSegmentDownloader):

1. **Tag-based file naming** - Customizable output filename patterns using configurable tag tokens
2. **Metadata embedding** - Source metadata automatically embedded into downloaded MP4 files
3. **Fixed naming/settings overlap** - Resolved UI issue where naming settings overlapped with other input fields
4. **CI/CD improvements** - Automated releases, pull request build triggers
5. **App renamed to YTS Downloader** - Differentiated from the archived upstream repository
6. **Modern dark theme UI** - Redesigned interface with a dark color scheme

## LICENSE

> [!NOTE]
> This software uses **FFmpeg** licensed under the **GPLv3**.
> FFmpeg binary distributions will be downloaded from [here](https://github.com/yt-dlp/FFmpeg-Builds/releases/latest).
> FFmpeg source code can be found [here](https://github.com/FFmpeg/FFmpeg/commit/390d6853d0).
>
> This software uses **yt-dlp** licensed under the **Unlicense License**.
> yt-dlp binary distribution will be downloaded from [here](https://github.com/yt-dlp/yt-dlp/releases/latest).
>
> This software uses **Xabe.FFmpeg** licensed under the [License Agreement](https://ffmpeg.xabe.net/license.html) with **Non-commercial use** - Attribution-NonCommercial-ShareAlike 3.0 Unported (CC BY-NC-SA 3.0).
>
> This software uses **Beautiful Flat Icons** licensed under the **GPLv2**.
> Icon source can be found [here](https://www.elegantthemes.com/blog/freebie-of-the-week/beautiful-flat-icons-for-free).

[GNU GENERAL PUBLIC LICENSE Version 3](LICENSE)

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
