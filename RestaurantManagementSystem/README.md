# GlowBite - Premium Restaurant Management System

GlowBite is a state-of-the-art, responsive Restaurant Management System built with **ASP.NET Core Web API (.NET 9.0)**, **Entity Framework Core**, and **SQLite**. It features a stunning glassmorphic Single Page Application (SPA) dashboard served directly from the backend API.

The system automates restaurant operations including table reservations, visual occupancy mapping, POS ordering, real-time inventory deductions/restorations, and detailed daily sales reporting.

---

## Technical Stack

* **Backend Framework**: ASP.NET Core Web API (targeted for `.NET 9.0`)
* **ORM & Database**: Entity Framework Core with SQLite (embedded self-contained database)
* **Frontend**: Vanilla HTML5, CSS3 (Glassmorphism design system, CSS variables, CSS transitions/animations), and JavaScript (Asynchronous ES6 fetch API integration)

---

## Core Features & Business Logic

### 1. POS Order Processing & Inventory Checks
* **Sufficient Stock Verification**: Before an order is created, the system maps the recipe requirements of the menu items. It verifies that the quantity of raw ingredients in the inventory is sufficient to fulfill the order. If insufficient, the order is rejected with a validation message.
* **Auto Stock Deductions**: On successful order placement, ingredient counts are deducted from the inventory.
* **Smart Auto-Restore on Cancel**: If an order status changes to `Cancelled`, the system automatically restores the exact amount of raw ingredients back to the inventory stock.
* **Table Occupancy**: Setting an order to `Preparing` or `Served` automatically updates the associated table status to `Occupied`.

### 2. Table Reservations
* **Capacity Constraint checks**: Prevents booking a table for a guest count exceeding the table's capacity limit.
* **Conflict Prevention**: Checks table availability and blocks overlapping reservations within a 2-hour window of the requested slot.
* **Seating Auto-updates**: When seating reservations, the associated table state transitions to `Occupied` or `Reserved` accordingly, returning to `Available` upon completion.

### 3. Reporting & Admin Analytics
* **Dashboard Stats**: Summarizes daily gross revenue, paid order volume, active tables, and active low-stock ingredient warnings.
* **Daily Sales Graph**: Serves hourly sales performance (revenue and order count) compiled dynamically.
* **Top Selling Dishes**: Lists popular items ranked by quantity sold and gross revenue.
* **Stock Levels**: Highlights low-stock ingredients whose quantities drop below defined reorder levels.

---

## Folder Structure

```
RestaurantManagementSystem/
├── Program.cs                   # API bootstrapping, DI, Static File Hosting, JSON settings
├── RestaurantManagementSystem.csproj
├── restaurant.db                # SQLite database file (created on first run)
├── Data/
│   ├── RestaurantDbContext.cs   # Entity relationships and EF configurations
│   └── DbSeeder.cs              # Database initialization & sample data seeder
├── Models/
│   ├── Table.cs                 # Table entity and occupancy statuses
│   ├── MenuItem.cs              # Dish listings
│   ├── InventoryItem.cs         # Raw stock ingredients
│   ├── MenuItemIngredient.cs    # Recipe relationships mapping menu to inventory
│   ├── Order.cs                 # Transaction orders
│   ├── OrderItem.cs             # Single item lines inside orders
│   └── Reservation.cs           # Guest bookings
├── Dtos/
│   ├── CreateOrderDto.cs        # POS post structures
│   └── ReportsDto.cs            # Analytics data structures
├── Services/
│   ├── IReservationService.cs   # Core booking rules
│   ├── ReservationService.cs
│   ├── IOrderService.cs         # Core stock deduction/checking rules
│   ├── OrderService.cs
│   ├── IReportingService.cs     # Calculations and stats
│   └── ReportingService.cs
├── Controllers/
│   ├── TablesController.cs      # API for managing dining tables
│   ├── ReservationsController.cs# API for reservations
│   ├── MenuController.cs        # API for recipe items
│   ├── OrdersController.cs      # API for processing orders
│   ├── InventoryController.cs   # API for managing ingredient levels
│   └── ReportsController.cs     # API serving sales metrics
└── wwwroot/                     # Client SPA static files
    ├── index.html               # Main dashboard UI
    ├── css/
    │   └── styles.css           # Premium glassmorphic stylesheet
    └── js/
        └── app.js               # Async API integration script
```

---

## Getting Started

### Prerequisites
* [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) installed on your system.

### Running the Application

1. Clone or copy the project files to your workspace.
2. Open your terminal and navigate to the project directory:
   ```bash
   cd RestaurantManagementSystem
   ```
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the API and static host:
   ```bash
   dotnet run --urls=http://localhost:5000
   ```
5. Open your web browser and navigate to:
   [http://localhost:5000](http://localhost:5000)

*Note: On startup, the database `restaurant.db` is automatically created, initialized, and populated with sample tables, ingredients, recipes, reservations, and sales transactions.*

---

## REST API Documentation

### 1. Tables Endpoint
* `GET /api/tables` - Get all tables and status.
* `POST /api/tables` - Create a new dining table.
* `PUT /api/tables/{id}/status` - Modify table status manually (`Available`, `Occupied`, `Reserved`).

### 2. Reservations Endpoint
* `GET /api/reservations` - Retrieve reservations list.
* `POST /api/reservations` - Create a reservation (performs capacity & conflict checks).
* `PUT /api/reservations/{id}/confirm` - Confirm a pending reservation.
* `PUT /api/reservations/{id}/complete` - Mark reservation completed (freeing the table).
* `PUT /api/reservations/{id}/cancel` - Cancel reservation.

### 3. Menu Endpoint
* `GET /api/menu` - Fetch all dishes with their recipe ingredients details.
* `POST /api/menu` - Add a new menu item listing with ingredient weights.
* `PUT /api/menu/{id}` - Update dish pricing, availability, and recipes.

### 4. Orders Endpoint
* `GET /api/orders` - Get all orders, table details, and lines.
* `POST /api/orders` - Process a new order (verifies stock and deducts ingredients).
* `PUT /api/orders/{id}/status` - Advance order status (`Pending` -> `Preparing` -> `Served` -> `Paid` or `Cancelled`).

### 5. Inventory Endpoint
* `GET /api/inventory` - Get raw ingredients stock levels.
* `POST /api/inventory` - Add a new ingredient.
* `PUT /api/inventory/{id}/restock` - Add a specific quantity to an ingredient's stock.

### 6. Reports Endpoint
* `GET /api/reports/dashboard` - Get dashboard statistics (revenue, popular items, low-stock warnings).
* `GET /api/reports/daily` - Get hourly sales performance for today.
