# Feature: Shanghai Weather Display

## 1. Overview

Replace the existing multi-user home page with a weather display that fetches current weather data for Shanghai from the Open-Meteo public API. The home screen will show the current temperature and weather condition, include a Refresh button to re-fetch data, and handle loading and error states. The existing User model, UserService, and all user-related ViewModel logic will be removed and replaced with the new weather feature. The MVVM pattern with `@Observable` macro and programmatic UIKit will be preserved.

## 2. Architecture Impact

- **Model** -- The existing `User` struct will be deleted. A new `WeatherResponse` model will be created to decode the Open-Meteo API JSON response. The model will be a plain Swift `Codable` struct.
- **Service** -- The existing `UserService` will be deleted. A new `WeatherService` will be created that performs a real `URLSession` network request to the Open-Meteo API and returns decoded weather data via a completion handler. A `WeatherServiceProtocol` will be extracted so tests can inject a mock.
- **ViewModel** -- The existing `HomeViewModel` will be completely rewritten. It will manage three states (idle, loading, loaded, error), expose weather display properties, and delegate fetching to `WeatherService`.
- **UI** -- The existing `HomeViewController` will be completely rewritten. It will display a city label ("Shanghai"), a temperature label, a weather condition label, a Refresh button, and show loading/error states appropriately. It will use `withObservationTracking` for reactive updates from the `@Observable` ViewModel.
- **Tests** -- The existing `HomeViewModelTests` will be completely rewritten to test the new ViewModel with a mock `WeatherService`. Tests will cover successful fetch, error handling, loading state, and refresh behavior.
- **SceneDelegate** -- No changes. `SceneDelegate` already wraps `HomeViewController` in a `UINavigationController`, which remains correct.

### Files to Delete
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Models/User.swift`
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Services/UserService.swift`

### Files to Create
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Models/WeatherResponse.swift`
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Services/WeatherService.swift`

### Files to Rewrite
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Home/HomeViewModel.swift`
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Home/HomeViewController.swift`
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemoTests/TechDemoTests.swift`

### Files Unchanged
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/SceneDelegate.swift`
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/AppDelegate.swift`
- `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo.xcodeproj/project.pbxproj` (project uses `PBXFileSystemSynchronizedRootGroup` so file additions/removals within `TechDemo/` and `TechDemoTests/` are auto-detected)

## 3. Task Breakdown

### Task 1: Create WeatherResponse model

- **Description:** Create a new `Codable` struct `WeatherResponse` that maps to the Open-Meteo API JSON response. The API endpoint is `https://api.open-meteo.com/v1/forecast?latitude=31.2304&longitude=121.4737&current_weather=true`. The relevant JSON structure is:
  ```
  {
    "current_weather": {
      "temperature": 12.7,
      "windspeed": 4.8,
      "winddirection": 312,
      "weathercode": 3,
      "is_day": 0,
      "time": "2026-04-23T14:15",
      "interval": 900
    }
  }
  ```
  Create two structs:
  - `CurrentWeather`: `Codable` with properties `temperature: Double`, `windspeed: Double`, `winddirection: Int`, `weathercode: Int`, `isDay: Int` (mapped from JSON key `is_day` via `CodingKeys`), `time: String`, `interval: Int`.
  - `WeatherResponse`: `Codable` with a single property `currentWeather: CurrentWeather` (mapped from JSON key `current_weather` via `CodingKeys`).
  Both structs use `CodingKeys` enum for snake_case to camelCase conversion. No methods, no protocols beyond `Codable`. Also add a computed property `conditionDescription: String` on `CurrentWeather` that maps WMO weather codes to human-readable strings (e.g., 0 = "Clear sky", 1-3 = "Partly cloudy", 45-48 = "Fog", 51-55 = "Drizzle", 61-65 = "Rain", 71-75 = "Snowfall", 80-82 = "Rain showers", 95 = "Thunderstorm"; default to "Unknown" for unlisted codes).

- **Affected files:**
  - CREATE `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Models/WeatherResponse.swift`

- **Agent:** model_agent

