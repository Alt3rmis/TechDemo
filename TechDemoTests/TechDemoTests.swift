//
//  TechDemoTests.swift
//  TechDemoTests
//

import XCTest
@testable import TechDemo

// MARK: - Mock

final class MockWeatherService: WeatherServiceProtocol {

    var result: Result<WeatherResponse, Error>?

    private(set) var fetchWeatherCallCount: Int = 0

    func fetchWeather(completion: @Sendable @escaping (Result<WeatherResponse, Error>) -> Void) {
        fetchWeatherCallCount += 1
        DispatchQueue.main.async { [self] in
            guard let result else {
                XCTFail("MockWeatherService.result was not set before fetchWeather was called")
                return
            }
            completion(result)
        }
    }
}

// MARK: - Tests

final class HomeViewModelTests: XCTestCase {

    private var viewModel: HomeViewModel!
    private var mockService: MockWeatherService!

    override func setUp() {
        super.setUp()
        mockService = MockWeatherService()
        viewModel = HomeViewModel(weatherService: mockService)
    }

    override func tearDown() {
        viewModel = nil
        mockService = nil
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

        XCTAssertEqual(viewModel.temperatureText, "20.0°C", "temperatureText should format temperature as '20.0°C'")
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
