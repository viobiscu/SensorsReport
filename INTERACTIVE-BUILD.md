# Interactive Build Scripts

## Overview

The main directory contains interactive build scripts (`build.sh` and `build.cmd`) that provide a user-friendly interface for executing build targets across individual projects or all projects at once.

## Features

### 🌟 All Projects Support
- Execute any target across all projects in the solution
- Progress indicators for each project
- Summary reporting with success/failure counts
- Smart skipping for projects without required files (e.g., Dockerfile)

### 📋 Project Selection Menu
```
Available projects:
===================
 0) 🌟 All Projects
 1) Sensors-Report-MQTT-to-Orion
 2) Sensors-Report-Workflow.API
 3) SensorsReport.Alarm.API
 4) SensorsReport.Api.Core
 5) SensorsReport.Audit.API
 6) SensorsReport.Audit
 7) SensorsReport.Business.Broker.API
 8) SensorsReport.Email.API
 9) SensorsReport.Email.Consumer
10) SensorsReport.Provision.API
11) SensorsReport.SMS.API
12) SensorsReport.Swagger.API
```

### 🎯 Action Selection Menu
```
Available actions:
==================
1) Build project (dotnet build)
2) Increment version (dotnet build -t:IncrementVersion)
3) Copy flux files (dotnet build -t:CopyFluxFiles)
4) Build Docker image (dotnet build -t:BuildDocker)
5) Build and Push Docker (dotnet build -t:BuildAndPushDocker)
```

## Usage

### Linux/macOS
```bash
./build.sh
```

### Windows
```cmd
build.cmd
```

## Example Output

### Single Project Execution
```bash
✅ Selected: SensorsReport.Api.Core

==========================================
Executing: increment for SensorsReport.Api.Core
==========================================
📈 Incrementing version...
Setting API version to: 1.0.19
✅ Version incremented only

✅ Action completed successfully!
```

### All Projects Execution
```bash
✅ Selected: All Projects

==========================================
Executing: increment for All Projects
==========================================

🔄 Processing: SensorsReport.Api.Core
----------------------------------------
📈 Incrementing version...
✅ SensorsReport.Api.Core completed successfully

🔄 Processing: SensorsReport.Alarm.API
----------------------------------------
📈 Incrementing version...
✅ SensorsReport.Alarm.API completed successfully

... (continues for all projects)

==========================================
📊 Summary: 11 succeeded, 1 failed
==========================================
```

## Error Handling

- **Individual projects**: Stops execution and reports error
- **All projects mode**: Continues processing remaining projects and provides summary
- **Missing requirements**: Smart skipping with warnings (e.g., no Dockerfile found)
- **Invalid selections**: Clear error messages with exit

## Target Commands

The scripts execute these exact MSBuild targets:

| Action | Command |
|--------|---------|
| Build project | `dotnet build` |
| Increment version | `dotnet build -t:IncrementVersion` |
| Copy flux files | `dotnet build -t:CopyFluxFiles` |
| Build Docker image | `dotnet build -t:BuildDocker` |
| Build and Push Docker | `dotnet build -t:BuildAndPushDocker` |

## Advantages over Batch Scripts

1. **Interactive selection** - Choose specific projects and actions
2. **All projects option** - Execute across entire solution
3. **Progress tracking** - Real-time feedback for each project
4. **Error resilience** - Continue processing even if some projects fail
5. **Smart filtering** - Skip inappropriate actions (e.g., Docker on projects without Dockerfile)
6. **Summary reporting** - Clear overview of batch operation results