- **Input Context:**
  - Open-Meteo API JSON response structure (shown above)
  - WMO weather code reference: 0=Clear sky, 1=Mainly clear, 2=Partly cloudy, 3=Overcast, 45=Foggy, 48=Depositing rime fog, 51=Light drizzle, 53=Moderate drizzle, 55=Dense drizzle, 61=Slight rain, 63=Moderate rain, 65=Heavy rain, 71=Slight snowfall, 73=Moderate snowfall, 75=Heavy snowfall, 80=Slight rain showers, 81=Moderate rain showers, 82=Violent rain showers, 95=Thunderstorm

- **Output:**
  - `WeatherResponse.swift` containing `CurrentWeather` and `WeatherResponse` structs, both `Codable`, with `CodingKeys` for snake_case mapping, and `CurrentWeather.conditionDescription` computed property returning a human-readable weather description string.

---

### Task 2: Delete old User model and UserService

- **Description:** Remove the files that are being replaced by the new weather feature. Delete `User.swift` and `UserService.swift` from the project. The Xcode project uses `PBXFileSystemSynchronizedRootGroup` so these files will be automatically removed from the build when deleted from disk. No `project.pbxproj` edits needed.

- **Affected files:**
  - DELETE `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Models/User.swift`
  - DELETE `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Services/UserService.swift`

- **Agent:** model_agent

- **Input Context:**
  - None. These files are standalone and not referenced by any file that will survive this feature.

- **Output:**
  - Both files deleted from disk.

---

### Task 3: Create WeatherService and WeatherServiceProtocol

- **Description:** Create a network service that fetches weather data from the Open-Meteo API. Create:
  1. `WeatherServiceProtocol`: A protocol with a single method `func fetchWeather(completion: @escaping (Result<WeatherResponse, Error>) -> Void)`. This allows test injection.
  2. `WeatherService`: A concrete class conforming to `WeatherServiceProtocol`. It stores the API URL as a constant `let url = URL(string: "https://api.open-meteo.com/v1/forecast?latitude=31.2304&longitude=121.4737&current_weather=true")!`. The `fetchWeather` method uses `URLSession.shared.dataTask` to perform a GET request. On success, it decodes the JSON data into `WeatherResponse` using `JSONDecoder` and calls the completion with `.success`. On failure (network error or decoding error), it calls the completion with `.failure` passing the appropriate error. The completion handler is dispatched to the main queue using `DispatchQueue.main.async` so the caller does not need to worry about thread hopping.
  Note: The project has `SWIFT_DEFAULT_ACTOR_ISOLATION = MainActor`, so all types default to `@MainActor`. `URLSession` callbacks run on a background queue, so the completion must be explicitly dispatched to main. Use `nonisolated` on the data task callback if needed, or use `Task` with `@MainActor` for the async dispatch.

- **Affected files:**
  - CREATE `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Services/WeatherService.swift`

- **Agent:** service_agent

- **Input Context:**
  - `WeatherResponse` struct signature: `struct WeatherResponse: Codable { let currentWeather: CurrentWeather }` and `struct CurrentWeather: Codable { let temperature: Double; let windspeed: Double; let winddirection: Int; let weathercode: Int; let isDay: Int; let time: String; let interval: Int; var conditionDescription: String }` (from Task 1)
  - API URL: `https://api.open-meteo.com/v1/forecast?latitude=31.2304&longitude=121.4737&current_weather=true`
  - Project concurrency setting: `SWIFT_DEFAULT_ACTOR_ISOLATION = MainActor`

- **Output:**
  - `WeatherService.swift` containing `WeatherServiceProtocol` and `WeatherService`. The protocol defines `func fetchWeather(completion: @escaping (Result<WeatherResponse, Error>) -> Void)`. The concrete implementation performs a `URLSession` GET request, decodes JSON, and calls the completion on the main queue.

---

### Task 4: Rewrite HomeViewModel for weather

