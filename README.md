# nineth1ngs

A small Windows desktop app for capturing, prioritizing, and tracking the things that need doing — without turning them into a project of their own.

> **Quick capture. Clear priorities. Simple time tracking.**

## Why nineth1ngs?

I wanted a lightweight place for the small and medium-sized things that come up during the day.

Not a full project management suite.  
Not a board with ten workflows, labels, assignees, and dashboards.  
Just a fast way to write something down, keep the important things at the top, break work into smaller steps, and track the time spent on it.

`nineth1ngs` is built around that idea: **reduce the friction between remembering something and actually getting it done.**

## Features

- **Quick th1ng capture** — add a new th1ng and press Enter
- **Global shortcut** — `Ctrl + Alt + N` brings nineth1ngs to the front and focuses the input field
- **Drag & drop prioritization** — reorder open th1ngs freely
- **Subth1ngs** — break a th1ng into smaller steps
- **Built-in timer** — track time directly on a th1ng
- **Automatic timer safety** — running timers are paused and saved when the app closes
- **Copy tracked time** — copy tracked time in a configurable format
- **Billing-style rounding** — configure interval and round-up threshold
- **DONE view** — completed th1ngs stay accessible and can be reopened
- **Inline editing** — double-click a th1ng to edit it
- **Local persistence** — your data stays on your machine
- **Compact always-on-top UI** — designed to stay nearby without taking over the desktop

## Getting nineth1ngs

nineth1ngs currently targets **Windows**.

### Download

1. Open the **Releases** section of this repository.
2. Download the latest Windows release.
3. Place the executable wherever you want to keep it.
4. Start `nineth1ngs.exe`.

The release is intended to be self-contained, so a separate .NET installation should not be required.

> Windows may show a SmartScreen warning for unsigned builds. If you downloaded nineth1ngs from the official repository, you can review the warning and choose whether to run it.

## Using nineth1ngs

### Add a th1ng

Type into the input field at the bottom and press **Enter**.

You can also press:

```text
Ctrl + Alt + N
```

from anywhere while nineth1ngs is running. The app will come to the front and focus the new-th1ng input.

### Prioritize

Drag open th1ngs up or down to arrange them in the order that matters to you.

The order is persisted between sessions.

### Add subth1ngs

Click a th1ng to expand it, then use:

```text
+ add
```

to add smaller steps underneath it.

### Edit

Double-click the text of a th1ng or subth1ng to edit it.

- **Enter** saves
- **Escape** cancels

### Track time

Use the play button next to a th1ng to start its timer.

Only top-level th1ngs track time. When a th1ng is completed or the app closes, a running timer is stopped and its elapsed time is preserved.

Click the displayed tracked time to copy it.

### Configure copied time

The settings page lets you configure how tracked time is copied, including:

- billing interval
- round-up threshold
- decimal hours
- hours and minutes

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
dotnet publish .\nineth1ngs.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Project status

nineth1ngs is an actively developed personal project.

The first release focuses on getting the core workflow right:

```text
capture → prioritize → work → track → complete
```

Everything beyond that should earn its place.
