using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventorySystem.DAL
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            _connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=InventoryDB;Integrated Security=True;";
        }

        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public void EnsureDatabaseCreated()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string checkTableQuery = @"
                        SELECT COUNT(*) 
                        FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_NAME = 'Products'
                    ";

                    using (var command = new SqlCommand(checkTableQuery, connection))
                    {
                        int tableCount = (int)command.ExecuteScalar();

                        if (tableCount == 0)
                        {
                            MessageBox.Show("Creating tables...", "Setup",
                                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                            CreateTables();
                        }
                    }
                }
            }
            catch (SqlException)
            {
                MessageBox.Show("Creating database from scratch...", "Setup",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                CreateDatabase();
            }
        }

        private void CreateDatabase()
        {
            string masterConnection = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;";

            string createDbQuery = "CREATE DATABASE InventoryDB";

            using (var connection = new SqlConnection(masterConnection))
            using (var command = new SqlCommand(createDbQuery, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }

            CreateTables();
        }

        private void CreateTables()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string createTablesQuery = @"
                    CREATE TABLE Products (
                        ProductID INT PRIMARY KEY IDENTITY(1,1),
                        Name NVARCHAR(100) NOT NULL,
                        Description NVARCHAR(500),
                        Price DECIMAL(18,2) NOT NULL,
                        StockQuantity INT NOT NULL DEFAULT 0,
                        CreatedDate DATETIME DEFAULT GETDATE()
                    );

                    CREATE TABLE Customers (
                        CustomerID INT PRIMARY KEY IDENTITY(1,1),
                        Name NVARCHAR(100) NOT NULL,
                        Email NVARCHAR(100),
                        Phone NVARCHAR(20),
                        CreatedDate DATETIME DEFAULT GETDATE()
                    );

                    CREATE TABLE Orders (
                        OrderID INT PRIMARY KEY IDENTITY(1,1),
                        CustomerID INT FOREIGN KEY REFERENCES Customers(CustomerID),
                        OrderDate DATETIME DEFAULT GETDATE(),
                        TotalAmount DECIMAL(18,2) NOT NULL,
                        Status NVARCHAR(20) DEFAULT 'Pending'
                    );

                    CREATE TABLE OrderDetails (
                        OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
                        OrderID INT FOREIGN KEY REFERENCES Orders(OrderID),
                        ProductID INT FOREIGN KEY REFERENCES Products(ProductID),
                        Quantity INT NOT NULL,
                        UnitPrice DECIMAL(18,2) NOT NULL,
                        Subtotal AS (Quantity * UnitPrice) PERSISTED
                    );
                ";

                using (var command = new SqlCommand(createTablesQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}