- **Description:** Completely rewrite `HomeViewModel` to manage weather state instead of user state. The new ViewModel:
  1. Uses `@Observable` macro (same as before).
  2. Defines a `ViewState` enum with cases: `.idle` (initial state, before any fetch), `.loading`, `.loaded(weather: CurrentWeather)`, `.error(message: String)`. This enum is `@Observable`-compatible (it is a value type stored as a property).
  3. Has a stored property `var viewState: ViewState = .idle`.
  4. Has a stored property `var isLoading: Bool` derived from `viewState` (true when `.loading`).
  5. Has a computed property `var temperatureText: String?` that returns a formatted string like "12.7 C" when state is `.loaded`, nil otherwise.
  6. Has a computed property `var conditionText: String?` that returns `currentWeather.conditionDescription` when state is `.loaded`, nil otherwise.
  7. Has a computed property `var errorMessage: String?` that returns the error message when state is `.error`, nil otherwise.
  8. Has an initializer `init(weatherService: WeatherServiceProtocol = WeatherService())` for dependency injection.
  9. Has a method `func fetchWeather()` that sets `viewState = .loading`, then calls `weatherService.fetchWeather`. On success, sets `viewState = .loaded(weather:)`. On failure, sets `viewState = .error(message: error.localizedDescription)`.
  All user-related properties (`users`, `currentIndex`, `currentUserName`, `loadUsers()`, `nextUser()`) are removed.

- **Affected files:**
  - REWRITE `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Home/HomeViewModel.swift`

- **Agent:** viewmodel_agent

- **Input Context:**
  - `WeatherServiceProtocol` signature: `protocol WeatherServiceProtocol { func fetchWeather(completion: @escaping (Result<WeatherResponse, Error>) -> Void) }` (from Task 3)
  - `CurrentWeather` struct signature: `let temperature: Double; let windspeed: Double; let winddirection: Int; let weathercode: Int; let isDay: Int; let time: String; let interval: Int; var conditionDescription: String` (from Task 1)
  - `WeatherResponse` struct signature: `let currentWeather: CurrentWeather` (from Task 1)
  - Project uses `@Observable` macro for reactive state.
  - Project has `SWIFT_DEFAULT_ACTOR_ISOLATION = MainActor`.

- **Output:**
  - `HomeViewModel.swift` completely rewritten with weather logic. Public surface:
    - `var viewState: ViewState` (stored, observable, initially `.idle`)
    - `var isLoading: Bool` (computed)
    - `var temperatureText: String?` (computed)
    - `var conditionText: String?` (computed)
    - `var errorMessage: String?` (computed)
    - `func fetchWeather()`
    - `init(weatherService: WeatherServiceProtocol = WeatherService())`
  - `ViewState` enum defined at file scope (or nested inside HomeViewModel).

---

### Task 5: Rewrite HomeViewController for weather UI

- **Description:** Completely rewrite `HomeViewController` to display weather information. The new UI:
  1. **City label:** A `UILabel` at the top showing "Shanghai" in large bold text (font size ~28, weight `.bold`).
  2. **Temperature label:** A `UILabel` below the city label showing the temperature (e.g., "12.7 C") in a very large font (font size ~64, weight `.light`). Centered horizontally.
  3. **Condition label:** A `UILabel` below the temperature showing the weather condition description (e.g., "Overcast") in medium text (font size ~20, weight `.regular`). Centered horizontally.
  4. **Refresh button:** A `UIButton` of type `.system` below the condition label, titled "Refresh". Its action calls `viewModel.fetchWeather()`. Centered horizontally.
  5. **Loading state:** When `viewModel.isLoading` is true, show a `UIActivityIndicatorView` (medium style) centered on screen, and hide the temperature and condition labels. Stop and hide the indicator when loading finishes.
  6. **Error state:** When `viewModel.errorMessage` is non-nil, show the error message in a red-colored label. Hide temperature and condition labels.
  7. **Auto-fetch on load:** Call `viewModel.fetchWeather()` in `viewDidLoad()` so weather loads immediately when the screen appears.
  8. **Reactive updates:** Use `withObservationTracking` to observe `viewModel.viewState` and update all UI elements accordingly when it changes. Re-register observation in the `onChange` callback.
  9. **Navigation title:** Set `title = "Weather"` on the view controller.
  10. Remove all user-related UI elements and methods (`nameLabel`, `loadButton`, `nextButton`, `loadUsersTapped`, `nextUserTapped`).

- **Affected files:**
  - REWRITE `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemo/Home/HomeViewController.swift`

- **Agent:** ui_agent

