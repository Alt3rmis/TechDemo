//
//  CityPersistenceService.swift
//  TechDemo
//

import Foundation

protocol CityPersistenceServiceProtocol {
    func loadSelectedCity() -> City?
    func saveSelectedCity(_ city: City)
}

final class CityPersistenceService: CityPersistenceServiceProtocol {

    private let userDefaults: UserDefaults
    private let storageKey = "selectedCityName"

    init(userDefaults: UserDefaults = .standard) {
        self.userDefaults = userDefaults
    }

    func loadSelectedCity() -> City? {
        guard let name = userDefaults.string(forKey: storageKey) else {
            return nil
        }
        return City.allCities.first { $0.name == name }
    }

    func saveSelectedCity(_ city: City) {
        userDefaults.set(city.name, forKey: storageKey)
    }
}
