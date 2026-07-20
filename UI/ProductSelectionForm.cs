using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace InventorySystem.UI
{
    public partial class ProductSelectionForm : Form
    {
        public DataRow SelectedProduct { get; private set; }

        public ProductSelectionForm()
        {
            InitializeComponent();
            
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.dgvProducts.SelectionChanged += new System.EventHandler(this.dgvProducts_SelectionChanged);
            this.dgvProducts.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgvProducts_DataBindingComplete);

            LoadProducts();
        }

        private void LoadProducts(string searchTerm = null)
        {
            try
            {
                var db = new DAL.DatabaseHelper();
                string query = "SELECT ProductID, Name, Price, StockQuantity FROM Products";

                DataTable products;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE Name LIKE @Search OR Description LIKE @Search";
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Search", $"%{searchTerm}%")
                    };
                    products = db.ExecuteQuery(query, parameters);
                }
                else
                {
                    products = db.ExecuteQuery(query);
                }

                dgvProducts.DataSource = products;
                // FormatGridView will be called by DataBindingComplete event
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatGridView()
        {
            if (dgvProducts == null || dgvProducts.Columns.Count == 0) return;

            dgvProducts.Columns["ProductID"].HeaderText = "ID";
            dgvProducts.Columns["Name"].HeaderText = "Product Name";
            dgvProducts.Columns["Price"].HeaderText = "Price";
            dgvProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
            dgvProducts.Columns["StockQuantity"].HeaderText = "Stock";
             // if you have a error showing in this just add the new in the next order its the key the timing 
            dgvProducts.Columns["ProductID"].Width = 50;
            dgvProducts.Columns["Name"].Width = 250;
            dgvProducts.Columns["Price"].Width = 100;
            dgvProducts.Columns["StockQuantity"].Width = 80;
        }

        // ✅ NEW: This fires AFTER data is bound
        private void dgvProducts_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            FormatGridView(); 
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm) || searchTerm == "Type to search...")
            {
                LoadProducts();
            }
            else
            {
                LoadProducts(searchTerm);
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;

            DataGridViewRow row = dgvProducts.SelectedRows[0];
            DataTable dt = (DataTable)dgvProducts.DataSource;
            SelectedProduct = dt.Rows[row.Index];

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            btnSelect.Enabled = dgvProducts.SelectedRows.Count > 0;
        }
    }
}