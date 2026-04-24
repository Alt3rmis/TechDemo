//
//  HomeViewModel.swift
//  TechDemo
//

import Foundation

enum ViewState: Sendable {
    case idle
    case loading
    case loaded(weather: CurrentWeather)
    case error(message: String)
}

@Observable
final class HomeViewModel {

    var viewState: ViewState = .idle

    private let weatherService: WeatherServiceProtocol
    private let persistenceService: CityPersistenceServiceProtocol
    private(set) var currentCity: City
    private var fetchGeneration: Int = 0

    var cityName: String {
        return "\(currentCity.chineseName) \(currentCity.name)"
    }

    init(weatherService: WeatherServiceProtocol = WeatherService(),
         persistenceService: CityPersistenceServiceProtocol = CityPersistenceService()) {
        self.weatherService = weatherService
        self.persistenceService = persistenceService
        self.currentCity = persistenceService.loadSelectedCity() ?? City.defaultCity
    }

    var isLoading: Bool {
        if case .loading = viewState {
            return true
        }
        return false
    }

    var temperatureText: String? {
        if case .loaded(let weather) = viewState {
            return "\(weather.temperature)\u{00B0}C"
        }
        return nil
    }

    var conditionText: String? {
        if case .loaded(let weather) = viewState {
            return weather.conditionDescription
        }
        return nil
    }

    var errorMessage: String? {
        if case .error(let message) = viewState {
            return message
        }
        return nil
    }

    func selectCity(_ city: City) {
        viewState = .loading
        currentCity = city
        persistenceService.saveSelectedCity(city)
        fetchWeather()
    }

    func fetchWeather() {
        viewState = .loading
        fetchGeneration += 1
        let generation = fetchGeneration
        weatherService.fetchWeather(for: currentCity) { [weak self] result in
            guard let self, self.fetchGeneration == generation else { return }
            switch result {
            case .success(let response):
                self.viewState = .loaded(weather: response.currentWeather)
            case .failure(let error):
                self.viewState = .error(message: error.localizedDescription)
            }
        }
    }
}
