# CLAUDE.md - AI Assistant Guide for OfficeHelp Project

## Project Overview

**OfficeHelp** is a C# WinForms desktop application that monitors Outlook web mail and reminders, providing desktop notifications for new emails and calendar events. The application uses Microsoft Edge WebView2 to embed the Outlook web interface and JavaScript injection to scrape email and reminder data.

### Key Features
- Email monitoring with desktop notifications
- Reminder/calendar event monitoring
- System tray integration
- Modern, responsive UI design
- WebView2-based browser embedding
- Automatic periodic checks (every 60 seconds)

### Technology Stack
- **Framework**: .NET 6.0 (Windows Forms)
- **Target Platform**: Windows 10 or higher
- **Key Dependencies**:
  - Microsoft.Web.WebView2 (v1.0.2210.55)
  - Newtonsoft.Json (v13.0.3)
- **Language**: C# with nullable reference types enabled

---

## Project Structure

```
OfficeHelp/
├── Program.cs              # Application entry point (STAThread setup)
├── MainForm.cs             # Main window with WebView2, UI, and system tray logic
├── EmailScraper.cs         # Email and reminder scraping logic with JavaScript injection
├── NotificationForm.cs     # Desktop notification window with fade animations
├── OfficeHelp.csproj       # Project configuration file
├── README.md               # User-facing documentation (Chinese)
├── .gitignore              # Standard .NET/Visual Studio ignore patterns
└── CLAUDE.md               # This file - AI assistant guidance
```

### File Responsibilities

#### Program.cs (19 lines)
- Standard WinForms application entry point
- Enables visual styles and runs MainForm
- Minimal setup, no complex logic

#### MainForm.cs (346 lines)
**Core responsibilities**:
- WebView2 initialization and management
- UI layout (toolbar, status bar, buttons)
- System tray integration (minimize, restore, context menu)
- Event handling for email/reminder notifications
- Timer-based periodic checks (60-second interval)
- Modern button styling with hover effects

**Key components**:
- `webView` (WebView2): Embedded browser control for Outlook web
- `emailScraper` (EmailScraper): Data scraping engine
- `checkTimer` (Timer): 60-second interval for periodic checks
- `notifyIcon` (NotifyIcon): System tray icon
- `statusLabel` (ToolStripStatusLabel): Bottom status bar

**Important methods**:
- `InitializeAsync()`: Sets up WebView2, navigates to Outlook, starts timer
- `CoreWebView2_NavigationCompleted()`: Updates status on page load
- `CheckTimer_Tick()`: Periodic email/reminder check
- `EmailScraper_OnNewEmail()`: Displays notification for new emails
- `EmailScraper_OnNewReminder()`: Displays notification for reminders

#### EmailScraper.cs (309 lines)
**Core responsibilities**:
- JavaScript injection to scrape Outlook web page
- Email and reminder detection
- Deduplication using HashSet tracking
- Event-based notification system

**Key features**:
- Multiple CSS selector strategies for robustness
- JSON serialization for data extraction
- HashSet-based ID tracking to prevent duplicate notifications
- Error handling for DOM parsing failures

**Important methods**:
- `CheckForUpdatesAsync()`: Main entry point for checking both emails and reminders
- `CheckUnreadEmailsAsync()`: Injects JS to find unread emails
- `CheckRemindersAsync()`: Injects JS to find reminder dialogs/elements

**JavaScript strategy**:
- Uses multiple selectors to adapt to Outlook UI changes
- Extracts: sender, subject, time, ID for emails
- Extracts: title, time, ID for reminders
- Returns JSON arrays for deserialization

#### NotificationForm.cs (236 lines)
**Core responsibilities**:
- Borderless, topmost notification window
- Fade-in/fade-out animations
- Auto-positioning to screen bottom-right
- Color-coded notifications by type

**Key features**:
- Three notification types: Email (blue), Reminder (orange), Info (teal)
- 5-second auto-close timer
- Click-to-close functionality
- WS_EX_NOACTIVATE flag to prevent focus stealing
- Smooth opacity animations (20ms intervals, 0.05 increments)

---

## Development Conventions

### Code Style
- **Namespace**: All code under `OfficeHelp` namespace
- **Nullable references**: Enabled throughout project
- **Event handlers**: Use nullable sender (`object?`)
- **Async patterns**: Async/await for WebView2 operations
- **Comments**: Chinese comments for implementation details
- **UI text**: Chinese language for user-facing strings

### Naming Conventions
- **Fields**: camelCase with descriptive names
- **Properties**: PascalCase
- **Events**: PascalCase with "On" prefix (e.g., `OnNewEmail`)
- **Event args**: Suffix with "EventArgs" (e.g., `EmailEventArgs`)
- **Private classes**: PascalCase even when nested

### Error Handling
- Try-catch blocks around all async WebView2 operations
- Console.WriteLine for debug logging (no formal logging framework)
- MessageBox for critical initialization failures
- Status label updates for user-visible errors
- Graceful degradation when selectors fail

