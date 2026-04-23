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
    private var fetchGeneration: Int = 0

    init(weatherService: WeatherServiceProtocol = WeatherService()) {
        self.weatherService = weatherService
    }

    var isLoading: Bool {
        if case .loading = viewState {
            return true
        }
        return false
    }

    var temperatureText: String? {
        if case .loaded(let weather) = viewState {
            return "\(weather.temperature)°C"
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

    func fetchWeather() {
        viewState = .loading
        fetchGeneration += 1
        let generation = fetchGeneration
        weatherService.fetchWeather { [weak self] result in
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
