# E-Commerce .NET Web API

Welcome to the E-Commerce Web API! This API provides backend functionality for an e-commerce platform. It includes endpoints for user authentication, product management, cart functionality, and order processing.

---

## Table of Contents
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Getting Started](#getting-started)
- [Environment Variables](#environment-variables)
- [API Endpoints](#api-endpoints)
  - [Authentication](#authentication)
  - [Products](#products)
  - [Categories](#categories)
  - [Cart](#cart)
  - [Orders](#orders)
  - [Admin](#admin)
- [Error Handling](#error-handling)
- [Contributing](#contributing)
- [License](#license)

---

## Features
- **User Authentication**: Secure login and registration using JWT.
- **Product Management**: Browse, search, and filter products.
- **Cart Functionality**: Add, update, and remove items in the cart.
- **Order Management**: Place orders and view their status.
- **Admin Features**: Manage products, categories, and orders.

---

## Technologies Used
- **Framework**: ASP.NET Core
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Authentication**: JSON Web Token (JWT)
- **API Documentation**: Swagger/OpenAPI
- **Logging**: Serilog

---

## Getting Started

### Prerequisites
1. .NET SDK 7.0+
2. SQL Server
3. Postman or any API testing tool (optional)

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/Jalalgorithm/E-CommerceAPI.git
   cd E-CommerceAPI
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Update the database:
   ```bash
   dotnet ef database update
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

---

## Environment Variables
Ensure you set the following environment variables in your `appsettings.json` or using a `.env` file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YourDatabaseConnectionString"
  },
  "JWT": {
    "Key": "YourJWTSecretKey",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience"
  }
}
```

---

## API Endpoints

### Authentication

#### **Register a New User**
- **POST** `/api/account/register`
- **Request Body**:
  ```json
  {
    "firstname": "john_doe",
    "lastname": "john_doe",
    "phonenumber": "1123459876",
    "email": "john.doe@example.com",
    "password": "StrongPassword123"
  }
  ```
- **Response**:
  ```json
  {
    "message": "Registration successful."
  }
  ```

#### **Log In**
- **POST** `/api/account/login`
- **Request Body**:
  ```json
  {
    "email": "john.doe@example.com",
    "password": "StrongPassword123"
  }
  ```
- **Response**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
  ```

---

### Products

#### **Get All Products**
- **GET** `/api/products`
- **Response**:
  ```json
  [
    {
      "id": 1,
      "name": "Product A",
      "description": "Description of Product A",
      "price": 49.99,
      "category": "Electronics",
      "stock": 20
    },
    {
      "id": 2,
      "name": "Product B",
      "description": "Description of Product B",
      "price": 29.99,
      "category": "Books",
      "stock": 50
    }
  ]
  ```

#### **Get a Product by ID**
- **GET** `/api/products/{id}`
- **Response**:
  ```json
  {
    "id": 1,
    "name": "Product A",
    "description": "Description of Product A",
    "price": 49.99,
    "category": "Electronics",
    "stock": 20
  }
  ```

#### **Create a New Product** *(Admin Only)*
- **POST** `/api/products`
- **Request Body**:
  ```json
  {
    "name": "Product C",
    "description": "Description of Product C",
    "price": 59.99,
    "categoryId": 3,
    "stock": 10
  }
  ```
- **Response**:
  ```json
  {
    "message": "Product created successfully.",
    "productId": 3
  }
  ```

#### **Update an Existing Product** *(Admin Only)*
- **PUT** `/api/products/{id}`
- **Request Body**:
  ```json
  {
    "name": "Updated Product A",
    "description": "Updated Description",
    "price": 54.99,
    "stock": 25
  }
  ```
- **Response**:
  ```json
  {
    "message": "Product updated successfully."
  }
  ```

#### **Delete a Product** *(Admin Only)*
- **DELETE** `/api/products/{id}`
- **Response**:
  ```json
  {
    "message": "Product deleted successfully."
  }
  ```

---

### Categories

#### **Get All Categories**
- **GET** `/api/categories`
- **Response**:
  ```json
  [
    { "id": 1, "name": "Electronics" },
    { "id": 2, "name": "Books" }
  ]
  ```

#### **Create a New Category** *(Admin Only)*
- **POST** `/api/categories`
- **Request Body**:
  ```json
  {
    "name": "New Category"
  }
  ```
- **Response**:
  ```json
  {
    "message": "Category created successfully."
  }
  ```

#### **Update a Category** *(Admin Only)*
- **PUT** `/api/categories/{id}`
- **Request Body**:
  ```json
  {
    "name": "Updated Category Name"
  }
  ```
- **Response**:
  ```json
  {
    "message": "Category updated successfully."
  }
  ```

#### **Delete a Category** *(Admin Only)*
- **DELETE** `/api/categories/{id}`
- **Response**:
  ```json
  {
    "message": "Category deleted successfully."
  }
  ```

---

### Cart

#### **Get Cart**
- **GET** `/api/cart`
- **Response**:
  ```json
  {
    "items": [
      {
        "productId": 1,
        "quantity": 2
      }
    ],
    "totalPrice": 99.98
  }
  ```

#### **Add Item to Cart**
- **POST** `/api/cart/add`
- **Request Body**:
  ```json
  {
    "productId": 1,
    "quantity": 1
  }
  ```
- **Response**:
  ```json
  {
    "message": "Item added to cart."
  }
  ```

#### **Remove Item from Cart**
- **DELETE** `/api/cart/remove/{productId}`
- **Response**:
  ```json
  {
    "message": "Item removed from cart."
  }
  ```

---

### Orders

#### **Get All Orders (User-Specific)**
- **GET** `/api/orders`
- **Response**:
  ```json
  [
    {
      "id": 1,
      "status": "Pending",
      "totalPrice": 199.98,
      "items": [
        {
          "productId": 1,
          "quantity": 2

