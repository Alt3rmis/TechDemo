//
//  TechDemoTests.swift
//  TechDemoTests
//

import XCTest
@testable import TechDemo

final class HomeViewModelTests: XCTestCase {

    private var viewModel: HomeViewModel!

    override func setUp() {
        super.setUp()
        viewModel = HomeViewModel()
    }

    override func tearDown() {
        viewModel = nil
        super.tearDown()
    }

    func testLoadUser_updatesUserName() {
        let expectation = XCTestExpectation(description: "userName is updated")

        viewModel.loadUser()

        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
            XCTAssertEqual(self.viewModel.userName, "Jane Doe")
            expectation.fulfill()
        }

        wait(for: [expectation], timeout: 1.0)
    }
}
