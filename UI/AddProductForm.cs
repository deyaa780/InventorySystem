using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventorySystem.UI
{
    public partial class AddProductForm : Form
    {
        public AddProductForm()
        {
            InitializeComponent();

            // Set default values
            txtPrice.Text = "0.00";
            txtStock.Text = "0";
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // ===== 1. VALIDATE INPUT =====
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Product name is required.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid price (0 or greater).", "Validation Error",
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

            // ===== 2. CONFIRM ADD =====
            DialogResult confirm = MessageBox.Show(
                $"Add '{txtName.Text}'?\n\nPrice: {price:C2}\nStock: {stock} units",
                "Confirm Add",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes)
                return;

            // ===== 3. INSERT INTO DATABASE =====
            try
            {
                var db = new DAL.DatabaseHelper();

                string query = @"
                    INSERT INTO Products (Name, Description, Price, StockQuantity) 
                    VALUES (@Name, @Description, @Price, @Stock)
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Name", txtName.Text.Trim()),
                    new SqlParameter("@Description", txtDescription.Text.Trim()),
                    new SqlParameter("@Price", price),
                    new SqlParameter("@Stock", stock)
                };

                int rowsAffected = db.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show($"✅ '{txtName.Text}' added successfully!", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Failed to add product. Please try again.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void AddProductForm_Load(object sender, EventArgs e)
        {
            // Enter key = Add, Escape key = Cancel
            this.AcceptButton = btnAdd;
            this.CancelButton = btnCancel;
        } 
    }
}