- **Input Context:**
  - `HomeViewModel` public surface (from Task 4):
    - `var viewState: ViewState` (observable)
    - `var isLoading: Bool` (computed)
    - `var temperatureText: String?` (computed)
    - `var conditionText: String?` (computed)
    - `var errorMessage: String?` (computed)
    - `func fetchWeather()`
    - `init(weatherService: WeatherServiceProtocol = WeatherService())`
  - `ViewState` is an enum: `.idle`, `.loading`, `.loaded(weather: CurrentWeather)`, `.error(message: String)`
  - Existing `HomeViewController` pattern: programmatic UIKit, `lazy var` for UI elements, Auto Layout via `NSLayoutConstraint.activate`, all in `setupUI()` called from `viewDidLoad()`.

- **Output:**
  - `HomeViewController.swift` completely rewritten with weather display UI including city label, temperature label, condition label, refresh button, loading indicator, and error label. All using programmatic UIKit with Auto Layout.

---

### Task 6: Rewrite unit tests for HomeViewModel with mock WeatherService

- **Description:** Completely rewrite the test file to test the new weather ViewModel. Create:
  1. **MockWeatherService:** A test-only class conforming to `WeatherServiceProtocol`. It stores a `result: Result<WeatherResponse, Error>?` that can be set before each test. Its `fetchWeather` method calls the completion with the stored result.
  2. **HomeViewModelTests test class:** Rename the test class from `HomeViewModelTests` (keep the same name for consistency).
  3. **Test: `testFetchWeather_success_updatesViewStateAndProperties`:** Set the mock to return a successful `WeatherResponse` with temperature 20.0 and weathercode 0. Call `viewModel.fetchWeather()`. Use `XCTestExpectation` with `DispatchQueue.main.asyncAfter` (0.1s delay, matching existing pattern). Assert `viewModel.isLoading == false`, `viewModel.temperatureText == "20.0 C"`, `viewModel.conditionText == "Clear sky"`, `viewModel.errorMessage == nil`.
  4. **Test: `testFetchWeather_failure_setsErrorMessage`:** Set the mock to return a failure with a dummy `NSError`. Call `viewModel.fetchWeather()`. Assert `viewModel.isLoading == false`, `viewModel.temperatureText == nil`, `viewModel.conditionText == nil`, `viewModel.errorMessage != nil`.
  5. **Test: `testFetchWeather_setsLoadingState`:** Set the mock to return success but do not wait for completion. Immediately after calling `viewModel.fetchWeather()`, assert `viewModel.isLoading == true`. (Note: since the mock completion is synchronous on main queue with `DispatchQueue.main.async`, the loading state may transition immediately. Adjust assertion timing as needed, or test that loading is true at some point during the fetch.)
  6. **Test: `testInitialState_isIdle`:** After creating the ViewModel (without calling `fetchWeather()`), assert `viewModel.isLoading == false`, `viewModel.temperatureText == nil`, `viewModel.conditionText == nil`, `viewModel.errorMessage == nil`.
  Remove all existing user-related tests (`testLoadUsers_populatesUsersAndSetsUserName`, `testNextUser_cyclesThroughUsers`, `testNextUser_doesNothingWhenUsersEmpty`).

- **Affected files:**
  - REWRITE `/Users/qinlinxie/Desktop/Dev/TechDemo/TechDemoTests/TechDemoTests.swift`

- **Agent:** test_agent

- **Input Context:**
  - `WeatherServiceProtocol` signature: `protocol WeatherServiceProtocol { func fetchWeather(completion: @escaping (Result<WeatherResponse, Error>) -> Void) }` (from Task 3)
  - `HomeViewModel` public surface (from Task 4):
    - `var viewState: ViewState`
    - `var isLoading: Bool`
    - `var temperatureText: String?`
    - `var conditionText: String?`
    - `var errorMessage: String?`
    - `func fetchWeather()`
    - `init(weatherService: WeatherServiceProtocol = WeatherService())`
  - `WeatherResponse` construction: `WeatherResponse(currentWeather: CurrentWeather(temperature: 20.0, windspeed: 10.0, winddirection: 180, weathercode: 0, isDay: 1, time: "2026-04-23T12:00", interval: 900))`
  - `CurrentWeather.conditionDescription` for weathercode 0 returns `"Clear sky"`

- **Output:**
  - `TechDemoTests.swift` containing `MockWeatherService` and `HomeViewModelTests` with at least 4 test methods covering success, failure, loading state, and initial state.

