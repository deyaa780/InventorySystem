using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventorySystem.UI
{
    public partial class CustomerForm : Form
    {
        private int _customerId = 0;
        private bool _isEditMode = false;

        // Constructor for Add mode
        public CustomerForm()
        {
            InitializeComponent();

            // === WIRE UP EVENTS ===
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            this.Text = "➕ Add Customer";
        }

        // Constructor for Edit mode
        public CustomerForm(int customerId, string name, string email, string phone)
        {
            InitializeComponent();

            // === WIRE UP EVENTS ===
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;

            _customerId = customerId;
            _isEditMode = true;
            this.Text = "✏️ Edit Customer";

            txtName.Text = name;
            txtEmail.Text = email;
            txtPhone.Text = phone;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Customer name is required.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Email address is required.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            // Basic email validation
            try
            {
                var addr = new System.Net.Mail.MailAddress(txtEmail.Text.Trim());
                if (addr.Address != txtEmail.Text.Trim())
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEmail.Focus();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            // Confirm
            DialogResult confirm = MessageBox.Show(
                $"{(_isEditMode ? "Update" : "Add")} customer '{txtName.Text}'?",
                $"Confirm {(_isEditMode ? "Update" : "Add")}",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            // Save to database
            try
            {
                var db = new DAL.DatabaseHelper();
                string query;
                SqlParameter[] parameters;

                if (_isEditMode)
                {
                    query = @"
                        UPDATE Customers 
                        SET Name = @Name, Email = @Email, Phone = @Phone 
                        WHERE CustomerID = @CustomerID
                    ";

                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CustomerID", _customerId),
                        new SqlParameter("@Name", txtName.Text.Trim()),
                        new SqlParameter("@Email", txtEmail.Text.Trim()),
                        new SqlParameter("@Phone", txtPhone.Text.Trim())
                    };
                }
                else
                {
                    query = @"
                        INSERT INTO Customers (Name, Email, Phone) 
                        VALUES (@Name, @Email, @Phone)
                    ";

                    parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Name", txtName.Text.Trim()),
                        new SqlParameter("@Email", txtEmail.Text.Trim()),
                        new SqlParameter("@Phone", txtPhone.Text.Trim())
                    };
                }

                int rowsAffected = db.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show($"✅ Customer {(_isEditMode ? "updated" : "added")} successfully!",
                                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No changes were made.", "Warning",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving customer: {ex.Message}", "Error",
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