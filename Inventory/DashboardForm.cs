using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Inventory
{
    public partial class DashboardForm : Form

    {
        private int selectedProductID = 0;
        private sampledbDataSetTableAdapters.ProductsTableAdapter productsTableAdapter = new sampledbDataSetTableAdapters.ProductsTableAdapter();
        public DashboardForm()
        {
            InitializeComponent();

            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += btnClear_Click;
            btnRefresh.Click += btnRefresh_Click;

            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            // txtSearch.TextChanged += txtSearch_TextChange;
            dgvProducts.CellClick += dgvProducts_CellClick;

            CustomizeDataGridView();

            loadProducts();
            loadDashboardStats();
        }

        private void loadDashboardStats()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"SELECT COUNT(*) AS TotalProducts, 
                                     SUM(IIF(Quantity > 0 AND Quantity <= ReorderLevel, 1, 0)) AS InStock, 
                                     SUM(IIF(Quantity > ReorderLevel, 1, 0)) AS LowStock, 
                                     SUM(IIF(Quantity = 0, 1, 0)) AS OutOfStock FROM Products";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblTotalCount.Text = reader["TotalProducts"].ToString();
                            lblInStockCount.Text = reader["InStock"].ToString();
                            lblLowStockCount.Text = reader["LowStock"].ToString();
                            lblOutOfStockCount.Text = reader["OutOfStock"].ToString();
                        }
                    }

                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Statistic Error" + ex.Message);
            }
        }

        private void loadProducts()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"SELECT * FROM Products ORDER by ProductID DESC";

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();

                        adapter.Fill(dt);

                        dgvProducts.DataSource = dt;
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Unable to load Products:" + ex.Message);
            }
        }

        private void CustomizeDataGridView()
        {
            dgvProducts.BorderStyle = BorderStyle.None;
            dgvProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 243, 246);
            dgvProducts.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProducts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
            dgvProducts.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProducts.BackgroundColor = Color.White;

            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94); 
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProducts.ColumnHeadersHeight = 35;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log out?", "Confrim Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {

                Application.Restart();
            }
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {
            this.productsTableAdapter.Fill(this.sampledbDataSet.Products);
        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private bool ValidateProduct()
        {
            if (string.IsNullOrEmpty(txtProductName.Text.Trim()))
            {
                MessageBox.Show("Please enter a product name.");
                txtProductName.Focus();
                return false;
            }

            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Please enter a product category.");
                cmbCategory.Focus();
                return false;
            }

            decimal price;
            if (!decimal.TryParse(txtUnitPrice.Text, out price) || price < 0)
            {
                MessageBox.Show("Please enter a product price.");
                txtUnitPrice.Focus();
                return false;
            }

            int quantity;
            if (!int.TryParse(txtQuantity.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Please enter a quantity.");
                txtQuantity.Focus();
                return false;
            }

            int reorder;
            if (!int.TryParse(txtReorderLevel.Text, out reorder) || reorder < 0)
            {
                MessageBox.Show("Please enter reorder level.");
                txtReorderLevel.Focus();
                return false;
            }

            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateProduct())
                return;

            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"INSERT INTO Products ([ProductName], [Category], [Description], [UnitPrice], [Quantity], [ReorderLevel], [CreatedAt]) VALUES (?, ?, ?, ?, ?, ?, Now())";
                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtProductName.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = cmbCategory.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.LongVarWChar).Value = txtDescription.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = decimal.Parse(txtUnitPrice.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtQuantity.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtReorderLevel.Text);

                        cmd.ExecuteNonQuery();

                    }
                }

                MessageBox.Show("Product added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearField();
                loadProducts();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error adding Product" + ex.Message);
            }
        }

        private void ClearField()
        {
            txtProductName.Clear();
            cmbCategory.SelectedIndex = -1;
            txtDescription.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            txtReorderLevel.Clear();

            selectedProductID = 0;

            dgvProducts.ClearSelection();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearField();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product row to update.");
                return;
            }

            if (!ValidateProduct())
                return;

            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();

                    string query = @"UPDATE Products SET ProductName=?, Category=?, Description=?, UnitPrice=?, Quantity=?, ReorderLevel=? WHERE ProductID=?";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtProductName.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = cmbCategory.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.LongVarWChar).Value = txtDescription.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = decimal.Parse(txtUnitPrice.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtQuantity.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = int.Parse(txtReorderLevel.Text);
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedProductID;


                        cmd.ExecuteNonQuery();

                    }
                }

                MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearField();
                loadDashboardStats();
                loadProducts();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error updating Product" + ex.Message);
            }
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

                selectedProductID = Convert.ToInt32(row.Cells[0].Value);

                selectedProductID = Convert.ToInt32(row.Cells[0].Value);
                txtProductName.Text = row.Cells[1].Value.ToString();
                cmbCategory.Text = row.Cells[2].Value.ToString();
                txtDescription.Text = row.Cells[3].Value.ToString();
                txtUnitPrice.Text = row.Cells[4].Value.ToString();
                txtQuantity.Text = row.Cells[5].Value.ToString();
                txtReorderLevel.Text = row.Cells[6].Value.ToString();

            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductID == 0)
            {
                MessageBox.Show("Please select a product row to update.");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (OleDbConnection con = Database.GetConnection())
                    {
                        con.Open();

                        string query = @"DELETE FROM Products WHERE ProductID = ?";

                        using (OleDbCommand cmd = new OleDbCommand(query, con))
                        {
                            cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedProductID;

                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Product deleted successfully!");

                    ClearField();
                    loadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting Product: " + ex.Message);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void btnUserManagement_Click(object sender, EventArgs e)
        {
            UserManagement usermanagementForm = new UserManagement();
            usermanagementForm.Show();
            this.Hide();
        }

        private void panelForm_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
