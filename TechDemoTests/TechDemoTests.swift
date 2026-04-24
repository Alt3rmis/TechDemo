//
//  TechDemoTests.swift
//  TechDemoTests
//

import XCTest
@testable import TechDemo

// MARK: - Mocks

final class MockWeatherService: WeatherServiceProtocol {

    var result: Result<WeatherResponse, Error>?

    private(set) var fetchWeatherCallCount: Int = 0
    private(set) var lastCity: City?

    func fetchWeather(for city: City, completion: @Sendable @escaping (Result<WeatherResponse, Error>) -> Void) {
        fetchWeatherCallCount += 1
        lastCity = city
        DispatchQueue.main.async { [self] in
            guard let result else {
                XCTFail("MockWeatherService.result was not set before fetchWeather was called")
                return
            }
            completion(result)
        }
    }
}

final class MockCityPersistenceService: CityPersistenceServiceProtocol {

    var savedCity: City?

    private(set) var saveCallCount: Int = 0

    func loadSelectedCity() -> City? {
        return savedCity
    }

    func saveSelectedCity(_ city: City) {
        saveCallCount += 1
        savedCity = city
    }
}

// MARK: - HomeViewModelTests

final class HomeViewModelTests: XCTestCase {

    private var viewModel: HomeViewModel!
    private var mockService: MockWeatherService!
    private var mockPersistence: MockCityPersistenceService!

    override func setUp() {
        super.setUp()
        mockService = MockWeatherService()
        mockPersistence = MockCityPersistenceService()
        viewModel = HomeViewModel(weatherService: mockService, persistenceService: mockPersistence)
    }

    override func tearDown() {
        viewModel = nil
        mockService = nil
        mockPersistence = nil
        super.tearDown()
    }

    // MARK: - Initial State

    func testInitialState_isIdle() {
        guard case .idle = viewModel.viewState else {
            XCTFail("viewState should be .idle on creation, got \(viewModel.viewState)")
            return
        }
        XCTAssertFalse(viewModel.isLoading, "isLoading should be false before any fetch")
        XCTAssertNil(viewModel.temperatureText, "temperatureText should be nil before any fetch")
        XCTAssertNil(viewModel.conditionText, "conditionText should be nil before any fetch")
        XCTAssertNil(viewModel.errorMessage, "errorMessage should be nil before any fetch")
    }

    // MARK: - Loading State

    func testFetchWeather_setsLoadingState() {
        // Arrange: configure mock to succeed, but we will assert *before* the async completion fires
        let weather = CurrentWeather(
            temperature: 20.0,
            windspeed: 10.0,
            winddirection: 180,
            weathercode: 0,
            isDay: 1,
            time: "2026-04-23T12:00",
            interval: 900
        )
        mockService.result = .success(WeatherResponse(currentWeather: weather))

        // Act: calling fetchWeather synchronously sets .loading before the async completion fires
        viewModel.fetchWeather()

        // Assert: immediately after calling, state should be .loading because
        // the mock dispatches via DispatchQueue.main.async which has not executed yet
        XCTAssertTrue(viewModel.isLoading, "isLoading should be true immediately after calling fetchWeather")
        guard case .loading = viewModel.viewState else {
            XCTFail("viewState should be .loading immediately after calling fetchWeather, got \(viewModel.viewState)")
            return
        }
        XCTAssertNil(viewModel.temperatureText, "temperatureText should be nil while loading")
        XCTAssertNil(viewModel.conditionText, "conditionText should be nil while loading")
        XCTAssertNil(viewModel.errorMessage, "errorMessage should be nil while loading")
    }

    // MARK: - Success Path

    func testFetchWeather_success_updatesViewStateAndProperties() {
        // Arrange
        let weather = CurrentWeather(
            temperature: 20.0,
            windspeed: 10.0,
            winddirection: 180,
            weathercode: 0,
            isDay: 1,
            time: "2026-04-23T12:00",
            interval: 900
        )
        mockService.result = .success(WeatherResponse(currentWeather: weather))

        let expectation = XCTestExpectation(description: "fetchWeather completes successfully")

        // Act
        viewModel.fetchWeather()

        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
            expectation.fulfill()
        }

        wait(for: [expectation], timeout: 1.0)

        // Assert
        XCTAssertFalse(viewModel.isLoading, "isLoading should be false after successful fetch")
        XCTAssertNil(viewModel.errorMessage, "errorMessage should be nil on success")

        if case .loaded(let loadedWeather) = viewModel.viewState {
            XCTAssertEqual(loadedWeather.temperature, 20.0, "Loaded weather should have the correct temperature")
            XCTAssertEqual(loadedWeather.weathercode, 0, "Loaded weather should have the correct weathercode")
        } else {
            XCTFail("viewState should be .loaded after successful fetch, got \(viewModel.viewState)")
        }

