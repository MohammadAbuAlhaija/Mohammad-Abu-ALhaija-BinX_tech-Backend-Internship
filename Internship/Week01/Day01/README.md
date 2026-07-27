
## 📅 Week 1 — Day 1: Program Orientation & .NET Development Environment Setup

### Overview
The internship started on Day 1 with its Phase 1 (Onboarding & Foundations). The emphasis was on gaining an overview of the program and establishing a working, professional .NET development environment before writing any code or using a web framework.

### What I Learned
- **Program Orientation**: Covered the four phases of the program, the weekly schedule, the mentor check-in schedule, and an example of a successful Phase 3 capstone project.
- **.NET SDK Installation**: Installed the current .NET LTS SDK, which includes the runtime, compiler, and the `dotnet` CLI. Tested with:
  ```bash
  dotnet --version
  dotnet --list-sdks
  ```
- **The `dotnet` CLI**: Learned the basic commands used throughout the program:
  - `dotnet new console` — scaffold a new console application
  - `dotnet run` — build and run the project in one step
  - `dotnet build` — compile without executing
- **IDE Setup**: Installed and configured Visual Studio Code (using the C# Dev Kit extension), enabling IntelliSense and the debugger — both essential for the coming weeks.

### Hands-On Lab
1. Installed the .NET LTS SDK and confirmed it with `dotnet --version`.
2. Configured the IDE with debugging enabled.
3. Created a new console app:
   ```bash
   dotnet new console -o HelloBinX
   dotnet run
   ```
4. Modified the program to print my name and today's date, then rebuilt and reran it.
5. Created this GitHub repository to track the internship's progress.

### Tools Used
`.NET SDK` • `Visual Studio Code (C# Dev Kit)` • `Terminal` • `GitHub`

### Resources
- [Environment Setup Video](https://www.youtube.com/watch?v=ZVGutgqBMUM)

### Status
✅ .NET SDK installed and verified
✅ IDE configured with debugging
✅ First console app ("Hello World") built and run successfully
✅ GitHub repository created

---
*More daily summaries will be added as Week 1 progresses.*