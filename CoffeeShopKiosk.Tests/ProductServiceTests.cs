using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoffeeShopKiosk.Services;
using CoffeeShopKiosk.Models;

namespace CoffeeShopKiosk.Tests
{
    [TestClass]
    public class ProductServiceTests
    {
        private ProductService _productService;

        [TestInitialize]
        public void Setup()
        {
            _productService = new ProductService();
        }

        [TestMethod]
        public void GetAllProducts_ShouldReturnAllProducts()
        {
            // Arrange
            var expectedProductCount = 5; // Adjust based on your initial data

            // Act
            var products = _productService.GetAllProducts();

            // Assert
            Assert.AreEqual(expectedProductCount, products.Count());
        }

        [TestMethod]
        public void GetProductById_ValidId_ShouldReturnCorrectProduct()
        {
            // Arrange
            var productId = 1; // Adjust based on your initial data
            var expectedProductName = "Coffee"; // Adjust based on your initial data

            // Act
            var product = _productService.GetProductById(productId);

            // Assert
            Assert.IsNotNull(product);
            Assert.AreEqual(expectedProductName, product.Name);
        }

        [TestMethod]
        public void GetProductById_InvalidId_ShouldReturnNull()
        {
            // Arrange
            var invalidProductId = 999; // Assuming this ID does not exist

            // Act
            var product = _productService.GetProductById(invalidProductId);

            // Assert
            Assert.IsNull(product);
        }
    }
}