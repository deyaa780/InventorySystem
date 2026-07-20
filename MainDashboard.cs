using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventorySystem.UI; // import the UI namespace for AddProductForm and EditProductForm 

namespace InventorySystem
{
    public partial class MainDashboard : Form
    {
        public MainDashboard()
        {
            InitializeComponent();
        }

        private void MainDashboard_Load(object sender, EventArgs e)
        {
            var db = new DAL.DatabaseHelper();
            db.EnsureDatabaseCreated();

            lblStatus.Text = "✅ Database ready – loading products...";

            // Auto-load products when form opens
            btnLoadProducts_Click(sender, e); 
        }

        private void btnLoadProducts_Click(object sender, EventArgs e)
        {
            try
            {
                var db = new DAL.DatabaseHelper();
                string query = "SELECT ProductID, Name, Description, Price, StockQuantity, CreatedDate FROM Products";

                DataTable products = db.ExecuteQuery(query);
                dgvProducts.DataSource = products;

                // ✅ Force the DataGridView to finish creating columns
                dgvProducts.Refresh();
                Application.DoEvents();

                // ✅ Now format the columns
                FormatGridView();

                // Reset search box
                txtSearch.Text = "Type to search...";
                txtSearch.ForeColor = System.Drawing.Color.Gray;

                lblStatus.Text = $"📦 {products.Rows.Count} products loaded";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            } 
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            // this for a random sample 
            //    try
            //    { 
            //        var db = new DAL.DatabaseHelper();

            //        // Random product generator
            //        string[] productNames = { "Laptop Pro", "Wireless Mouse", "Mechanical Keyboard",
            //                           "USB-C Hub", "External SSD", "Monitor 27\"",
            //                           "Webcam HD", "Speaker System", "Gaming Headset",
            //                           "Tablet Stand" };

            //        string[] descriptions = { "High performance", "Ergonomic design", "RGB lighting",
            //                           "Plug and play", "Ultra fast", "Crystal clear",
            //                           "Noise cancelling", "Premium quality", "Best seller",
            //                           "Portable" };

            //        Random rand = new Random();
            //        int index = rand.Next(productNames.Length);
            //        decimal price = Math.Round((decimal)(50 + rand.NextDouble() * 950), 2);
            //        int stock = rand.Next(1, 50);

            //        string query = @"
            //    INSERT INTO Products (Name, Description, Price, StockQuantity) 
            //    VALUES (@Name, @Desc, @Price, @Stock)
            //";

            //        SqlParameter[] parameters = new SqlParameter[]
            //        {
            //    new SqlParameter("@Name", productNames[index]),
            //    new SqlParameter("@Desc", descriptions[rand.Next(descriptions.Length)]),
            //    new SqlParameter("@Price", price),
            //    new SqlParameter("@Stock", stock)
            //        };

            //        int rowsAffected = db.ExecuteNonQuery(query, parameters);

            //        // Refresh the DataGridView automatically
            //        btnLoadProducts_Click(sender, e); // Reuse the load method!

            //        MessageBox.Show($"✅ Added: {productNames[index]} (${price:F2})\nStock: {stock} units",
            //                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"Error adding product: {ex.Message}", "Error",
            //                        MessageBoxButtons.OK, MessageBoxIcon.Error);

            //    }  
            //-------------

            // Open the Add Product Form
            AddProductForm addForm = new AddProductForm();
            addForm.ShowDialog(); // Show as modal dialog

            // After form closes, refresh the grid
            if (addForm.DialogResult == DialogResult.OK)
            {
                btnLoadProducts_Click(sender, e);
            }
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            // Enable delete button only if a row is selected
            btnDeleteProduct.Enabled = dgvProducts.SelectedRows.Count > 0;
        }

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            // Check if a row is selected
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to delete.", "No Selection",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the ProductID from the selected row
            int productId = Convert.ToInt32(dgvProducts.SelectedRows[0].Cells["ProductID"].Value);
            string productName = dgvProducts.SelectedRows[0].Cells["Name"].Value.ToString();

            // Ask for confirmation
            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete '{productName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    var db = new DAL.DatabaseHelper();
                    string query = "DELETE FROM Products WHERE ProductID = @ProductID";

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                new SqlParameter("@ProductID", productId)
                    };

                    int rowsAffected = db.ExecuteNonQuery(query, parameters);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"✅ Deleted: {productName}", "Success",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh the grid
                        btnLoadProducts_Click(sender, e);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting product: {ex.Message}", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } 
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();

