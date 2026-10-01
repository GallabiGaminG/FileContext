# FileContext

FileContext is a local Windows utility for attaching human context to files and folders.

Everything answers:

> Where is this file?

FileContext answers:

> Why is it here, what project does it belong to, and what was I planning to do with it?

## Features

- Local SQLite database
- Context notes for files and folders
- Status tracking
- Next-action tracking
- Tags
- Search across context metadata
- Everything filesystem search integration
- Add Everything search results directly to FileContext
- Open, edit and delete context entries
- Native Windows file and folder pickers
- Path validation
- Paginated Everything results
- Virtualized result rendering
- Resizable context and search panes

## Requirements

- Windows
- .NET 8
- Everything by voidtools

FileContext includes the Everything Command-line Interface (`es.exe`) used to query
the Everything index.

Everything itself must be installed. If Everything is installed but not currently
running, FileContext attempts to start it in the background.

## Data

FileContext stores its local database at:

`%LOCALAPPDATA%\FileContext\FileContext.db`

No cloud service or runtime AI service is required.

## Third-party software

See `THIRD_PARTY_NOTICES.txt`.

## Development note

This project was developed with AI-assisted coding and design discussions using ChatGPT.
