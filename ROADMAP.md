\# FileContext Roadmap



\## Purpose



FileContext is designed not only to manage files, but to manage the decisions around those files.



Explorer moves the file.

Everything finds the file.

A duplicate finder detects matching files.



FileContext keeps the context needed to answer questions such as:



> Why is this file here?

> Which project does it belong to?

> What was I planning to do with it?

> Where did it come from?

> Why does this duplicate exist?

> What is the safest decision now?





==================================================

CURRENT / IMPLEMENTED

==================================================



\- .NET 8 WPF

\- Local SQLite database

\- Context entries

\- Title / Path / Description

\- Status

\- Next action

\- Tags

\- Create new entry

\- Edit

\- Delete

\- Open file / folder

\- Native Windows file picker

\- Native Windows folder picker

\- Path validation

\- Everything integration

\- Bundled tools\\es.exe

\- Everything runtime dependency check

\- Automatic background startup for Everything

\- Combined Context + Everything search

\- Add Everything search results directly to FileContext

\- Search debounce

\- Paginated result loading

\- UI virtualization / recycling

\- Responsive handling of large search result sets

\- “Searching Everything...” status

\- Resizable Context / Everything panes

\- Path ellipsis + tooltip for long paths





==================================================

01 — STORAGE OVERVIEW / STORAGE MAP

==================================================



Current active development stage.



Goal:

Show storage devices in a more meaningful way than only technical drive names.



Information to display:



\- Drive letter

\- Volume label

\- Physical disk model

\- Total capacity

\- Used space

\- Free space

\- Usage percentage

\- USB / SATA / NVMe

\- HDD / SSD

\- System disk / external disk

\- Steam-like visual capacity bar



Example:



22TB\_DEPO (E:)

Toshiba MG10

Internal SATA HDD



Total: 20 TB

Used: 9.1 TB

Free: 10.9 TB





\### Development Stages



\#### Storage Overview v1

\- \[x] Drive enumeration

\- \[x] Volume label

\- \[x] Total capacity

\- \[x] Used space

\- \[x] Free space

\- \[x] Usage percentage

\- \[x] Visual usage bar



\#### Storage Overview v1.1

\- \[ ] Live disk activity

\- \[ ] Read MB/s

\- \[ ] Write MB/s

\- \[ ] Periodic activity refresh



\#### Storage Overview v1.2

\- \[ ] Active I/O process

\- \[ ] Per-process disk usage

\- \[ ] Probable source → destination relationship

\- \[ ] Never present inferred relationships as confirmed facts



\#### Migration Assistant integration

\- \[ ] Confirmed source → destination

\- \[ ] Active file

\- \[ ] Progress

\- \[ ] Transfer speed

\- \[ ] ETA





==================================================

02 — STORAGE CLASSIFICATION

==================================================



Classify disk usage into meaningful categories.



\- System / Do Not Touch

\- Applications

\- User data

\- Temp

\- Cache

\- Cleanable

\- Archive

\- To Move

\- To Sort

\- Suspected Duplicate

\- Active Project

\- Free Space



The goal is not only to say:



“The disk is full.”



But to answer:



“Why is the disk full?”

“What can be deleted?”

“What should be moved?”

“What should not be touched?”





==================================================

03 — PROGRAM-BASED STORAGE ANALYSIS

==================================================



Goal:

Show the actual disk footprint of an application.



Where possible, include:



\- Installation files

\- Program Files

\- ProgramData

\- AppData

\- Cache

\- Temp

\- Logs

\- User content

\- Related working directories



Example:



DaVinci Resolve



Program: 4 GB

Cache: 310 GB

Projects: 18 GB

Other: 2 GB



Total disk footprint: 334 GB





==================================================

04 — CONTEXT / PROVENANCE EXPANSION

==================================================



Expand the existing Context core.



\- Old path

\- New path

\- PreviousPath

\- SourceDrive

\- TargetDrive

\- MovedAt

\- MoveReason

\- Provenance / migration history



Logical history should remain available even if the physical file is moved.





==================================================

05 — DUPLICATE ANALYZER

==================================================



Duplicate analysis should be performed in stages.



Stage 1:

\- File size

\- Name

\- Date

\- Extension



Stage 2:

\- Partial hash when needed



Stage 3:

\- Full hash for final verification



Duplicate group actions:



\- Keep

\- Delete

\- Ignore

\- Keep Both



Provenance should remain even if physical duplicate files are removed.





==================================================

06 — MIGRATION / CONSOLIDATION ASSISTANT

==================================================



Real-world workflow:



Old disks

→ 22TB\_DEPO\\\_To\_Sort

→ Duplicate check

→ Classification

→ New permanent path

→ Update old path references

→ Provenance record



Track:



\- Source disk

\- Old path

