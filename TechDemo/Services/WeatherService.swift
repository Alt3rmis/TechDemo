//
//  WeatherService.swift
//  TechDemo
//

import Foundation

protocol WeatherServiceProtocol {
    func fetchWeather(for city: City, completion: @Sendable @escaping (Result<WeatherResponse, Error>) -> Void)
}

final class WeatherService: WeatherServiceProtocol {

    func fetchWeather(for city: City, completion: @escaping @Sendable (Result<WeatherResponse, Error>) -> Void) {
        let urlString = "https://api.open-meteo.com/v1/forecast?latitude=\(city.latitude)&longitude=\(city.longitude)&current_weather=true"
        guard let url = URL(string: urlString) else {
            let error = NSError(domain: "WeatherService", code: -1, userInfo: [NSLocalizedDescriptionKey: "Invalid URL"])
            completion(.failure(error))
            return
        }

        let task = URLSession.shared.dataTask(with: url) { data, _, error in
            DispatchQueue.main.async {
                if let error {
                    completion(.failure(error))
                    return
                }

                guard let data else {
                    let noDataError = NSError(domain: "WeatherService", code: -1, userInfo: [NSLocalizedDescriptionKey: "No data received"])
                    completion(.failure(noDataError))
                    return
                }

                do {
                    let decoded = try JSONDecoder().decode(WeatherResponse.self, from: data)
                    completion(.success(decoded))
                } catch {
                    completion(.failure(error))
                }
            }
        }
        task.resume()
    }
}