---

## 4. Execution Order

```
Step 1: model_agent    -- Task 1 (Create WeatherResponse model)
Step 2: model_agent    -- Task 2 (Delete old User model and UserService)
Step 3: service_agent  -- Task 3 (Create WeatherService and protocol)  [depends on Task 1]
Step 4: viewmodel_agent -- Task 4 (Rewrite HomeViewModel)              [depends on Task 1, Task 3]
Step 5: ui_agent       -- Task 5 (Rewrite HomeViewController)         [depends on Task 4]
Step 6: test_agent     -- Task 6 (Rewrite unit tests)                  [depends on Task 4]
Step 7: reviewer_agent -- Full build + test verification               [depends on all above]
```

Tasks 1 and 2 are independent of each other and can run in parallel. Task 2 can also run in parallel with Task 3 since the deleted files are not referenced by the new code. Task 5 and Task 6 are independent of each other and can run in parallel after Task 4 completes.

Dependency graph:
- Task 1 -- no dependencies
- Task 2 -- no dependencies
- Task 3 -- depends on Task 1 (needs `WeatherResponse` type)
- Task 4 -- depends on Task 1 (needs model types) and Task 3 (needs `WeatherServiceProtocol`)
- Task 5 -- depends on Task 4 (needs ViewModel public surface)
- Task 6 -- depends on Task 4 (needs ViewModel and protocol for mock)
- Task 7 -- depends on Tasks 1-6 (full integration verification)

## 5. Risk Notes

- **`@MainActor` default isolation and `URLSession`:** The project has `SWIFT_DEFAULT_ACTOR_ISOLATION = MainActor`, meaning `WeatherService` is implicitly `@MainActor`. `URLSession.dataTask` callbacks run on a background queue. The service must properly handle the actor boundary -- either by dispatching the completion to main via `DispatchQueue.main.async`, or by using `nonisolated` on the callback and `@MainActor` on the completion invocation. If not handled correctly, the compiler will emit errors about sending non-Sendable types across actor boundaries. The `Result<WeatherResponse, Error>` type is `Sendable` since both `WeatherResponse` (a struct of `Codable` primitives) and `Error` are `Sendable`, so this should compile cleanly.

- **Network dependency in tests:** Tests must NOT hit the real network. The `WeatherServiceProtocol` extraction in Task 3 and the `MockWeatherService` in Task 6 ensure tests are fully deterministic. If an agent forgets to inject the mock, tests will be flaky or fail in environments without network access.

- **`withObservationTracking` re-registration pattern:** The `@Observable` framework's `withObservationTracking` only fires once per registration. The `onChange` callback must re-register observation to get continuous updates. If the UI agent implements this incorrectly, the UI will update on the first state change but not subsequent ones. This is a well-known pattern but easy to get wrong.

- **`ViewState` enum as `@Observable` stored property:** The Observation framework tracks changes to stored properties. When `viewState` is reassigned from `.loading` to `.loaded(...)`, the framework detects the change and notifies observers. This works correctly because `ViewState` is stored as a single property -- the entire enum value is replaced, not mutated in place.

- **Thread safety of mock service in tests:** The `MockWeatherService` must call its completion on the main queue (matching the real `WeatherService` contract) to avoid test flakiness. If the mock calls completion synchronously, the ViewModel state may change before the test can assert the loading state. The mock should mimic the real service's async dispatch behavior.

- **Open-Meteo API availability:** The API is free and does not require authentication, but it could be temporarily unavailable. The error handling in the ViewModel and the error state in the UI mitigate this. The UI should show a user-friendly error message and allow retry via the Refresh button.

- **WMO weather code coverage:** The `conditionDescription` computed property handles the most common WMO codes but may not cover all possible values. The default case returns "Unknown" to prevent crashes. This is acceptable for the initial implementation.

- **No `project.pbxproj` edits needed:** The Xcode project uses `PBXFileSystemSynchronizedRootGroup` (objectVersion 77), which automatically synchronizes files on disk with the project. Adding files to `TechDemo/Models/`, `TechDemo/Services/`, or deleting files from those directories will be reflected in Xcode without manual pbxproj changes. Agents should NOT modify `project.pbxproj`.