        XCTAssertEqual(viewModel.temperatureText, "20.0\u{00B0}C", "temperatureText should format temperature as '20.0\u{00B0}C'")
        XCTAssertEqual(viewModel.conditionText, "Clear sky", "conditionText should return 'Clear sky' for weathercode 0")

        XCTAssertEqual(mockService.fetchWeatherCallCount, 1, "fetchWeather should be called exactly once")
    }

    // MARK: - Failure Path

    func testFetchWeather_failure_setsErrorMessage() {
        // Arrange
        let testError = NSError(
            domain: "TestErrorDomain",
            code: 42,
            userInfo: [NSLocalizedDescriptionKey: "Network connection failed"]
        )
        mockService.result = .failure(testError)

        let expectation = XCTestExpectation(description: "fetchWeather completes with failure")

        // Act
        viewModel.fetchWeather()

        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
            expectation.fulfill()
        }

        wait(for: [expectation], timeout: 1.0)

        // Assert
        XCTAssertFalse(viewModel.isLoading, "isLoading should be false after failed fetch")
        XCTAssertNil(viewModel.temperatureText, "temperatureText should be nil on failure")
        XCTAssertNil(viewModel.conditionText, "conditionText should be nil on failure")

        if case .error(let message) = viewModel.viewState {
            XCTAssertEqual(message, "Network connection failed", "Error message should match the error's localizedDescription")
        } else {
            XCTFail("viewState should be .error after failed fetch, got \(viewModel.viewState)")
        }

        XCTAssertEqual(viewModel.errorMessage, "Network connection failed", "errorMessage should match the error's localizedDescription")
        XCTAssertEqual(mockService.fetchWeatherCallCount, 1, "fetchWeather should be called exactly once")
    }
}

// MARK: - CityPersistenceServiceTests

final class CityPersistenceServiceTests: XCTestCase {

    private var testUserDefaults: UserDefaults!
    private var persistenceService: CityPersistenceService!

    override func setUp() {
        super.setUp()
        testUserDefaults = UserDefaults(suiteName: "CityPersistenceServiceTests")
        testUserDefaults.removePersistentDomain(forName: "CityPersistenceServiceTests")
        persistenceService = CityPersistenceService(userDefaults: testUserDefaults)
    }

    override func tearDown() {
        testUserDefaults.removePersistentDomain(forName: "CityPersistenceServiceTests")
        testUserDefaults = nil
        persistenceService = nil
        super.tearDown()
    }

    func testLoadCity_whenNothingSaved_returnsNil() {
        // Act
        let loadedCity = persistenceService.loadSelectedCity()

        // Assert
        XCTAssertNil(loadedCity, "loadSelectedCity should return nil when nothing has been saved")
    }

    func testSaveAndLoadCity_roundTripsCorrectly() {
        // Arrange
        let city = City.allCities[1] // Beijing

        // Act
        persistenceService.saveSelectedCity(city)
        let loadedCity = persistenceService.loadSelectedCity()

        // Assert
        XCTAssertEqual(loadedCity, city, "Loaded city should equal the saved city")
    }

    func testSaveCity_overwritesPreviousValue() {
        // Arrange
        let firstCity = City.allCities[2] // Shanghai
        let secondCity = City.allCities[1] // Beijing

        // Act
        persistenceService.saveSelectedCity(firstCity)
        persistenceService.saveSelectedCity(secondCity)
        let loadedCity = persistenceService.loadSelectedCity()

        // Assert
        XCTAssertEqual(loadedCity, secondCity, "Loaded city should be the most recently saved city (Beijing), not the first (Shanghai)")
    }
}

// MARK: - HomeViewModelCityTests

final class HomeViewModelCityTests: XCTestCase {

    private var mockService: MockWeatherService!
    private var mockPersistence: MockCityPersistenceService!
    private var viewModel: HomeViewModel!

    override func setUp() {
        super.setUp()
        mockService = MockWeatherService()
        mockPersistence = MockCityPersistenceService()
    }

    override func tearDown() {
        viewModel = nil
        mockService = nil
        mockPersistence = nil
        super.tearDown()
    }

    func testInitialCityName_isDefaultWhenNoPersistence() {
        // Arrange: mockPersistence.savedCity is nil by default, so HomeViewModel falls back to City.defaultCity
        viewModel = HomeViewModel(weatherService: mockService, persistenceService: mockPersistence)

        // Assert: cityName = "\(chineseName) \(name)" => "\u{4E0A}\u{6D77} Shanghai"
        let expectedCityName = "\(City.defaultCity.chineseName) \(City.defaultCity.name)"
        XCTAssertEqual(viewModel.cityName, expectedCityName, "cityName should be the default city (Shanghai) when persistence returns nil")
    }

