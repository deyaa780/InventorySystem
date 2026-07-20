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

namespace InventorySystem.UI
{
    public partial class OrderForm : Form
    {
        // ADD THIS: DataTable to hold order items
        private DataTable _orderItemsTable;
        private decimal _totalAmount = 0;

        public OrderForm()
        {
            InitializeComponent();
            // ===== FORCE WIRE ALL EVENTS =====
            this.btnAddProduct.Click += new System.EventHandler(this.btnAddProduct_Click);
            this.btnRemoveProduct.Click += new System.EventHandler(this.btnRemoveProduct_Click);
            this.btnSaveOrder.Click += new System.EventHandler(this.btnSaveOrder_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.dgvOrderItems.SelectionChanged += new System.EventHandler(this.dgvOrderItems_SelectionChanged);

            
            LoadCustomers();
            InitializeOrderItemsGrid();
            UpdateTotal();
        }
        private void LoadCustomers()
        {
            try
            {
                var db = new DAL.DatabaseHelper();
                string query = "SELECT CustomerID, Name FROM Customers";
                DataTable customers = db.ExecuteQuery(query);

                cmbCustomers.DataSource = customers;
                cmbCustomers.DisplayMember = "Name";
                cmbCustomers.ValueMember = "CustomerID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InitializeOrderItemsGrid()
        {
            _orderItemsTable = new DataTable();
            _orderItemsTable.Columns.Add("ProductID", typeof(int));
            _orderItemsTable.Columns.Add("ProductName", typeof(string));
            _orderItemsTable.Columns.Add("Price", typeof(decimal));
            _orderItemsTable.Columns.Add("Quantity", typeof(int));
            _orderItemsTable.Columns.Add("Subtotal", typeof(decimal));

            dgvOrderItems.DataSource = _orderItemsTable;

            // Format columns
            dgvOrderItems.Columns["ProductID"].Visible = false;
            dgvOrderItems.Columns["ProductName"].HeaderText = "Product Name";
            dgvOrderItems.Columns["Price"].HeaderText = "Price";
            dgvOrderItems.Columns["Price"].DefaultCellStyle.Format = "C2";
            dgvOrderItems.Columns["Quantity"].HeaderText = "Qty";
            dgvOrderItems.Columns["Subtotal"].HeaderText = "Subtotal";
            dgvOrderItems.Columns["Subtotal"].DefaultCellStyle.Format = "C2";

            dgvOrderItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        private void UpdateTotal()
        {
            _totalAmount = 0;
            foreach (DataRow row in _orderItemsTable.Rows)
            {
                _totalAmount += Convert.ToDecimal(row["Subtotal"]);
            }
            txtTotal.Text = _totalAmount.ToString("C2");
        } 



        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            ProductSelectionForm productForm = new ProductSelectionForm();
            if (productForm.ShowDialog() == DialogResult.OK)
            {
                DataRow product = productForm.SelectedProduct;
                int productId = Convert.ToInt32(product["ProductID"]);
                string productName = product["Name"].ToString();
                decimal price = Convert.ToDecimal(product["Price"]);

                DataRow[] existing = _orderItemsTable.Select($"ProductID = {productId}");
                if (existing.Length > 0)
                {
                    int currentQty = Convert.ToInt32(existing[0]["Quantity"]);
                    existing[0]["Quantity"] = currentQty + 1;
                    existing[0]["Subtotal"] = price * (currentQty + 1);
                }
                else
                {
                    _orderItemsTable.Rows.Add(productId, productName, price, 1, price);
                }

                UpdateTotal();
            } 
        }

        private void btnRemoveProduct_Click(object sender, EventArgs e)
        {
            if (dgvOrderItems.SelectedRows.Count == 0) return;

            DataGridViewRow row = dgvOrderItems.SelectedRows[0];
            string productName = row.Cells["ProductName"].Value.ToString();

            DialogResult confirm = MessageBox.Show($"Remove '{productName}' from order?",
                "Confirm Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                _orderItemsTable.Rows.RemoveAt(row.Index);
                UpdateTotal();
            } 
        }

        private void btnSaveOrder_Click(object sender, EventArgs e)
        {
            if (cmbCustomers.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a customer.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_orderItemsTable.Rows.Count == 0)
            {
                MessageBox.Show("Please add at least one product to the order.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Save order for {cmbCustomers.Text}?\n\nTotal: {txtTotal.Text}\nItems: {_orderItemsTable.Rows.Count}",
                "Confirm Order",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            try
            {
                int customerId = Convert.ToInt32(cmbCustomers.SelectedValue);
                var db = new DAL.DatabaseHelper();

                string orderQuery = @"
                    INSERT INTO Orders (CustomerID, TotalAmount, Status) 
                    VALUES (@CustomerID, @TotalAmount, 'Pending');
                    SELECT SCOPE_IDENTITY();
                ";

                SqlParameter[] orderParams = new SqlParameter[]
                {
                    new SqlParameter("@CustomerID", customerId),
                    new SqlParameter("@TotalAmount", _totalAmount)
                };

                DataTable result = db.ExecuteQuery(orderQuery, orderParams);
                int orderId = Convert.ToInt32(result.Rows[0][0]);

                foreach (DataRow item in _orderItemsTable.Rows)
                {
                    string detailQuery = @"
                        INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice) 
                        VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice)
                    ";

                    SqlParameter[] detailParams = new SqlParameter[]
                    {
                        new SqlParameter("@OrderID", orderId),
                        new SqlParameter("@ProductID", Convert.ToInt32(item["ProductID"])),
                        new SqlParameter("@Quantity", Convert.ToInt32(item["Quantity"])),
                        new SqlParameter("@UnitPrice", Convert.ToDecimal(item["Price"]))
                    };

                    db.ExecuteNonQuery(detailQuery, detailParams);
                }

                MessageBox.Show($"✅ Order saved successfully!\n\nOrder ID: {orderId}\nCustomer: {cmbCustomers.Text}\nTotal: {txtTotal.Text}",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving order: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close(); 
        }

        private void dgvOrderItems_SelectionChanged(object sender, EventArgs e)
        {
            btnRemoveProduct.Enabled = dgvOrderItems.SelectedRows.Count > 0;
        }

        private void OrderForm_Load(object sender, EventArgs e)
        {
            this.AcceptButton = btnSaveOrder;
            this.CancelButton = btnCancel;
        }
       
    }
}
