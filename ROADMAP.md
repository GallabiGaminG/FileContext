# FileContext Roadmap

## Purpose

FileContext is designed not only to manage files, but to manage the decisions around those files.

- Explorer moves the file.
- Everything finds the file.
- A duplicate finder detects matching files.

FileContext keeps the context needed to answer questions such as:

> Why is this file here?  
> What was I planning to do with it?  
> Where was it before?  
> Why does this duplicate exist?  
> What is the safest decision now?

---

## CURRENT / IMPLEMENTED

- .NET 8 WPF
- Local SQLite database
- Context entries
- Title / Path / Description
- Status
- Next action
- Tags
- Create new entry
- Edit
- Delete
- Open file / folder
- Native Windows file picker
- Native Windows folder picker
- Path validation
- Everything integration
- Bundled `tools\es.exe`
- Everything runtime dependency check
- Automatic background startup for Everything
- Combined Context + Everything search
- Add Everything search results directly to FileContext
- Search debounce
- Paginated result loading
- UI virtualization / recycling
- Responsive handling of large search result sets
- “Searching Everything...” status
- Resizable Context / Everything panes
- Path ellipsis + tooltip for long paths
- Storage Overview basic drive view
- Live per-drive Read / Write MB/s
- Active I/O process list
- PID + per-process I/O
- ETW-based Recent File I/O
- Real READ / WRITE path tracking
- Probable source → destination correlation
- Confidence scoring
- Correlation handling for `System / PID 4` write events
- Live telemetry Pause / Resume
- Initial selectable / copyable technical fields
- Background execution + overlap prevention for runtime UI-freeze issues

---

# 01 — STORAGE OVERVIEW / STORAGE MAP

## Goal

Show storage devices in a more meaningful way than only technical drive names.

### Information to display

- Drive letter
- Volume label
- Physical disk model
- Total capacity
- Used space
- Free space
- Usage percentage
- USB / SATA / NVMe
- HDD / SSD
- System disk / external disk
- Steam-like visual capacity bar

### Example

```text
22TB_DEPO (E:)
Toshiba MG10
Internal SATA HDD

Total: 20 TB
Used: 9.1 TB
Free: 10.9 TB
```

## Development Stages

### Storage Overview v1

- [x] Drive enumeration
- [x] Volume label
- [x] Total capacity
- [x] Used space
- [x] Free space
- [x] Usage percentage
- [x] Visual usage bar

### Storage Overview v1.1

- [x] Live disk activity
- [x] Read MB/s
- [x] Write MB/s
- [x] Periodic activity refresh
- [x] Move heavy WMI queries off the UI thread
- [x] Prevent recurring-task overlap

### Storage Overview v1.2

- [x] Active I/O process
- [x] Per-process disk usage
- [x] ETW-based real file READ / WRITE path tracking
- [x] Probable source → destination relationship
- [x] Confidence scoring
- [x] Handle `System / PID 4` write events in correlation
- [x] Never present inferred relationships as confirmed facts
- [x] Pause / Resume for inspecting fast-moving telemetry

### Remaining hardware identity layer

- [ ] Physical disk model
- [ ] HDD / SSD detection
- [ ] USB / SATA / NVMe detection
- [ ] System / external / removable classification
- [ ] Human-readable device identity

### Migration Assistant integration

- [ ] Confirmed source → destination
- [ ] Active file
- [ ] Progress
- [ ] Transfer speed
- [ ] ETA

---

# 02 — STORAGE CLASSIFICATION

Classify disk usage into meaningful categories.

## Storage categories

- System / Do Not Touch
- Applications
- User data
- Temp
- Cache
- Cleanable
- Archive
- To Move
- To Sort
- Suspected Duplicate
- Active Project
- Free Space

## Safety / decision classes

- SYSTEM / DO NOT TOUCH
- APPLICATION FILE
- USER DATA
- CACHE
- TEMP
- LOG
- ARCHIVE
- LEFTOVER / ORPHAN CANDIDATE
- SAFE TO REVIEW
- SAFE TO CLEAN
- UNKNOWN / USER REVIEW REQUIRED

## Goal

Do not only say:

> The disk is full.

Instead answer:

- Why is the disk full?
- What can be deleted?
- What should be moved?
- What should not be touched?
- Why was this item classified as cleanable, risky, or unknown?

---

# 03 — PROGRAM-BASED STORAGE ANALYSIS

## Goal

Show the actual disk footprint of an application.

## Where possible, include

- Installation files
- Program Files
- Program Files (x86)
- ProgramData
- AppData Local
- AppData Roaming
- Cache
- Temp
- Logs
- User content
- User-created project / content folders
- Related working directories
- Installer / update / download caches
- Related paths discovered from uninstall / registry metadata
- Installed application detection
- Uninstalled application leftovers
- Orphan / leftover candidate detection
- Total application footprint
- Explain why a file / folder is considered related
- Never silently delete leftovers

## Example

```text
DaVinci Resolve

Program: 4 GB
Cache: 310 GB
Projects: 18 GB
Other: 2 GB

Total disk footprint: 334 GB
```

If an application is no longer installed:

```text
Adobe Premiere Pro
Status: Not installed

Possible leftovers:
AppData / ProgramData / Cache / Logs ...

Classification:
LEFTOVER / ORPHAN CANDIDATE
```

Possible actions:

- Review
- Open Location
- Mark as Keep
- Mark as Cleanable

---

# 04 — CONTEXT / PROVENANCE EXPANSION

Expand the existing Context core.

- Old path
- New path
- PreviousPath
- SourceDrive
- TargetDrive
- MovedAt
- MoveReason
- Provenance / migration history