\- Temporary path

\- New path

\- Status

\- Migration date

\- Migration reason

\- Duplicate status

\- Conflict status





==================================================

07 — CONFLICT INBOX

==================================================



When two files have the same name but different content, FileContext should not automatically overwrite one of them.



Options:



\- Decide now

\- Safe backup and continue

\- Skip



The conflict can later be reviewed in the Conflict Inbox.



For text files:

\- Diff



For binary files:

\- Size

\- Date

\- Hash



Both source and destination versions may be retained.



When both versions are kept, prefer meaningful provenance-aware names instead of generic names such as “(1)”.





==================================================

08 — SAFE BACKUP POLICY

==================================================



Prevent conflict backup storage from growing without control.



Settings:



\- Maximum safe backup size

\- Backup quota

\- Warning when quota is nearly full

\- Retention period

\- Cleanup policy for old conflict backups

\- Warning for large backup operations



Example options:



50 GB

100 GB

500 GB

Custom





==================================================

09 — SAFE EJECT / DEVICE USAGE DIAGNOSTICS

==================================================



Analyze device usage before removing an external drive.



Windows:

“Device is in use.”



FileContext:

“Which process is using it, and why?”



Show where possible:



\- Process name

\- PID

\- Related path

\- Open file handle

\- Open directory handle

\- Active disk I/O

\- Read / write activity



Status levels:



RED

\- Active write operation

\- Active copy operation

\- Migration in progress

\- Do not disconnect



YELLOW

\- Open handle

\- Explorer / terminal / application is holding the device

\- Suspicious activity



GREEN

\- FileContext did not detect active usage



Important:

A green state does not mean guaranteed safe removal.

Windows remains the final authority for eject approval.



Actions:



\- Show process

\- Show location

\- Check again

\- Retry safe eject



If FileContext knows about an active migration, provide a more meaningful message:



“Steam Library Migration is still running.

Source: G:

Destination: E:

Do not disconnect this drive.”





==================================================

10 — LINK / JUNCTION / SHORTCUT SUPPORT

==================================================



If moving a file would break an old path, FileContext may suggest:



\- Shortcut

\- Junction

\- Symlink



But it should clearly warn about:



\- Permissions

\- Compatibility

\- Administrator requirements

\- Risk





==================================================

11 — POWER USER SETTINGS

==================================================



Expose advanced behavior to power users.



Search:



\- Everything result limit

\- Page size

\- Search debounce

\- Scan scope

\- Virtualization behavior



Duplicate:



\- Hash strategy

\- Duplicate depth

\- Partial hash on/off

\- Full hash threshold



Migration:



\- Default conflict behavior

\- Safe backup quota

\- Backup retention

\- Default target rules

\- Junction suggestions



Safe Eject:



\- Handle scan on/off

\- Disk I/O observation period

\- Safe eject scan depth

\- Confirmation level



General:



\- Confirmation level for risky actions

\- Log / diagnostic detail level





==================================================

12 — MAIN NAVIGATION

==================================================



The current single-screen layout should not be forced to contain every feature.



Planned main modules:



\- Context

\- Storage

\- Duplicates

\- Migration

\- Settings



Optional additional modules:



\- Conflicts

\- Devices





==================================================

13 — UI / VISUAL POLISH

==================================================



After the core functionality is stable.



\- Gumball blue → Darwin orange gradient

\- Steam-like disk usage bars

\- Status colors

\- Visual separation for risk / safe / temp states

\- Disk cards

\- More consistent spacing / typography

\- App icon





==================================================

DEVELOPMENT PRINCIPLES

==================================================



\- Local-first

\- No runtime AI dependency required

\- Prefer deterministic behavior

\- No silent overwrite

\- No silent delete

\- Preserve rollback whenever uncertainty exists

\- Preserve provenance whenever possible

\- Do not force the user into immediate decisions

\- Show risk clearly

\- FileContext should explain what it knows whenever possible

\- Final decisions remain with the user







\### Development Stages



\#### Storage Overview v1

\- \[x] Drive enumeration

\- \[x] Volume label

\- \[x] Total capacity

\- \[x] Used space

\- \[x] Free space

\- \[x] Usage percentage

\- \[x] Visual usage bar



\#### Storage Overview v1.1

\- \[x] Live disk activity

\- \[x] Read MB/s

\- \[x] Write MB/s

\- \[x] Periodic activity refresh



\#### Storage Overview v1.2

\- \[x] Active I/O process

\- \[x] Per-process disk usage

\- \[x] Probable source → destination relationship

\- \[x] Never present inferred relationships as confirmed facts



\#### Migration Assistant integration

\- \[ ] Confirmed source → destination

\- \[ ] Active file

\- \[ ] Progress

\- \[ ] Transfer speed

\- \[ ] ETA

