using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Data
{
    public static class DbSeeder
    {
        public static void Seed(RestaurantDbContext context)
        {
            // Seed Tables
            if (!context.Tables.Any())
            {
                context.Tables.AddRange(new List<Table>
                {
                    new() { TableNumber = 1, Capacity = 2, Status = TableStatus.Available },
                    new() { TableNumber = 2, Capacity = 2, Status = TableStatus.Available },
                    new() { TableNumber = 3, Capacity = 4, Status = TableStatus.Available },
                    new() { TableNumber = 4, Capacity = 4, Status = TableStatus.Available },
                    new() { TableNumber = 5, Capacity = 6, Status = TableStatus.Available },
                    new() { TableNumber = 6, Capacity = 8, Status = TableStatus.Available }
                });
                context.SaveChanges();
            }

            // Seed Inventory
            if (!context.InventoryItems.Any())
            {
                context.InventoryItems.AddRange(new List<InventoryItem>
                {
                    new() { Name = "Beef Patty", QuantityInStock = 50, Unit = "pcs", ReorderLevel = 10 },
                    new() { Name = "Burger Buns", QuantityInStock = 50, Unit = "pcs", ReorderLevel = 10 },
                    new() { Name = "Cheddar Cheese", QuantityInStock = 60, Unit = "slices", ReorderLevel = 15 },
                    new() { Name = "Potatoes", QuantityInStock = 10000, Unit = "g", ReorderLevel = 2000 },
                    new() { Name = "Pasta", QuantityInStock = 5000, Unit = "g", ReorderLevel = 1000 },
                    new() { Name = "Tomato Sauce", QuantityInStock = 6000, Unit = "ml", ReorderLevel = 1000 },
                    new() { Name = "Chicken Breast", QuantityInStock = 8000, Unit = "g", ReorderLevel = 1000 },
                    new() { Name = "Coffee Beans", QuantityInStock = 2000, Unit = "g", ReorderLevel = 500 },
                    new() { Name = "Milk", QuantityInStock = 5000, Unit = "ml", ReorderLevel = 1000 }
                });
                context.SaveChanges();
            }

            // Seed Menu Items and their recipes
            if (!context.MenuItems.Any())
            {
                var beef = context.InventoryItems.First(i => i.Name == "Beef Patty");
                var bun = context.InventoryItems.First(i => i.Name == "Burger Buns");
                var cheese = context.InventoryItems.First(i => i.Name == "Cheddar Cheese");
                var potato = context.InventoryItems.First(i => i.Name == "Potatoes");
                var pasta = context.InventoryItems.First(i => i.Name == "Pasta");
                var chicken = context.InventoryItems.First(i => i.Name == "Chicken Breast");
                var milk = context.InventoryItems.First(i => i.Name == "Milk");
                var coffee = context.InventoryItems.First(i => i.Name == "Coffee Beans");

                var burgerItem = new MenuItem
                {
                    Name = "Classic Cheeseburger",
                    Description = "Juicy beef patty with melted cheddar, toasted buns, and signature house sauce.",
                    Price = 12.99m,
                    Category = "Main",
                    IsAvailable = true
                };
                burgerItem.Ingredients.Add(new MenuItemIngredient { InventoryItemId = beef.Id, QuantityRequired = 1 });
                burgerItem.Ingredients.Add(new MenuItemIngredient { InventoryItemId = bun.Id, QuantityRequired = 1 });
                burgerItem.Ingredients.Add(new MenuItemIngredient { InventoryItemId = cheese.Id, QuantityRequired = 1 });

                var friesItem = new MenuItem
                {
                    Name = "French Fries",
                    Description = "Crispy golden sea-salted hand-cut potatoes.",
                    Price = 4.99m,
                    Category = "Starter",
                    IsAvailable = true
                };
                friesItem.Ingredients.Add(new MenuItemIngredient { InventoryItemId = potato.Id, QuantityRequired = 200 });

                var pastaItem = new MenuItem
                {
                    Name = "Chicken Alfredo Pasta",
                    Description = "Fettuccine tossed in rich creamy white Alfredo sauce with grilled chicken breast.",
                    Price = 15.99m,
                    Category = "Main",
                    IsAvailable = true
                };
                pastaItem.Ingredients.Add(new MenuItemIngredient { InventoryItemId = pasta.Id, QuantityRequired = 150 });
                pastaItem.Ingredients.Add(new MenuItemIngredient { InventoryItemId = chicken.Id, QuantityRequired = 150 });
                pastaItem.Ingredients.Add(new MenuItemIngredient { InventoryItemId = milk.Id, QuantityRequired = 100 });

                var cappuccinoItem = new MenuItem
                {
                    Name = "Cappuccino",
                    Description = "Classic espresso drink prepared with double shot espresso and steamed milk foam.",
                    Price = 3.99m,
                    Category = "Beverage",
                    IsAvailable = true
                };
                cappuccinoItem.Ingredients.Add(new MenuItemIngredient { InventoryItemId = coffee.Id, QuantityRequired = 15 });
                cappuccinoItem.Ingredients.Add(new MenuItemIngredient { InventoryItemId = milk.Id, QuantityRequired = 150 });

                context.MenuItems.AddRange(new List<MenuItem> { burgerItem, friesItem, pastaItem, cappuccinoItem });
                context.SaveChanges();
            }

            // Seed some default orders & reservations if database is empty
            if (!context.Orders.Any())
            {
                var burger = context.MenuItems.First(m => m.Name == "Classic Cheeseburger");
                var fries = context.MenuItems.First(m => m.Name == "French Fries");
                var cappuccino = context.MenuItems.First(m => m.Name == "Cappuccino");

                var table1 = context.Tables.First(t => t.TableNumber == 1);
                var table3 = context.Tables.First(t => t.TableNumber == 3);

                // Add paid orders
                var order1 = new Order
                {
                    TableId = table3.Id,
                    OrderTime = DateTime.UtcNow.AddHours(-3),
                    Status = OrderStatus.Paid,
                    TotalAmount = burger.Price + fries.Price
                };
                order1.OrderItems.Add(new OrderItem { MenuItemId = burger.Id, Quantity = 1, UnitPrice = burger.Price });
                order1.OrderItems.Add(new OrderItem { MenuItemId = fries.Id, Quantity = 1, UnitPrice = fries.Price });

                var order2 = new Order
                {
                    TableId = table1.Id,
                    OrderTime = DateTime.UtcNow.AddHours(-1),
                    Status = OrderStatus.Paid,
                    TotalAmount = cappuccino.Price * 2
                };
                order2.OrderItems.Add(new OrderItem { MenuItemId = cappuccino.Id, Quantity = 2, UnitPrice = cappuccino.Price });

                context.Orders.AddRange(order1, order2);
                context.SaveChanges();
            }

            if (!context.Reservations.Any())
            {
                var table4 = context.Tables.First(t => t.TableNumber == 4);
                context.Reservations.Add(new Reservation
                {
                    TableId = table4.Id,
                    CustomerName = "Alice Smith",
                    CustomerPhone = "555-0199",
                    ReservationTime = DateTime.UtcNow.AddHours(2),
                    NumberOfGuests = 3,
                    Status = ReservationStatus.Confirmed
                });
                table4.Status = TableStatus.Reserved;
                context.SaveChanges();
            }
        }
    }
}
