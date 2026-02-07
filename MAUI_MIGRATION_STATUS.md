# Xamarin.Forms to .NET MAUI Migration - Status and TODO

## Summary
The basic MAUI project structure has been created with **multi-platform support for Android and iOS**. The project now targets both `net9.0-android` and `net9.0-ios`. However, there are many namespace and package issues that need to be resolved before the migration is complete.

**Platform Status:**
- ✅ **Android**: Platform structure complete, needs namespace migration
- ✅ **iOS**: Platform structure complete, needs namespace migration + macOS for testing

## Completed Tasks

### 1. Project Structure ✓
- Created iPMCloud.Mobile.Maui project with **multi-platform support**
- **TargetFrameworks**: `net9.0-android;net9.0-ios`
- Set up Platforms/Android/ structure with all Android-specific files
- Set up Platforms/iOS/ structure with all iOS-specific files
- Copied all shared code from iPMCloud.Mobile
- Created MauiProgram.cs as entry point
- Created basic Resources structure (Styles, Images, Fonts, Raw)

### 2. Platform-Specific Files ✓ (Partially)
- **MainActivity.cs**: Updated to inherit from MauiAppCompatActivity
- **MainApplication.cs**: Updated to inherit from MauiApplication with CreateMauiApp()
- **SplashActivity.cs**: Updated namespaces for MAUI
- **AndroidManifest.xml**: Updated minSdkVersion from 27 to 21
- **Android Resources**: All copied to Platforms/Android/Resources/

### 3. Project Configuration ✓
- Created iPMCloud.Mobile.Maui.csproj targeting **net9.0-android and net9.0-ios**
- Added iOS-specific PropertyGroup with codesigning configuration
- Updated package references for AndroidX libraries
- Added core MAUI packages (Microsoft.Maui.Controls 9.0.120)
- Configured App.xaml with MAUI namespace
- Created Colors.xaml and Styles.xaml

## Known Issues and Remaining Work

### Critical - Namespace Migration (NOT YET DONE)

#### XAML Files Need Update
All XAML files still use old namespace. Need to change from:
```xml
xmlns="http://xamarin.com/schemas/2014/forms"
```
to:
```xml
xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
```

Files affected:
- MainPage.xaml
- SettingsPage.xaml
- StartPage.xaml
- All XAML files in XAML/ subdirectory
- All Popup XAML files

#### C# Files Need Namespace Updates

**Xamarin.Forms → Microsoft.Maui.Controls:**
- All .cs files using `Xamarin.Forms`
- Custom controls (TFEntry, CustomScrollView)
- Page files (MainPage.xaml.cs, SettingsPage.xaml.cs, StartPage.xaml.cs)
- Navigation (TFPageNavigator.cs)
- Many vo/ files

**Xamarin.Essentials → Microsoft.Maui packages:**
- `Xamarin.Essentials` → `Microsoft.Maui.ApplicationModel` or `Microsoft.Maui.Devices`
- Files: IDependentService.cs, MainPage.xaml.cs, SettingsPage.xaml.cs, StartPage.xaml.cs, AppModel.cs, etc.

### Plugin/Package Replacements Needed

1. **Matcha.BackgroundService** - No direct MAUI equivalent
   - Need to implement native background services
   - Code commented with TODO in App.xaml.cs

2. **Plugin.FirebasePushNotification** - No MAUI-compatible version yet
   - Need alternative Firebase implementation for MAUI
   - Code commented in App.xaml.cs, MainActivity.cs, MainApplication.cs

3. **Plugin.LocalNotification** - Need MAUI-compatible alternative
   - Code commented in App.xaml.cs

4. **ZXing.Net.Mobile.Forms** → **ZXing.Net.Maui**
   - Need to install and configure ZXing.Net.Maui package
   - Update scanner initialization code

5. **NativeMedia** → **Microsoft.Maui.Media.MediaPicker**
   - Built into MAUI, just needs namespace changes
   - Files: MainPage.xaml.cs, MainActivity.cs

6. **Xamarin.Forms.Maps** → **Microsoft.Maui.Controls.Maps**
   - Need to add Microsoft.Maui.Controls.Maps package
   - Update initialization in MauiProgram.cs

7. **Xam.Plugin.Media** → MAUI MediaPicker
   - Use Microsoft.Maui.Media APIs instead

8. **Xamarin.Forms.RangeSlider** - Need MAUI alternative
   - May need custom control or community package

9. **Xamarin.MediaGallery** - Need MAUI alternative
   - Use MAUI Media APIs or find alternative

### Custom Renderers → Handlers

Need to convert:
- **CustomEntryRenderer** (Platforms/Android/Renderer/)
- **CustomScrollView renderers** (TFControls/)
- **TFEntry renderers** (TFControls/TFEntry/)

MAUI uses Handlers instead of Renderers. These need to be rewritten.

### Dependency Injection

