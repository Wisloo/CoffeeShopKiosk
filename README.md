# Coffee Shop Kiosk

Welcome to the Coffee Shop Kiosk application! This project is designed to provide a user-friendly interface for browsing and purchasing coffee products and pastries.

## Project Structure

The project consists of the following files and directories:

- **CoffeeShopKiosk.sln**: The solution file that organizes the project and its dependencies.
- **App.xaml**: Defines application-level resources and styles for the WPF application.
- **App.xaml.cs**: Contains the application class that initializes the application and handles startup events.
- **MainWindow.xaml**: Defines the main user interface layout, including visual elements for displaying coffee products and pastries.
- **MainWindow.xaml.cs**: Code-behind for the MainWindow, handling events and interactions defined in the XAML.
- **Models**:
  - **ProductModel.cs**: Represents a coffee product or pastry with properties such as ProductId, Name, Description, Price, and ImageUrl.
  - **OrderModel.cs**: Represents a customer's order, including properties like OrderId, List of Products, TotalAmount, and OrderDate.
- **ViewModels**:
  - **MainViewModel.cs**: Serves as the data context for the MainWindow, containing properties and commands for managing the product list and handling user interactions.
- **Views**:
  - **ProductListView.xaml**: Layout for displaying the list of products, allowing users to scroll through and select items for purchase.
- **Services**:
  - **ProductService.cs**: Handles business logic for retrieving and managing product data, including methods for fetching products from a database or an API.
- **Tests**:
  - **ProductServiceTests.cs**: Contains unit tests for the ProductService class to ensure methods for managing products work as expected.

## Features

- Browse a variety of coffee products and pastries.
- User-friendly interface for selecting and purchasing items.
- Dynamic product listing with scrolling capabilities.

## Setup Instructions

1. Clone the repository to your local machine.
2. Open the solution file `CoffeeShopKiosk.sln` in your preferred IDE.
3. Restore the NuGet packages required for the project.
4. Build the solution to ensure all dependencies are resolved.
5. Run the application to start browsing products.

## Usage Guidelines

- Use the main interface to scroll through available products.
- Click on a product to view more details and add it to your order.
- Proceed to checkout to complete your purchase.

Thank you for using the Coffee Shop Kiosk application! Enjoy your coffee and pastries!