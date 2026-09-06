# nineth1ngs

A small Windows desktop app for capturing, prioritizing, and tracking the things that need doing — without turning them into a project of their own.

> **Quick capture. Clear priorities. Simple time tracking.**

## Why nineth1ngs?

I wanted a lightweight place for the small and medium-sized things that come up during the day.

Not a full project management suite.  
Not a board with ten workflows, labels, assignees, and dashboards.  
Just a fast way to write something down, keep the important things in order, break work into smaller steps, and track the time spent on it.

`nineth1ngs` is built around that idea: **reduce the friction between remembering something and actually getting it done.**

## Features

- **Quick th1ng capture** — add a new th1ng and press Enter
- **Global quick add** — `Ctrl + Alt + N` opens the quick input from anywhere while nineth1ngs is running
- **Drag & drop prioritization** — reorder open th1ngs freely
- **Subth1ngs** — break a th1ng into smaller steps
- **Parent and subth1ng timers** — track total time on a th1ng and optionally split that time across individual subth1ngs
- **Mini Mode** — compact always-on-top timer view for keeping the current th1ng close without the full window
- **Keyboard navigation in Mini Mode** — switch between open th1ngs without leaving the compact view
- **Session-aware time tracking** — Windows lock pauses active timers automatically and resumes them after unlocking
- **Away-time review** — assign time spent away to an existing th1ng, create a new th1ng directly from the review, or discard the time
- **Subth1ng-aware away time** — if a subth1ng was active before Windows was locked, assigning the away time to its parent also adds that time to the active subth1ng
- **Automatic timer safety** — running timers are paused and saved when the app closes
- **Copy tracked time** — copy tracked time in a configurable format
- **Billing-style rounding** — configure interval and round-up threshold
- **DONE view** — completed th1ngs stay accessible and can be reopened
- **Inline editing** — double-click a th1ng or subth1ng to edit it
- **Local persistence** — your data stays on your machine
- **Always-on-top UI** — designed to stay nearby without taking over the desktop

## Global shortcuts

```text
Ctrl + Alt + N       Quick add th1ng
Ctrl + Alt + Space   Start / pause selected timer
Ctrl + Alt + Left    Previous th1ng in Mini Mode
Ctrl + Alt + Right   Next th1ng in Mini Mode
```

## Getting nineth1ngs

nineth1ngs currently targets **Windows**.

### Download

1. Open the **Releases** section of this repository.
2. Download the latest Windows x64 release ZIP.
3. Extract the ZIP to a folder of your choice.
4. Start `nineth1ngs.exe`.

The release is self-contained, so a separate .NET installation should not be required.

> Windows may show a SmartScreen warning for unsigned builds. If you downloaded nineth1ngs from the official repository, you can review the warning and choose whether to run it.

## Using nineth1ngs

### Add a th1ng

Type into the input field at the bottom and press **Enter**.

You can also press:

```text
Ctrl + Alt + N
```

from anywhere while nineth1ngs is running.

In normal mode, the app comes to the front and focuses the new-th1ng input. In Mini Mode, the compact quick input opens directly inside the Mini Mode window.

### Prioritize

Drag open th1ngs up or down to arrange them in the order that matters to you.

The order is persisted between sessions.

### Add subth1ngs

Click a th1ng to expand it, then use:

```text
+ add
```

to add smaller steps underneath it.

Subth1ngs are kept in creation order, so newer steps are added at the bottom of the list.

### Edit

Double-click the text of a th1ng or subth1ng to edit it.

- **Enter** saves
- **Escape** cancels

### Track time

Use the play button next to a th1ng to start its timer.

A top-level th1ng tracks the total time spent on that th1ng.

Subth1ngs can also track time. Starting a subth1ng timer starts its parent timer automatically if necessary. Only one subth1ng timer can run at a time, while the parent timer continues to represent the overall time spent on the th1ng.

Pausing a parent pauses its active subth1ng as well.

When a th1ng is completed or the app closes, running timers are paused and their elapsed time is preserved.

Click the displayed tracked time on a top-level th1ng to copy it.

### Mini Mode

Mini Mode provides a compact always-on-top view containing the current th1ng, its timer, and the main timer control.

Use:

```text
Ctrl + Alt + Left
Ctrl + Alt + Right
```

to move between open th1ngs.

Use:

```text
Ctrl + Alt + Space
```

to start or pause the currently selected th1ng.

`Ctrl + Alt + N` opens the quick input directly inside Mini Mode.

### Windows lock and away time

If Windows is locked while a timer is running, nineth1ngs pauses the active timer at the lock time.

After unlocking:

1. The previously running timer resumes.
2. If a subth1ng was active, its timer resumes as well.
3. nineth1ngs asks what should happen to the time spent away.

The away time can be:

- assigned to an existing open th1ng
- assigned to a newly created th1ng
- discarded

If a subth1ng was active before Windows was locked and you assign the away time to its parent, the away time is added to **both the parent and that subth1ng**.

### Configure copied time

The settings page lets you configure how tracked time is copied, including:

- billing interval
- round-up threshold
- decimal hours
- hours and minutes

Any non-zero tracked duration is rounded to at least one billing interval when copied.

This is useful when the tracked time needs to be transferred into another system.

### Complete and reopen

Completing a th1ng moves it to **DONE**.

Completed th1ngs remain available there and can be reopened when needed.

## Data

nineth1ngs stores its data locally.

Application data and settings are stored under:

```text
%LOCALAPPDATA%\nineth1ngs
```

No account or cloud service is required.

## Roadmap

nineth1ngs is intentionally small, but there are a few directions I would like to explore.

### Configurable shortcuts

Global shortcuts currently use fixed defaults. A future version could make them configurable, including conflict detection and restoring defaults.

### Appearance

Possible future appearance options include:

- light mode
- custom main, secondary, and accent colors
- additional UI preferences

### Optional time tracking

A future setting could disable time tracking entirely for users who only want task capture and prioritization.

When disabled, timer controls and timer-related shortcuts would be hidden or inactive while previously tracked data remains preserved.

### Archive

The DONE section is useful for recently completed work, but over time it should not become an endless list.

A future archive could move older completed th1ngs out of the active DONE view while keeping them searchable and accessible.

### Time insights

The timer already captures useful information. A future version could turn that into simple insights such as:

- time spent per day or week
- recently tracked th1ngs
- total time spent on completed work
- lightweight summaries and trends

The goal would be useful reflection without turning nineth1ngs into a complex reporting tool.

### More ideas

Possible future improvements include better search and filtering, additional keyboard-driven workflows, and further polish around daily use.

The guiding rule stays the same: **new features should make the app faster or clearer, not heavier.**

## Development

nineth1ngs is built with:

- C#
- .NET 10
- WPF
- Entity Framework Core
- SQLite
- CommunityToolkit.Mvvm

To run the project locally:

```powershell
git clone <repository-url>
cd nineth1ngs
dotnet restore
dotnet run
```

To run the tests:

```powershell
dotnet test
```

To create a self-contained Windows x64 release:

```powershell
dotnet publish .\nineth1ngs.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false
```

## Project status

nineth1ngs is an actively developed personal project.

The current release focuses on keeping the core workflow fast while adding compact time tracking and better handling for interruptions:

```text
capture → prioritize → work → track → review → complete
```

Everything beyond that should earn its place.