            if (string.IsNullOrEmpty(searchTerm) || searchTerm == "Type to search...")
            {
                MessageBox.Show("Please enter a search term.", "Empty Search",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var db = new DAL.DatabaseHelper();

                // Search by Name or Description
                string query = @"
            SELECT ProductID, Name, Description, Price, StockQuantity, CreatedDate 
            FROM Products 
            WHERE Name LIKE @Search OR Description LIKE @Search
        ";

                SqlParameter[] parameters = new SqlParameter[]
                {
            new SqlParameter("@Search", $"%{searchTerm}%") // % = wildcard
                };

                DataTable results = db.ExecuteQuery(query, parameters);
                dgvProducts.DataSource = results;

                // Re-format columns
                FormatGridView();

                lblStatus.Text = $"🔍 Found {results.Rows.Count} results for '{searchTerm}'";

                if (results.Rows.Count == 0)
                {
                    MessageBox.Show($"No products found matching '{searchTerm}'.", "No Results",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {

            if (txtSearch.Text == "Type to search...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = System.Drawing.Color.Black;
            }
        }
        private void FormatGridView()
        {
            // Wait for columns to be created
            if (dgvProducts.Columns.Count == 0)
            {
                // If no columns, exit quietly
                return;
            }

            // Check if each column exists before accessing it
            if (dgvProducts.Columns.Contains("ProductID"))
                dgvProducts.Columns["ProductID"].HeaderText = "ID";

            if (dgvProducts.Columns.Contains("Name"))
                dgvProducts.Columns["Name"].HeaderText = "Product Name";

            if (dgvProducts.Columns.Contains("Description"))
                dgvProducts.Columns["Description"].HeaderText = "Description";

            if (dgvProducts.Columns.Contains("Price"))
            {
                dgvProducts.Columns["Price"].HeaderText = "Price ($)";
                dgvProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
                dgvProducts.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dgvProducts.Columns.Contains("StockQuantity"))
                dgvProducts.Columns["StockQuantity"].HeaderText = "Stock";

            if (dgvProducts.Columns.Contains("CreatedDate"))
            {
                dgvProducts.Columns["CreatedDate"].HeaderText = "Date Added";
                dgvProducts.Columns["CreatedDate"].DefaultCellStyle.Format = "MM/dd/yyyy";
            }

            // Auto-size columns
            dgvProducts.AutoResizeColumns();

            // Set column widths (with safety checks)
            if (dgvProducts.Columns.Contains("ProductID"))
                dgvProducts.Columns["ProductID"].Width = 50;

            if (dgvProducts.Columns.Contains("Name"))
                dgvProducts.Columns["Name"].Width = 200;

            if (dgvProducts.Columns.Contains("Description"))
                dgvProducts.Columns["Description"].Width = 250;

            if (dgvProducts.Columns.Contains("Price"))
                dgvProducts.Columns["Price"].Width = 100;

            if (dgvProducts.Columns.Contains("StockQuantity"))
                dgvProducts.Columns["StockQuantity"].Width = 80;

            if (dgvProducts.Columns.Contains("CreatedDate"))
                dgvProducts.Columns["CreatedDate"].Width = 120; 
        } 

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Type to search...";
                txtSearch.ForeColor = System.Drawing.Color.Gray; 
            } 
        }

        private void dgvProducts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if user double-clicked a valid row (not header)
            if (e.RowIndex < 0) return;

            // Get the selected row
            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

            // Extract product data
            int productId = Convert.ToInt32(row.Cells["ProductID"].Value);
            string name = row.Cells["Name"].Value.ToString();
            string description = row.Cells["Description"].Value.ToString();
            decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
            int stock = Convert.ToInt32(row.Cells["StockQuantity"].Value);

            // Open Edit Form with this data
            EditProductForm editForm = new EditProductForm(productId, name, description, price, stock);
            editForm.ShowDialog(); // Show as modal dialog

            // After form closes, refresh the grid
            btnLoadProducts_Click(sender, e);
        }

        private void btnManageCustomers_Click(object sender, EventArgs e)
        {
            CustomerManagementForm customerForm = new CustomerManagementForm();
            customerForm.ShowDialog(); 
        }

        private void btnNewOrder_Click(object sender, EventArgs e)
        {
            OrderForm orderForm = new OrderForm();
            orderForm.ShowDialog();
        }

        private void btnViewOrders_Click(object sender, EventArgs e)
        {
            MessageBox.Show("📊 View Orders feature coming soon!", "Coming Soon",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    }
