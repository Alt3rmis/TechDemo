//
//  CitySelectionViewModel.swift
//  TechDemo
//

import Foundation

@Observable
final class CitySelectionViewModel {

    let cities: [City]
    private(set) var selectedCity: City

    init(selectedCity: City) {
        self.cities = City.allCities
        self.selectedCity = selectedCity
    }

    func selectCity(at index: Int) {
        guard index >= 0, index < cities.count else { return }
        selectedCity = cities[index]
    }

    func isCitySelected(at index: Int) -> Bool {
        guard index >= 0, index < cities.count else { return false }
        return cities[index] == selectedCity
    }
}