    func testInitialCityName_usesPersistedCity() {
        // Arrange: pre-populate persistence with Beijing
        let beijing = City.allCities[1]
        mockPersistence.savedCity = beijing
        viewModel = HomeViewModel(weatherService: mockService, persistenceService: mockPersistence)

        // Assert: cityName = "\(chineseName) \(name)" => "\u{5317}\u{4EAC} Beijing"
        let expectedCityName = "\(beijing.chineseName) \(beijing.name)"
        XCTAssertEqual(viewModel.cityName, expectedCityName, "cityName should be the persisted city (Beijing)")
    }

    func testSelectCity_updatesCityNameAndPersists() {
        // Arrange
        viewModel = HomeViewModel(weatherService: mockService, persistenceService: mockPersistence)
        mockService.result = .success(WeatherResponse(currentWeather: CurrentWeather(
            temperature: 15.0, windspeed: 5.0, winddirection: 90,
            weathercode: 1, isDay: 1, time: "2026-04-23T12:00", interval: 900
        )))
        let chengdu = City.allCities[0] // Chengdu

        // Act
        viewModel.selectCity(chengdu)

        // Assert: cityName is updated synchronously before the async fetch completes
        let expectedCityName = "\(chengdu.chineseName) \(chengdu.name)"
        XCTAssertEqual(viewModel.cityName, expectedCityName, "cityName should update to Chengdu after selectCity")
        XCTAssertEqual(mockPersistence.savedCity, chengdu, "selectCity should persist the selected city")
        XCTAssertEqual(mockPersistence.saveCallCount, 1, "saveSelectedCity should be called exactly once")
    }

    func testSelectCity_triggersWeatherFetch() {
        // Arrange
        viewModel = HomeViewModel(weatherService: mockService, persistenceService: mockPersistence)
        mockService.result = .success(WeatherResponse(currentWeather: CurrentWeather(
            temperature: 25.0, windspeed: 8.0, winddirection: 270,
            weathercode: 3, isDay: 1, time: "2026-04-23T12:00", interval: 900
        )))
        let shenzhen = City.allCities[3] // Shenzhen

        // Act
        viewModel.selectCity(shenzhen)

        // Assert
        XCTAssertEqual(mockService.fetchWeatherCallCount, 1, "selectCity should trigger exactly one weather fetch")
        XCTAssertEqual(mockService.lastCity, shenzhen, "The weather fetch should be for Shenzhen")
    }
}

// MARK: - CitySelectionViewModelTests

final class CitySelectionViewModelTests: XCTestCase {

    private var viewModel: CitySelectionViewModel!

    override func setUp() {
        super.setUp()
        viewModel = CitySelectionViewModel(selectedCity: City.defaultCity)
    }

    override func tearDown() {
        viewModel = nil
        super.tearDown()
    }

    func testCities_containsAllFourCities() {
        // Assert
        XCTAssertEqual(viewModel.cities.count, 4, "cities should contain exactly 4 entries")
    }

    func testIsCitySelected_returnsTrueForSelectedCity() {
        // Arrange: defaultCity is Shanghai (index 2)
        let shanghaiIndex = 2

        // Act & Assert
        XCTAssertTrue(viewModel.isCitySelected(at: shanghaiIndex), "isCitySelected should return true for the currently selected city (Shanghai at index 2)")
        XCTAssertFalse(viewModel.isCitySelected(at: 0), "isCitySelected should return false for a non-selected city (Chengdu at index 0)")
    }

    func testSelectCity_updatesSelectedCity() {
        // Arrange
        let beijingIndex = 1

        // Act
        viewModel.selectCity(at: beijingIndex)

        // Assert
        XCTAssertEqual(viewModel.selectedCity, City.allCities[beijingIndex], "selectedCity should be updated to Beijing after selectCity(at: 1)")
    }

    func testSelectCity_withInvalidIndex_doesNotChangeSelectedCity() {
        // Arrange: selectedCity starts as defaultCity (Shanghai)
        let originalCity = viewModel.selectedCity

        // Act
        viewModel.selectCity(at: -1)
        viewModel.selectCity(at: 99)

        // Assert
        XCTAssertEqual(viewModel.selectedCity, originalCity, "selectedCity should remain unchanged after out-of-bounds indices")
    }

    func testIsCitySelected_withInvalidIndex_returnsFalse() {
        // Assert
        XCTAssertFalse(viewModel.isCitySelected(at: -1), "isCitySelected should return false for negative index")
        XCTAssertFalse(viewModel.isCitySelected(at: 100), "isCitySelected should return false for index beyond bounds")
    }
}
