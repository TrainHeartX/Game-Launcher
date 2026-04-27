# Phase 2 Quick Start Guide

## 🎯 Phase 2 Goal: Business Logic Implementation

Build the services layer that will:
- Launch games with emulators
- Track play statistics
- Manage game CRUD operations
- Handle settings

---

## 📋 Prerequisites (Already Complete ✅)

- [x] Solution structure
- [x] Data models (Game, Platform, Emulator, etc.)
- [x] XML parser (XmlDataContext)
- [x] Caching system (GameCacheManager)
- [x] Tests passing (6/6)

---

## 🚀 Step-by-Step Implementation Plan

### Step 1: Create Services Directory

```bash
cd /h/GameLauncher/src/Core/GameLauncher.Infrastructure
mkdir Services
```

### Step 2: Implement EmulatorLauncher

**File**: `src/Core/GameLauncher.Infrastructure/Services/EmulatorLauncher.cs`

**Purpose**: Launch games with proper emulator configuration

**Key Features**:
- Build command line with parameter substitution (`{rom}`, `{platform}`, etc.)
- Handle paths with spaces (proper quoting)
- Launch process and wait for exit
- Capture start/end times
- Return launch result (success/failure)

**Pseudocode**:
```csharp
public class EmulatorLauncher
{
    public async Task<LaunchResult> LaunchGameAsync(Game game)
    {
        // 1. Get emulator configuration
        var emulator = GetEmulatorForGame(game);

        // 2. Validate ROM file exists
        if (!File.Exists(game.ApplicationPath))
            return LaunchResult.Failure("ROM not found");

        // 3. Build command line
        var commandLine = BuildCommandLine(emulator, game);

        // 4. Start tracking
        var startTime = DateTime.UtcNow;

        // 5. Launch process
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = emulator.ApplicationPath,
            Arguments = commandLine,
            UseShellExecute = false
        });

        // 6. Wait for exit
        await process.WaitForExitAsync();

        // 7. Calculate play time
        var endTime = DateTime.UtcNow;
        var playTimeSeconds = (int)(endTime - startTime).TotalSeconds;

        return LaunchResult.Success(playTimeSeconds);
    }
}
```

### Step 3: Implement StatisticsTracker

**File**: `src/Core/GameLauncher.Infrastructure/Services/StatisticsTracker.cs`

**Purpose**: Track and update game play statistics

**Key Features**:
- Update PlayCount, PlayTime, LastPlayed
- Record play sessions with timestamps
- Aggregate statistics by platform
- Export statistics to CSV/Excel

**Pseudocode**:
```csharp
public class StatisticsTracker
{
    private readonly XmlDataContext _dataContext;

    public async Task RecordPlaySessionAsync(Game game, int playTimeSeconds)
    {
        // Update game stats
        game.PlayCount++;
        game.PlayTime += playTimeSeconds;
        game.DateModified = DateTime.UtcNow;

        // Save to XML
        var games = _dataContext.LoadGames(game.Platform);
        var index = games.FindIndex(g => g.ID == game.ID);
        games[index] = game;
        _dataContext.SaveGames(game.Platform, games);
    }

    public PlatformStatistics GetPlatformStatistics(string platformName)
    {
        var games = _dataContext.LoadGames(platformName);

        return new PlatformStatistics
        {
            TotalGames = games.Count,
            TotalPlayTime = games.Sum(g => g.PlayTime),
            MostPlayedGame = games.OrderByDescending(g => g.PlayTime).First(),
            CompletionRate = games.Count(g => g.Completed) / (double)games.Count
        };
    }
}
```

### Step 4: Implement GameManager

**File**: `src/Core/GameLauncher.Infrastructure/Services/GameManager.cs`

**Purpose**: CRUD operations for games

**Methods**:
```csharp
Task<Game> CreateGameAsync(string platform, Game game)
Task UpdateGameAsync(string platform, Game game)
Task DeleteGameAsync(string platform, string gameId)
Task<List<Game>> SearchGamesAsync(string query)
Task<Game?> GetGameByIdAsync(string gameId)
```

### Step 5: Implement PlatformManager

**File**: `src/Core/GameLauncher.Infrastructure/Services/PlatformManager.cs`

**Purpose**: Platform operations and statistics

**Methods**:
```csharp
Task<List<Platform>> GetAllPlatformsAsync()
Task<Dictionary<string, List<Platform>>> GetPlatformsByCategoryAsync()
Task<PlatformStatistics> GetPlatformStatisticsAsync(string platformName)
```

### Step 6: Write Tests

**File**: `tests/GameLauncher.Infrastructure.Tests/EmulatorLauncherTests.cs`

Test cases:
- Launch game with valid emulator ✅
- Handle missing ROM file ❌
- Build command line correctly ✅
- Update statistics after play ✅

---

## 🧪 Testing Strategy

### Unit Tests

Create tests for each service:

