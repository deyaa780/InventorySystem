# 📦 Inventory Management System

A Windows desktop application for managing product inventory, customers, and orders for small businesses — built in C# with a classic 3-tier architecture (UI / BLL / DAL) on top of SQL Server LocalDB.

📖 **Full technical documentation:** [InventorySystem_TechnicalDocumentation.docx](./InventorySystem_TechnicalDocumentation.docx) — architecture diagram, 
database schema, application flow, and key code walkthroughs live there. This README is just the tour; go there for the details.

## Screens

**Main Dashboard** — the home screen. Load, add, delete, and search products, and jump into customer management or order creation.

![Main Dashboard](screenshots/main-dashboard.png)

**Add Product**

![Add Product](screenshots/add-product.png)

**Edit Product**

![Edit Product](screenshots/edit-product.png)

**Customer Management**

![Customer Management](screenshots/customer-management.png)

**Add / Edit Customer**

![Customer Form](screenshots/customer-form.png)

**Create Order** — pick a customer, add products via the selection window, and save with a running total.

![Order Form](screenshots/order-form.png)

**Product Selection** (used when building an order)

![Product Selection](screenshots/product-selection.png)

## Tech Stack
| **Language** | C# (.NET Framework 4.8) |
| **UI Framework** | Windows Forms |
| **Database** | SQL Server LocalDB |
| **Architecture** | 3-Tier (UI / BLL / DAL) |

## Getting Started

1. Clone the repo:
   ```
   git clone https://github.com/deyaa780/InventorySystem.git
   ```
2. Open `InventorySystem.sln` in Visual Studio.
3. Make sure **SQL Server LocalDB** is installed (it ships with Visual Studio's default workload).
4. Press **Start** (F5). The database and tables are created automatically on first run.

For everything else — the architecture diagram, the full database schema (Products, Customers, Orders, OrderDetails), the application flow, 
and explanations of the key code (connection handling, parameterized queries, delete flow, etc.) — see the [Technical Documentation](./InventorySystem_TechnicalDocumentation.docx).

## Roadmap

Products, customers, and orders are fully working end to end. Still planned: export to Excel/CSV, printable receipts, and low-stock alerts. See the doc's Future Enhancements section for details.
