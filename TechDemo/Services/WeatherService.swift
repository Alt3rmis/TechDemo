//
//  WeatherService.swift
//  TechDemo
//

import Foundation

protocol WeatherServiceProtocol {
    func fetchWeather(completion: @Sendable @escaping (Result<WeatherResponse, Error>) -> Void)
}

final class WeatherService: WeatherServiceProtocol {

    private let url = URL(string: "https://api.open-meteo.com/v1/forecast?latitude=31.2304&longitude=121.4737&current_weather=true")!

    func fetchWeather(completion: @escaping @Sendable (Result<WeatherResponse, Error>) -> Void) {
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
