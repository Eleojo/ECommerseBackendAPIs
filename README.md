# 🛒 E-Commerce Backend System

Welcome to the **E-Commerce Backend System**! This project is a fully functional e-commerce backend built with **ASP.NET Core**. It includes features like product management, order handling, shopping carts, and user authentication with **JWT**. It also integrates **Redis caching** for improved performance.

## Features 🚀

- **JWT Authentication** 🔐  
  Secure access control using JWT, with role-based access for Admin, Seller, and Customer.

- **Product Management** 📦  
  Add, update, and retrieve products, with caching for faster performance.

- **Order Management** 🛍️  
  Manage user orders, including order creation and tracking.

- **Shopping Cart** 🛒  
  Full shopping cart functionality to add, remove, and list items.

- **Redis Caching** 🗄️  
  Faster product retrieval using Redis caching for efficient data storage.

- **Soft Deletion** 🗑️  
  Keep essential data like order history while logically deleting users.

## Technologies Used 🛠️

- **ASP.NET Core** - Backend framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Database
- **Redis** - Caching layer
- **JWT (JSON Web Token)** - Authentication
- **Swagger** - API documentation
- **xUnit** - Unit testing

## Setup and Installation 🛠️

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [Redis](https://redis.io/download)
- [Postman](https://www.postman.com/downloads/) (optional, for API testing)

### Getting Started

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/your-username/ecommerce-backend.git
   cd ecommerce-backend
   ```

2. **Set up the Database:**
   - Update the connection string in `appsettings.json` with your SQL Server details.
   - Run the following command to apply migrations and create the database:
     ```bash
     dotnet ef database update
     ```

3. **Set up Redis:**
   - Ensure Redis is running locally or configure a Redis server.
   - You can install Redis locally using Docker:
     ```bash
     docker run -d --name redis -p 6379:6379 redis
     ```

4. **Set JWT Secret Key:**
   - Create a `.env` file and set the `JWT_SECRET_KEY` value:
     ```
     JWT_SECRET_KEY=YourSecretKeyHere
     ```

5. **Run the Application:**
   ```bash
   dotnet run
   ```

6. **Swagger API Documentation:**
   - Access the API documentation at `https://localhost:5001/swagger` to explore all endpoints.

## Usage 📝

- **Authentication:**  
  To access secured endpoints, first generate a JWT by logging in with valid credentials (Admin/Seller/Customer).
  
- **Product Management:**  
  You can add, update, and retrieve products using the `/api/products` endpoints.

- **Shopping Cart:**  
  Add products to the shopping cart via `/api/cart`, and manage items efficiently.

- **Order Management:**  
  Handle orders through `/api/orders` for order placement and tracking.

## Testing ⚙️

This project includes unit tests using **xUnit**. To run the tests, use the following command:

```bash
dotnet test
```

## Roadmap 🛤️

- Frontend implementation using **Angular** 🅰️
- Deployment on **Azure** ☁️
- Payment gateway integration

## License 📄

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Connect With Me 🤝

Feel free to reach out if you have any questions or suggestions!

- LinkedIn: [My LinkedIn](https://linkedin.com/in/eleojoadegbe)


---

Feel free to adjust any sections or add more details specific to your project!
