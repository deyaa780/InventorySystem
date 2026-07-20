using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventorySystem.UI
{
    public partial class CustomerManagementForm : Form
    {
        public CustomerManagementForm()
        {
            InitializeComponent();
            LoadCustomers();

            // === WIRE UP ALL EVENTS ===
            // DataGridView
            this.dgvCustomers.SelectionChanged += new System.EventHandler(this.dgvCustomers_SelectionChanged);

            // Buttons
            this.btnAddCustomer.Click += new System.EventHandler(this.btnAddCustomer_Click);
            this.btnEditCustomer.Click += new System.EventHandler(this.btnEditCustomer_Click);
            this.btnDeleteCustomer.Click += new System.EventHandler(this.btnDeleteCustomer_Click);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            this.btnSearchCustomer.Click += new System.EventHandler(this.btnSearchCustomer_Click);

            // TextBox
            this.txtSearchCustomer.Enter += new System.EventHandler(this.txtSearchCustomer_Enter);
            this.txtSearchCustomer.Leave += new System.EventHandler(this.txtSearchCustomer_Leave);
        } 
        private void LoadCustomers()
        {
            try
            {
                var db = new DAL.DatabaseHelper();
                string query = "SELECT CustomerID, Name, Email, Phone, CreatedDate FROM Customers";
                DataTable customers = db.ExecuteQuery(query);

                dgvCustomers.DataSource = customers;
                FormatGridView();
                lblCustomerStatus.Text = $"👥 {customers.Rows.Count} customers loaded";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGridView()
        {
            // Turn OFF auto-resize BEFORE doing anything
            dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            if (dgvCustomers.Columns.Count == 0) return;

            // Set column headers
            dgvCustomers.Columns["CustomerID"].HeaderText = "ID";
            dgvCustomers.Columns["Name"].HeaderText = "Full Name";
            dgvCustomers.Columns["Email"].HeaderText = "Email Address";
            dgvCustomers.Columns["Phone"].HeaderText = "Phone Number";
            dgvCustomers.Columns["CreatedDate"].HeaderText = "Date Added";

            // Format date
            dgvCustomers.Columns["CreatedDate"].DefaultCellStyle.Format = "MM/dd/yyyy";

            // Set column widths (NO AutoResizeColumns!)
            dgvCustomers.Columns["CustomerID"].Width = 50;
            dgvCustomers.Columns["Name"].Width = 280;
            dgvCustomers.Columns["Email"].Width = 280;
            dgvCustomers.Columns["Phone"].Width = 120;
            dgvCustomers.Columns["CreatedDate"].Width = 180;
        } 
        private void dgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvCustomers.SelectedRows.Count > 0;
            btnEditCustomer.Enabled = hasSelection;
            btnDeleteCustomer.Enabled = hasSelection;
        }

        private void btnAddCustomer_Click(object sender, EventArgs e)
        {
            CustomerForm customerForm = new CustomerForm();
            customerForm.ShowDialog();

            if (customerForm.DialogResult == DialogResult.OK)
            {
                LoadCustomers();
            }
        }

        private void btnEditCustomer_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0) return;

            DataGridViewRow row = dgvCustomers.SelectedRows[0];
            int customerId = Convert.ToInt32(row.Cells["CustomerID"].Value);
            string name = row.Cells["Name"].Value.ToString();
            string email = row.Cells["Email"].Value.ToString();
            string phone = row.Cells["Phone"].Value.ToString();

            CustomerForm customerForm = new CustomerForm(customerId, name, email, phone);
            customerForm.ShowDialog();

            if (customerForm.DialogResult == DialogResult.OK)
            {
                LoadCustomers();
            }
        }

        private void btnDeleteCustomer_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0) return;

            DataGridViewRow row = dgvCustomers.SelectedRows[0];
            int customerId = Convert.ToInt32(row.Cells["CustomerID"].Value);
            string customerName = row.Cells["Name"].Value.ToString();

            DialogResult confirm = MessageBox.Show(
                $"Are you sure you want to delete '{customerName}'?\n\nThis will also delete all orders linked to this customer.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes) return;

            try
            {
                var db = new DAL.DatabaseHelper();
                string query = "DELETE FROM Customers WHERE CustomerID = @CustomerID";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@CustomerID", customerId)
                };

                int rowsAffected = db.ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    MessageBox.Show($"✅ Deleted customer: {customerName}", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadCustomers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting customer: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearchCustomer_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearchCustomer.Text.Trim();

            if (string.IsNullOrEmpty(searchTerm) || searchTerm == "Type to search...")
            {
                LoadCustomers();
                return;
            }

            try
            {
                var db = new DAL.DatabaseHelper();
                string query = @"
                    SELECT CustomerID, Name, Email, Phone, CreatedDate 
                    FROM Customers 
                    WHERE Name LIKE @Search OR Email LIKE @Search OR Phone LIKE @Search
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Search", $"%{searchTerm}%")
                };

                DataTable results = db.ExecuteQuery(query, parameters);
                dgvCustomers.DataSource = results;
                FormatGridView();
                lblCustomerStatus.Text = $"🔍 Found {results.Rows.Count} results for '{searchTerm}'";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching customers: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtSearchCustomer_Enter(object sender, EventArgs e)
        {
            if (txtSearchCustomer.Text == "Type to search...")
            {
                txtSearchCustomer.Text = "";
                txtSearchCustomer.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void txtSearchCustomer_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchCustomer.Text))
            {
                txtSearchCustomer.Text = "Type to search...";
                txtSearchCustomer.ForeColor = System.Drawing.Color.Gray;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadCustomers();
            txtSearchCustomer.Text = "Type to search...";
            txtSearchCustomer.ForeColor = System.Drawing.Color.Gray;
        }
    }
}