### Resource Management
- IDisposable pattern implemented where needed
- Explicit disposal of timers, WebView2, NotifyIcon
- HashSet clearing in Dispose methods
- Timer stopping before disposal

---

## Common Development Tasks

### Adding a New Notification Type
1. Add enum value to `NotificationType` in NotificationForm.cs
2. Add color case in NotificationForm constructor switch statement
3. Create corresponding event args class if needed
4. Wire up event handler in MainForm.cs

### Modifying Email Detection Logic
**File**: EmailScraper.cs:57-170
- Update JavaScript selectors in `CheckUnreadEmailsAsync()` method
- Test multiple selector strategies for robustness
- Ensure unique ID generation for deduplication
- Update `EmailInfo` class if adding new fields

### Changing Check Interval
**File**: MainForm.cs:123-126
```csharp
checkTimer = new System.Windows.Forms.Timer
{
    Interval = 60000 // Milliseconds (60000 = 1 minute)
};
```

### Customizing Notification Appearance
**File**: NotificationForm.cs:37-142
- Window size: Line 43 `this.Size = new Size(350, 120)`
- Auto-close time: Line 157 `Interval = 5000` (5 seconds)
- Colors: Lines 58-67 (accent color switch)
- Font sizes: Lines 91, 102
- Animation speed: Line 149 `Interval = 20ms`

### Adding UI Elements to MainForm
1. Declare field in MainForm class (lines 11-17)
2. Create control in `InitializeComponent()` (lines 25-131)
3. Add to appropriate parent container (topPanel, statusStrip, or this.Controls)
4. Wire up event handlers
5. Consider anchor/dock properties for resizing

---

## Technical Details

### WebView2 Integration
- **Initialization**: `EnsureCoreWebView2Async(null)` in InitializeAsync
- **User data folder**: Default location (not specified)
- **Settings**: JavaScript enabled, script dialogs enabled, web messages enabled
- **Navigation**: Direct to https://outlook.office.com/mail/
- **Script execution**: `ExecuteScriptAsync()` for data extraction

### JavaScript Injection Pattern
```javascript
(function() {
    try {
        // Scraping logic here
        return JSON.stringify(results);
    } catch (error) {
        return JSON.stringify({ error: error.message });
    }
})();
```
- Immediately invoked function expression (IIFE)
- Returns JSON string for C# deserialization
- Error handling within JavaScript

### Data Flow
1. Timer triggers every 60 seconds → `CheckTimer_Tick()`
2. Calls `emailScraper.CheckForUpdatesAsync()`
3. JavaScript injected into WebView2 DOM
4. JSON results parsed into C# objects
5. New items checked against HashSet
6. Events fired for new items → `OnNewEmail`, `OnNewReminder`
7. MainForm handlers invoked on UI thread
8. NotificationForm created and displayed

### Threading Model
- UI thread for all WinForms operations
- Async/await for WebView2 operations
- `this.Invoke()` used to marshal events to UI thread (MainForm.cs:237, 258)

---

## Known Issues and Considerations

### Outlook DOM Changes
- **Problem**: Outlook web UI changes frequently, breaking selectors
- **Solution**: Multiple selector strategies implemented
- **When modifying**: Always test with current Outlook web version
- **Files affected**: EmailScraper.cs lines 63-77 (emails), 186-200 (reminders)

### WebView2 Runtime Dependency
- First-time users must install Microsoft Edge WebView2 Runtime
- Error handling in place for initialization failures
- No auto-download implemented

### Login State Persistence
- WebView2 stores login cookies automatically
- User data persists between sessions
- No manual session management needed

### Performance
- DOM queries run every 60 seconds
- No optimization for large inboxes
- Memory grows with HashSet size (cleared only on restart)

### Localization
- UI entirely in Chinese
- No i18n framework in place
- Hard-coded strings throughout

---

## Testing Approach

### Manual Testing Checklist
- [ ] WebView2 initializes without errors
- [ ] Outlook login page loads correctly
- [ ] Email detection triggers on new mail
- [ ] Reminder detection works for calendar events
- [ ] Notifications appear in correct position
- [ ] Notifications auto-close after 5 seconds
- [ ] System tray minimize/restore works
- [ ] Close dialog offers minimize option
- [ ] Status bar updates correctly
- [ ] Refresh button reloads page

### Debugging JavaScript Selectors
1. Open application
2. Press F12 in WebView2 (if dev tools enabled)
3. Test selectors in console
4. Update selectors in EmailScraper.cs
5. Rebuild and test

### Common Debugging Scenarios
- **No emails detected**: Check selectors, verify login, inspect DOM structure
- **Duplicate notifications**: Verify ID generation is unique
- **Notification positioning**: Check Screen.PrimaryScreen.WorkingArea
- **Timer not firing**: Verify timer.Start() called, check Interval value

---

## Build and Deployment

### Building from Source
```bash
dotnet restore
dotnet build
dotnet run
```

