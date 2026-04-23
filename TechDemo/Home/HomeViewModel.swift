//
//  HomeViewModel.swift
//  TechDemo
//

import Foundation

@Observable
class HomeViewModel {

    var userName: String = ""

    private let userService: UserService

    init(userService: UserService = UserService()) {
        self.userService = userService
    }

    func loadUser() {
        userService.fetchUser { [weak self] user in
            self?.userName = user.name
        }
    }
}
