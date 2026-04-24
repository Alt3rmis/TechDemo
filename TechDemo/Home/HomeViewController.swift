//
//  HomeViewController.swift
//  TechDemo
//

import UIKit

final class HomeViewController: UIViewController {

    private let viewModel = HomeViewModel()

    private lazy var cityLabel: UILabel = {
        let label = UILabel()
        label.textAlignment = .center
        label.font = UIFont.systemFont(ofSize: 28, weight: .bold)
        label.translatesAutoresizingMaskIntoConstraints = false
        return label
    }()

    private lazy var temperatureLabel: UILabel = {
        let label = UILabel()
        label.textAlignment = .center
        label.font = UIFont.systemFont(ofSize: 64, weight: .light)
        label.translatesAutoresizingMaskIntoConstraints = false
        return label
    }()

    private lazy var conditionLabel: UILabel = {
        let label = UILabel()
        label.textAlignment = .center
        label.font = UIFont.systemFont(ofSize: 20, weight: .regular)
        label.translatesAutoresizingMaskIntoConstraints = false
        return label
    }()

    private lazy var refreshButton: UIButton = {
        let button = UIButton(type: .system)
        button.setTitle("Refresh", for: .normal)
        button.titleLabel?.font = UIFont.systemFont(ofSize: 18, weight: .semibold)
        button.translatesAutoresizingMaskIntoConstraints = false
        button.addTarget(self, action: #selector(refreshTapped), for: .touchUpInside)
        return button
    }()

    private lazy var activityIndicator: UIActivityIndicatorView = {
        let indicator = UIActivityIndicatorView(style: .medium)
        indicator.hidesWhenStopped = true
        indicator.translatesAutoresizingMaskIntoConstraints = false
        return indicator
    }()

    private lazy var errorLabel: UILabel = {
        let label = UILabel()
        label.textAlignment = .center
        label.font = UIFont.systemFont(ofSize: 16, weight: .regular)
        label.textColor = .systemRed
        label.numberOfLines = 0
        label.translatesAutoresizingMaskIntoConstraints = false
        return label
    }()

    private lazy var settingsButton: UIBarButtonItem = {
        let button = UIBarButtonItem(
            image: UIImage(systemName: "gearshape"),
            style: .plain,
            target: self,
            action: #selector(settingsTapped)
        )
        return button
    }()

    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
        observeViewModel()
        viewModel.fetchWeather()
        updateUI()
    }

    private func setupUI() {
        view.backgroundColor = .systemBackground
        title = "Weather"
        navigationItem.rightBarButtonItem = settingsButton

        view.addSubview(cityLabel)
        view.addSubview(temperatureLabel)
        view.addSubview(conditionLabel)
        view.addSubview(refreshButton)
        view.addSubview(activityIndicator)
        view.addSubview(errorLabel)

        NSLayoutConstraint.activate([
            cityLabel.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            cityLabel.topAnchor.constraint(equalTo: view.safeAreaLayoutGuide.topAnchor, constant: 40),

            temperatureLabel.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            temperatureLabel.topAnchor.constraint(equalTo: cityLabel.bottomAnchor, constant: 16),

            conditionLabel.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            conditionLabel.topAnchor.constraint(equalTo: temperatureLabel.bottomAnchor, constant: 8),

            refreshButton.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            refreshButton.topAnchor.constraint(equalTo: conditionLabel.bottomAnchor, constant: 32),

            activityIndicator.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            activityIndicator.centerYAnchor.constraint(equalTo: view.centerYAnchor, constant: 40),

            errorLabel.centerXAnchor.constraint(equalTo: view.centerXAnchor),
            errorLabel.topAnchor.constraint(equalTo: cityLabel.bottomAnchor, constant: 16),
            errorLabel.leadingAnchor.constraint(greaterThanOrEqualTo: view.leadingAnchor, constant: 20),
            errorLabel.trailingAnchor.constraint(lessThanOrEqualTo: view.trailingAnchor, constant: -20),
        ])
    }

    private func observeViewModel() {
        withObservationTracking {
            _ = viewModel.viewState
            _ = viewModel.cityName
        } onChange: { [weak self] in
            DispatchQueue.main.async {
                self?.updateUI()
                self?.observeViewModel()
            }
        }
    }

    private func updateUI() {
        let isLoading = viewModel.isLoading
        let temperature = viewModel.temperatureText
        let condition = viewModel.conditionText
        let error = viewModel.errorMessage

        cityLabel.text = viewModel.cityName

        if isLoading {
            activityIndicator.startAnimating()
            temperatureLabel.isHidden = true
            conditionLabel.isHidden = true
            errorLabel.isHidden = true
        } else if let error {
            activityIndicator.stopAnimating()
            temperatureLabel.isHidden = true
            conditionLabel.isHidden = true
            errorLabel.isHidden = false
            errorLabel.text = error
        } else {
            activityIndicator.stopAnimating()
            errorLabel.isHidden = true

            if let temperature {
                temperatureLabel.isHidden = false
                temperatureLabel.text = temperature
            } else {
                temperatureLabel.isHidden = true
            }

            if let condition {
                conditionLabel.isHidden = false
                conditionLabel.text = condition
            } else {
                conditionLabel.isHidden = true
            }
        }
    }

    @objc private func refreshTapped() {
        viewModel.fetchWeather()
        updateUI()
    }

    @objc private func settingsTapped() {
        let selectionViewModel = CitySelectionViewModel(selectedCity: viewModel.currentCity)
        let selectionVC = CitySelectionViewController(viewModel: selectionViewModel) { [weak self] city in
            self?.viewModel.selectCity(city)
        }
        navigationController?.pushViewController(selectionVC, animated: true)
    }
}