```csharp
[Test]
public async Task EmulatorLauncher_ValidGame_UpdatesStatistics()
{
    // Arrange
    var launcher = new EmulatorLauncher(dataContext);
    var game = CreateTestGame();
    var initialPlayCount = game.PlayCount;

    // Act
    var result = await launcher.LaunchGameAsync(game);

    // Assert
    Assert.IsTrue(result.Success);
    Assert.AreEqual(initialPlayCount + 1, game.PlayCount);
    Assert.Greater(game.PlayTime, 0);
}
```

### Integration Tests

Test with actual LaunchBox data:

```csharp
[Test]
[Ignore("Integration test")]
public async Task LaunchRealGame()
{
    var ctx = new XmlDataContext(@"H:\LaunchBox\LaunchBox");
    var launcher = new EmulatorLauncher(ctx);
    var games = ctx.LoadGames("Nintendo 64");
    var game = games.First(g => g.Title == "Super Mario 64");

    var result = await launcher.LaunchGameAsync(game);

    Assert.IsTrue(result.Success);
}
```

---

## 📦 Required NuGet Packages

Add these to `GameLauncher.Infrastructure.csproj`:

```bash
# For CSV export
dotnet add package CsvHelper

# For Excel export (optional)
dotnet add package ClosedXML
```

---

## 🎯 Success Criteria for Phase 2

- [ ] EmulatorLauncher can launch games
- [ ] StatisticsTracker updates PlayCount/PlayTime
- [ ] GameManager CRUD operations work
- [ ] PlatformManager provides statistics
- [ ] SettingsManager loads/saves settings
- [ ] All services have unit tests (>80% coverage)
- [ ] Integration test with real LaunchBox data passes

---

## 🔄 Development Workflow

1. **Create service class**
2. **Write interface** (for dependency injection later)
3. **Implement basic functionality**
4. **Write unit tests**
5. **Test with mock data**
6. **Test with real LaunchBox data**
7. **Refine and optimize**

---

## 💡 Pro Tips

### Tip 1: Use Interfaces

```csharp
public interface IEmulatorLauncher
{
    Task<LaunchResult> LaunchGameAsync(Game game);
}

public class EmulatorLauncher : IEmulatorLauncher
{
    // Implementation
}
```

**Why**: Easier testing with mocks, better dependency injection

### Tip 2: Use Async/Await

```csharp
public async Task<Game> GetGameAsync(string id)
{
    return await Task.Run(() => _dataContext.LoadGames(platform)
        .FirstOrDefault(g => g.ID == id));
}
```

**Why**: Better UI responsiveness, scalability

### Tip 3: Add Logging

```csharp
public class EmulatorLauncher
{
    private readonly ILogger<EmulatorLauncher> _logger;

    public EmulatorLauncher(ILogger<EmulatorLauncher> logger)
    {
        _logger = logger;
    }

    public async Task<LaunchResult> LaunchGameAsync(Game game)
    {
        _logger.LogInformation("Launching game: {Title}", game.Title);
        // ...
    }
}
```

**Why**: Debugging, monitoring, troubleshooting

### Tip 4: Handle Errors Gracefully

```csharp
try
{
    var result = await LaunchGameAsync(game);
}
catch (FileNotFoundException ex)
{
    _logger.LogError(ex, "ROM not found: {Path}", game.ApplicationPath);
    return LaunchResult.Failure($"ROM not found: {ex.Message}");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to launch game");
    return LaunchResult.Failure($"Launch failed: {ex.Message}");
}
```

---

## 📚 Helpful Resources

### Command Line Building
```csharp
// LaunchBox uses these placeholders:
// {rom} - Full path to ROM file
// {platform} - Platform name
// {emulator} - Emulator path
// {emudir} - Emulator directory

private string BuildCommandLine(Emulator emulator, Game game)
{
    var cmdLine = emulator.CommandLine ?? "";
    cmdLine = cmdLine.Replace("{rom}", QuoteIfNeeded(game.ApplicationPath));
    cmdLine = cmdLine.Replace("{platform}", game.Platform);
    // ... more replacements
    return cmdLine;
}

private string QuoteIfNeeded(string path)
{
    return path.Contains(" ") ? $"\"{path}\"" : path;
}
```

### Process Management
```csharp
var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = emulator.ApplicationPath,
        Arguments = commandLine,
        UseShellExecute = false,
        CreateNoWindow = emulator.HideConsole,
        WorkingDirectory = Path.GetDirectoryName(emulator.ApplicationPath)
    }
};

process.Start();

// Optional: Monitor process
while (!process.HasExited)
{
    await Task.Delay(1000);
    // Update UI, check for pause, etc.
}

var exitCode = process.ExitCode;
```

---

## 🚦 Ready to Start?

1. ✅ Read this guide
2. ✅ Review Phase 1 implementation
3. ✅ Understand data models
4. 🎯 Start with EmulatorLauncher (easiest)
5. 🎯 Add StatisticsTracker
6. 🎯 Implement GameManager
7. 🎯 Create PlatformManager
8. 🎯 Build SettingsManager
9. ✅ Write tests for all services
10. 🎉 Phase 2 complete!

---

**Estimated Time**: 2 weeks
**Complexity**: Medium
**Fun Level**: High! You'll see games launching! 🎮

Good luck! 🚀
