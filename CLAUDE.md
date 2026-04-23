# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Build the app
xcodebuild -project TechDemo.xcodeproj -scheme TechDemo -destination 'platform=iOS Simulator,name=iPhone 16' build

# Run all unit tests
xcodebuild -project TechDemo.xcodeproj -scheme TechDemo -destination 'platform=iOS Simulator,name=iPhone 16' test

# Run a single test class
xcodebuild -project TechDemo.xcodeproj -scheme TechDemo -destination 'platform=iOS Simulator,name=iPhone 16' -only-testing:TechDemoTests/HomeViewModelTests test

# Run a single test method
xcodebuild -project TechDemo.xcodeproj -scheme TechDemo -destination 'platform=iOS Simulator,name=iPhone 16' -only-testing:TechDemoTests/HomeViewModelTests/testLoadUser_updatesUserName test
```

## Architecture

UIKit iOS app using **MVVM** with the `@Observable` macro (Observation framework) for reactive state.

**Dependency flow:** `ViewController → ViewModel → Service → Model`

- **Models/** — Plain Swift structs (`User`)
- **Services/** — Data-fetching layer (`UserService`), currently returns static data. Injected into ViewModels via initializer with a default value.
- **Home/** — Feature module containing `HomeViewController` (programmatic UIKit with Auto Layout) and `HomeViewModel` (business logic, state). SceneDelegate wraps it in a UINavigationController as the root.
- **TechDemoTests/** — XCTest unit tests. Tests use `XCTestExpectation` for async verification since `UserService.fetchUser` uses a completion handler.

No storyboards or XIBs — all UI is built programmatically. No third-party dependencies.
