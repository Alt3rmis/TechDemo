//
//  WeatherResponse.swift
//  TechDemo
//

import Foundation

struct CurrentWeather: Codable, Sendable {

    let temperature: Double
    let windspeed: Double
    let winddirection: Int
    let weathercode: Int
    let isDay: Int
    let time: String
    let interval: Int

    var conditionDescription: String {
        switch weathercode {
        case 0:
            return "Clear sky"
        case 1, 2, 3:
            return "Partly cloudy"
        case 45, 48:
            return "Fog"
        case 51, 53, 55:
            return "Drizzle"
        case 61, 63, 65:
            return "Rain"
        case 71, 73, 75:
            return "Snowfall"
        case 80, 81, 82:
            return "Rain showers"
        case 95:
            return "Thunderstorm"
        default:
            return "Unknown"
        }
    }

    enum CodingKeys: String, CodingKey {
        case temperature
        case windspeed
        case winddirection
        case weathercode
        case isDay = "is_day"
        case time
        case interval
    }
}

struct WeatherResponse: Codable, Sendable {

    let currentWeather: CurrentWeather

    enum CodingKeys: String, CodingKey {
        case currentWeather = "current_weather"
    }
}
