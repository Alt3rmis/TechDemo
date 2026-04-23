//
//  HomeViewController.swift
//  TechDemo
//

import UIKit

class HomeViewController: UIViewController {

    private let viewModel = HomeViewModel()

    private lazy var nameLabel: UILabel = {
        let label = UILabel()
        label.textAlignment = .center
        label.font = UIFont.systemFont(ofSize: 24, weight: .medium)
        label.numberOfLines = 0
        label.text = "No user loaded"
        label.translatesAutoresizingMaskIntoConstraints = false
        return label
    }()

    private lazy var loadButton: UIButton = {
        let button = UIButton(type: .system)
        button.setTitle("Load User", for: .normal)
        button.titleLabel?.font = UIFont.systemFont(ofSize: 18, weight: .semibold)
        button.translatesAutoresizingMaskIntoConstraints = false
        button.addTarget(self, action: #selector(loadUserTapped), for: .touchUpInside)
        return button
    }()

    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
    }

    private func setupUI() {
        view.backgroundColor = .systemBackground
        title = "Home"

        view.addSubview(nameLabel)
        view.addSubview(loadButton)

        NSLayoutConstraint.activate([
            nameLabel.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            nameLabel.centerYAnchor.constraint(equalTo: view.centerYAnchor, constant: -40),
            nameLabel.leadingAnchor.constraint(greaterThanOrEqualTo: view.leadingAnchor, constant: 20),
            nameLabel.trailingAnchor.constraint(lessThanOrEqualTo: view.trailingAnchor, constant: -20),

            loadButton.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            loadButton.topAnchor.constraint(equalTo: nameLabel.bottomAnchor, constant: 32),
        ])
    }

    @objc private func loadUserTapped() {
        viewModel.loadUser()
        nameLabel.text = viewModel.userName
    }
}
