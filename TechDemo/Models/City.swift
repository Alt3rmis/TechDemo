//
//  City.swift
//  TechDemo
//

import Foundation

struct City: Equatable, Identifiable, Sendable {
    let name: String
    let chineseName: String
    let latitude: Double
    let longitude: Double

    var id: String { name }

    static let allCities: [City] = [
        City(name: "Chengdu", chineseName: "成都", latitude: 30.5728, longitude: 104.0668),
        City(name: "Beijing", chineseName: "北京", latitude: 39.9042, longitude: 116.4074),
        City(name: "Shanghai", chineseName: "上海", latitude: 31.2304, longitude: 121.4737),
        City(name: "Shenzhen", chineseName: "深圳", latitude: 22.5431, longitude: 114.0579)
    ]

    static let defaultCity: City = allCities[2]
}