Need to migrate from DependencyService to DI in MauiProgram.cs:
- ImageResizer
- IDependentService
- IBatteryInfo
- IImageAddText
- Any other services using DependencyService

### iOS Support ✓ (Added)

**iOS Platform Structure:**
- ✅ `Platforms/iOS/` directory created
- ✅ iOS `AppDelegate.cs` created in MAUI format (inherits from MauiUIApplicationDelegate)
- ✅ iOS `Main.cs` created as entry point
- ✅ iOS `Info.plist` migrated with MAUI-compatible settings (MinimumOSVersion: 14.0)
- ✅ iOS `Entitlements.plist` migrated (includes aps-environment for push notifications)
- ✅ iOS `GoogleService-Info.plist` copied for Firebase configuration
- ✅ iOS `NLog.config` created with iOS-specific log paths
- ✅ `.csproj` updated to target `net9.0-android;net9.0-ios`
- ✅ iOS-specific PropertyGroup added to `.csproj` (codesigning configuration)
- ✅ iOS-specific ItemGroup added to `.csproj` for iOS files

**iOS-Specific Known Blockers:**

1. **Firebase Push Notifications** - NOT YET FUNCTIONAL
   - Original iOS project used `Plugin.FirebasePushNotification` (not MAUI-compatible)
   - AppDelegate has TODO comments indicating Firebase needs MAUI-compatible implementation
   - Options: Use Firebase.iOS NuGet package directly or wait for community MAUI plugins
   - **Status**: Firebase initialization commented out with TODO

2. **Background Services** - NOT YET FUNCTIONAL
   - Original iOS project used `Matcha.BackgroundService.iOS` (not MAUI-compatible)
   - AppDelegate has TODO comments indicating background service needs MAUI-compatible implementation
   - Options: Implement native iOS background tasks or use WorkManager alternatives
   - **Status**: Background service initialization commented out with TODO

3. **macOS Build Environment Required**
   - iOS builds require macOS with Xcode installed
   - Current development environment is Linux - iOS cannot be built/tested here
   - **Status**: Structure is complete, but build/test requires macOS

**iOS Bundle Configuration:**
- Bundle Identifier: `com.ipmcloud.ipm.mobile.app`
- Display Name: `iPM Cloud`
- Supported iOS Version: 14.0+ (MAUI requirement)
- Device Family: iPhone + iPad
- Permissions: Camera, Photo Library, Microphone, Location (preserved from original)

**iOS Assets Migration Status:**
- ⚠️ **TODO**: iOS Assets from `iPMCloud.Mobile.iOS/Assets.xcassets/` not yet migrated
- Should be moved to `iPMCloud.Mobile.Maui/Resources/Images/` or kept in `Platforms/iOS/Resources/`
- This is a lower priority and can be done during macOS testing phase

### Configuration Files

- **NLog.config** - Copied ✓
- **google-services.json** - Needs to be placed in Platforms/Android/ and verified

### Resources and Assets

- Need to migrate all Assets/ to proper Resources/ structure
- Create proper app icon SVG (currently commented out)
- Create proper splash screen SVG (currently commented out)
- Update all asset references in code to use MAUI resource system

## Build Errors Summary

Current build attempt shows ~200+ compilation errors, primarily:
1. XAML namespace errors (3 files minimum)
2. Xamarin.Forms namespace errors (60+ files)
3. Xamarin.Essentials namespace errors (20+ files)
4. Plugin namespace errors (Firebase, LocalNotification, etc.)
5. Missing package references (ZXing, Maps, etc.)

## Recommended Next Steps

1. **Batch Update XAML Namespaces**: Update all XAML files xmlns declarations
2. **Batch Update C# Namespaces**: 
   - Replace `using Xamarin.Forms;` with `using Microsoft.Maui.Controls;`
   - Replace `using Xamarin.Essentials;` with appropriate MAUI namespaces
3. **Add Required Packages**: ZXing.Net.Maui, Microsoft.Maui.Controls.Maps
4. **Implement/Replace Plugins**: Find MAUI alternatives for Firebase, BackgroundService, etc.
5. **Convert Custom Renderers**: Rewrite as MAUI Handlers
6. **Setup Dependency Injection**: Move all DependencyService registrations to MauiProgram.cs
7. **Test Build**: After namespace updates, try building again
8. **Handle Remaining Compilation Errors**: Fix any specific API changes

## Notes

- The project structure is correct for MAUI
- The main challenge is the mass namespace migration
- Some plugins don't have MAUI equivalents yet (Firebase, BackgroundService)
- The migration is substantial but follows a clear pattern

## Estimated Work

- Namespace updates: 2-4 hours of systematic find/replace work
- Plugin replacements: 4-8 hours (depends on alternatives available)
- Custom renderer conversion: 2-4 hours
- Testing and fixes: 4-8 hours
- **Total**: 12-24 hours of development work

This is a complex but achievable migration. The project has a good foundation now with the MAUI structure in place.
