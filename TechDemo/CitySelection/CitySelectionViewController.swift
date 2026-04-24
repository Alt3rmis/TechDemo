//
//  CitySelectionViewController.swift
//  TechDemo
//

import UIKit

final class CitySelectionViewController: UIViewController {

    private let viewModel: CitySelectionViewModel
    private let onCitySelected: ((City) -> Void)?

    private lazy var tableView: UITableView = {
        let tv = UITableView(frame: .zero, style: .plain)
        tv.translatesAutoresizingMaskIntoConstraints = false
        tv.delegate = self
        tv.dataSource = self
        tv.register(UITableViewCell.self, forCellReuseIdentifier: "CityCell")
        return tv
    }()

    init(viewModel: CitySelectionViewModel, onCitySelected: ((City) -> Void)? = nil) {
        self.viewModel = viewModel
        self.onCitySelected = onCitySelected
        super.init(nibName: nil, bundle: nil)
    }

    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }

    override func viewDidLoad() {
        super.viewDidLoad()
        setupUI()
        observeViewModel()
    }

    private func setupUI() {
        title = "Select City"
        view.backgroundColor = .systemBackground

        view.addSubview(tableView)

        NSLayoutConstraint.activate([
            tableView.topAnchor.constraint(equalTo: view.topAnchor),
            tableView.bottomAnchor.constraint(equalTo: view.bottomAnchor),
            tableView.leadingAnchor.constraint(equalTo: view.leadingAnchor),
            tableView.trailingAnchor.constraint(equalTo: view.trailingAnchor)
        ])
    }

    private func observeViewModel() {
        withObservationTracking {
            _ = viewModel.selectedCity
        } onChange: { [weak self] in
            DispatchQueue.main.async {
                self?.tableView.reloadData()
                self?.observeViewModel()
            }
        }
    }
}

extension CitySelectionViewController: UITableViewDataSource {

    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return viewModel.cities.count
    }

    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cell = tableView.dequeueReusableCell(withIdentifier: "CityCell", for: indexPath)
        let city = viewModel.cities[indexPath.row]
        cell.textLabel?.text = "\(city.chineseName) \(city.name)"
        cell.accessoryType = viewModel.isCitySelected(at: indexPath.row) ? .checkmark : .none
        return cell
    }
}

extension CitySelectionViewController: UITableViewDelegate {

    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        tableView.deselectRow(at: indexPath, animated: true)
        viewModel.selectCity(at: indexPath.row)
        onCitySelected?(viewModel.selectedCity)
        navigationController?.popViewController(animated: true)
    }
}