### Publishing Standalone Executable
```bash
dotnet publish -c Release -r win-x64 --self-contained
```
Output: `bin/Release/net6.0-windows/win-x64/publish/`

### Build Artifacts to Ignore
See .gitignore for complete list:
- bin/, obj/ directories
- Debug/Release folders
- .vs/ Visual Studio folder
- *.user, *.suo files
- WebView2/ runtime folder
- NuGet packages

---

## AI Assistant Guidelines

### When Making Changes
1. **Always read files first**: Use Read tool before Edit/Write
2. **Preserve Chinese text**: Don't translate UI strings or comments unless requested
3. **Maintain async patterns**: Keep async/await for WebView2 operations
4. **Test selector robustness**: When updating EmailScraper, add multiple selector fallbacks
5. **Update status messages**: Keep user informed via statusLabel
6. **Handle nullables properly**: Respect nullable reference type annotations

### Code Review Focus Areas
1. **Thread safety**: Ensure UI updates use Invoke when needed
2. **Resource disposal**: Verify IDisposable implementation
3. **Error handling**: Try-catch around WebView2 and JavaScript execution
4. **ID generation**: Ensure uniqueness for deduplication
5. **Animation smoothness**: Check timer intervals and opacity increments

### Common Refactoring Patterns
- Extract magic numbers to constants/fields
- Add configuration file for intervals and settings
- Implement dependency injection for testability
- Add logging framework instead of Console.WriteLine
- Create service layer for WebView2 operations

### Security Considerations
- **No credentials stored**: OAuth handled by WebView2/Microsoft
- **JavaScript injection**: Only on trusted Outlook domain
- **No external data**: All scraping from official Outlook web
- **User data**: WebView2 stores in default user data folder

### Performance Optimization Opportunities
- Implement HashSet size limits (LRU cache)
- Optimize JavaScript selector performance
- Reduce DOM query frequency for large inboxes
- Add debouncing for rapid notifications
- Lazy-load WebView2 components

---

## Future Enhancement Ideas

From README.md development roadmap:
- [ ] Sound notifications
- [ ] Customizable notification styles
- [ ] Quick email actions (mark read, delete)
- [ ] Multi-account support
- [ ] Configuration file for user settings
- [ ] Memory optimization

Additional technical enhancements:
- [ ] Unit tests for scraping logic
- [ ] Integration tests with mock WebView2
- [ ] Localization/i18n framework
- [ ] Settings dialog for customization
- [ ] Logging framework (Serilog, NLog)
- [ ] Update checker for Outlook selector changes

---

## Important Links and Resources

### Documentation
- [Microsoft Edge WebView2 Docs](https://docs.microsoft.com/en-us/microsoft-edge/webview2/)
- [WebView2 Runtime Download](https://developer.microsoft.com/microsoft-edge/webview2/)
- [Windows Forms Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)

### Project Resources
- README.md: User-facing documentation (Chinese)
- This file (CLAUDE.md): Technical guide for AI assistants
- OfficeHelp.csproj: NuGet package versions and build settings

---

## Quick Reference

### Key Configuration Values
| Setting | Location | Value |
|---------|----------|-------|
| Check interval | MainForm.cs:125 | 60000ms (1 minute) |
| Notification duration | NotificationForm.cs:157 | 5000ms (5 seconds) |
| Notification size | NotificationForm.cs:43 | 350x120 pixels |
| Fade animation speed | NotificationForm.cs:149 | 20ms interval |
| Opacity increment | NotificationForm.cs:174,187 | 0.05 per tick |
| Main window size | MainForm.cs:28 | 1200x800 pixels |
| Minimum window size | MainForm.cs:31 | 800x600 pixels |
| Outlook URL | MainForm.cs:186 | https://outlook.office.com/mail/ |

### Event Flow Diagram
```
Timer (60s) → CheckTimer_Tick
              ↓
       CheckForUpdatesAsync
              ↓
       ┌──────┴──────┐
       ↓             ↓
CheckUnreadEmails  CheckReminders
       ↓             ↓
Execute JavaScript (returns JSON)
       ↓             ↓
Deserialize to C# objects
       ↓             ↓
Check against HashSet
       ↓             ↓
Fire events if new
       ↓             ↓
OnNewEmail     OnNewReminder
       ↓             ↓
Invoke on UI thread
       ↓             ↓
Create NotificationForm
       ↓
Show with fade animation
       ↓
Auto-close after 5s
```

### Critical Methods to Understand
1. **MainForm.InitializeAsync()**: Application startup sequence
2. **EmailScraper.CheckUnreadEmailsAsync()**: Email detection logic
3. **NotificationForm constructor**: Notification UI setup
4. **MainForm.CheckTimer_Tick()**: Periodic update trigger

---

## Version History

- **Current**: Modern UI with improved buttons and styling (commit 9519c3b)
- **Initial**: Basic Outlook scraper with desktop notifications (commit 7055a7b)

---

**Last Updated**: 2025-11-16
**For**: Claude AI Assistant
**Project**: OfficeHelp - Outlook Mail Reminder Application
