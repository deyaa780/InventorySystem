using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventorySystem.UI
{
    public partial class EditProductForm : Form
    {
        private int _productId;

        public EditProductForm(int productId, string name, string description, decimal price, int stock)
        {
            InitializeComponent();

            // Store the product ID
            _productId = productId;

            // Populate the form fields
            txtName.Text = name;
            txtDescription.Text = description;
            txtPrice.Text = price.ToString("F2");
            txtStock.Text = stock.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // ===== 1. VALIDATE INPUT =====
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Product name is required.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price (greater than 0).", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Please enter a valid stock quantity (0 or greater).", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStock.Focus();
                return;
            }

            // ===== 2. CONFIRM UPDATE =====
            DialogResult confirm = MessageBox.Show(
                $"Update '{txtName.Text}'?\n\n" +
                $"Price: {price:C2}\n" +
                $"Stock: {stock} units",
                "Confirm Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            // ===== 3. UPDATE DATABASE =====
            try
            {
                var db = new DAL.DatabaseHelper();

                string query = @"
                    UPDATE Products 
                    SET Name = @Name, 
                        Description = @Description,
                        Price = @Price, 
                        StockQuantity = @Stock 
                    WHERE ProductID = @ProductID
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@ProductID", _productId),
                    new SqlParameter("@Name", txtName.Text.Trim()),
                    new SqlParameter("@Description", txtDescription.Text.Trim()),
                    new SqlParameter("@Price", price),
                    new SqlParameter("@Stock", stock)
                };

                int rowsAffected = db.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show($"✅ Product updated successfully!", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No changes were made. The product may have been deleted.",
                                    "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating product: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}