Logical history should remain available even if the physical file is moved.

---

# 05 — DUPLICATE ANALYZER

Duplicate analysis should be performed in stages.

## Stage 1 — fast candidate detection

- File size
- Name
- Date
- Extension

## Stage 2

- Partial hash when needed

## Stage 3

- Full hash for final verification

## Duplicate group actions

- Keep
- Delete
- Ignore
- Keep Both

Provenance should remain even if physical duplicate files are removed.

---

# 06 — MIGRATION / CONSOLIDATION ASSISTANT

## Real-world workflow

```text
Old disks
→ 22TB_DEPO\_To_Sort
→ Duplicate check
→ Classification
→ New permanent path
→ Update old path references
→ Provenance record
```

## Track

- Source disk
- Old path
- Temporary path
- New path
- Status
- Migration date
- Migration reason
- Duplicate status
- Conflict status

For FileContext-managed migrations, eventually show:

- Confirmed source → destination
- Active file
- Progress
- Transfer speed
- ETA

---

# 07 — CONFLICT INBOX

When two files have the same name but different content, FileContext should not automatically overwrite one of them.

## Options

- Decide now
- Safe backup and continue
- Skip

The conflict can later be reviewed in the Conflict Inbox.

## For text files

- Diff

## For binary files

- Size
- Date
- Hash

Both source and destination versions may be retained.

When both versions are kept, prefer provenance-aware names instead of generic names such as `(1)`.

---

# 08 — SAFE BACKUP POLICY

Prevent conflict backup storage from growing without control.

## Settings

- Maximum safe backup size
- Backup quota
- Warning when quota is nearly full
- Retention period
- Cleanup policy for old conflict backups
- Warning for large backup operations

## Example options

- 50 GB
- 100 GB
- 500 GB
- Custom

---

# 09 — SAFE EJECT / DEVICE USAGE DIAGNOSTICS

Analyze device usage before removing an external drive.

Windows:

> Device is in use.

FileContext:

> Which process is using it, and why?

## Show where possible

- Process name
- PID
- Related path
- Open file handle
- Open directory handle
- Active disk I/O
- Read / write activity

## Status levels

### RED

- Active write operation
- Active copy operation
- Migration in progress
- Do not disconnect

### YELLOW

- Open handle
- Explorer / terminal / application is holding the device
- Suspicious activity

### GREEN

- FileContext did not detect active usage

Important:

> A green state does not mean guaranteed safe removal.  
> Windows remains the final authority for eject approval.

## Actions

- Show process
- Show location
- Check again
- Retry safe eject

If FileContext knows about an active migration:

```text
Steam Library Migration is still running.
Source: G:
Destination: E:
Do not disconnect this drive.
```

The existing disk telemetry + ETW infrastructure provides groundwork for this module.

---

# 10 — LINK / JUNCTION / SHORTCUT SUPPORT

If moving a file would break an old path, FileContext may suggest:

- Shortcut
- Junction
- Symlink

But it should clearly warn about:

- Permissions
- Compatibility
- Administrator requirements
- Risk

---

# 11 — POWER USER SETTINGS

Expose advanced behavior to power users.

## Search

- Everything result limit
- Page size
- Search debounce
- Scan scope
- Virtualization behavior

## Duplicate

- Hash strategy
- Duplicate depth
- Partial hash on / off
- Full hash threshold

## Migration

- Default conflict behavior
- Safe backup quota
- Backup retention
- Default target rules
- Junction suggestions

## Safe Eject

- Handle scan on / off
- Disk I/O observation period
- Safe eject scan depth
- Confirmation level

## Telemetry / Live Monitoring

- Refresh interval
- Pause / Resume behavior
- Event history limit
- Correlation confidence threshold
- ETW monitoring on / off

## General

- Confirmation level for risky actions
- Log / diagnostic detail level

---

# 12 — MAIN NAVIGATION

The current single-screen layout should not be forced to contain every feature.

## Planned main modules

- Context
- Storage
- Duplicates
- Migration
- Settings

## Optional additional modules

- Conflicts
- Devices

---

# 13 — PRIVILEGED HELPER / LEAST PRIVILEGE

The main application should not run as Administrator unless necessary.

- `FileContext.exe` runs as a normal user.
- `FileContext.ElevatedHelper.exe` requests UAC only when required.
- ETW / privileged handle / device operations run through the helper.
- The helper should remain small, auditable, and short-lived.
- Normal Context / Search / Storage / Duplicate operations should not require elevation.

---

# 14 — UI / VISUAL POLISH

After the core functionality is stable.

- Gumball blue → Darwin orange gradient
- Steam-like disk usage bars
- Status colors
- Visual separation for risk / safe / temp states
- Disk cards
- More consistent spacing / typography
- App icon

## Selectable / Copyable Technical Text

Technical information that may need to be reused should be selectable / copyable:

- Path
- PID
- Process name
- Disk information
- Read / Write values
- Hash / diagnostic details
- Context path

For fast-moving telemetry:

- Pause / Resume
- Optional future Snapshot / Freeze Selected Event

---

# DEVELOPMENT PRINCIPLES

- Local-first
- No runtime AI dependency required
- Prefer deterministic behavior
- No silent overwrite
- No silent delete
- Preserve rollback whenever uncertainty exists
- Preserve provenance whenever possible
- Do not force the user into immediate decisions
- Show risk clearly
- FileContext should explain what it knows whenever possible
- Final decisions remain with the user
- Heavy work → background
- UI update → UI thread
- Recurring task → prevent overlap
- Large result → pagination + virtualization
- Least privilege
- Never present inferred information as confirmed fact
