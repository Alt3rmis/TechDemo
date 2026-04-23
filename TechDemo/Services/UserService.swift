//
//  UserService.swift
//  TechDemo
//

import Foundation

class UserService {

    func fetchUser(completion: @escaping (User) -> Void) {
        let user = User(
            id: "1",
            name: "Jane Doe",
            email: "jane.doe@example.com"
        )
        completion(user)
    }
}
