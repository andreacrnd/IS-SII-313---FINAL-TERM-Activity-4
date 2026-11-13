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
    public partial class UserManagement : Form
    {
        private int selectedUserID = 0;

        public UserManagement()
        {
            InitializeComponent();

            btnAddUser.Click += btnAddUser_Click;
            btnConfirm.Click += btnConfirm_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += btnClear_Click;
            btnRefresh.Click += btnRefresh_Click;
            dgvUsers.CellClick += dgvUsers_CellClick;

            CustomizeDataGridView();
        }

        private void CustomizeDataGridView()
        {
            dgvUsers.BorderStyle = BorderStyle.None;
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 243, 246);
            dgvUsers.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(41, 128, 185);
            dgvUsers.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvUsers.BackgroundColor = Color.White;

            dgvUsers.EnableHeadersVisualStyles = false;
            dgvUsers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvUsers.ColumnHeadersHeight = 35;
        }
        private void UserManagement_Load(object sender, EventArgs e)
        {
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            LoadRoles();
            loadUsers();
        }

        private void LoadRoles()
        {
            cmbRole.Items.Clear();
            cmbRole.Items.Add("Admin");
            cmbRole.Items.Add("Staff");
            cmbRole.Items.Add("Manager");
        }

        private void loadUsers()
        {
            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();
                    string query = "SELECT * FROM UserManagement ORDER BY UserID DESC";
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, con))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvUsers.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load users: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            txtUsername.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            cmbRole.SelectedIndex = -1;
            selectedUserID = 0;
            dgvUsers.ClearSelection();
        }

        private bool ValidateUser()
        {
            if (string.IsNullOrEmpty(txtUsername.Text.Trim()))
            {
                MessageBox.Show("Please enter a username.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(txtPassword.Text.Trim()))
            {
                MessageBox.Show("Please enter a password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return false;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("Passwords do not match!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtConfirmPassword.Focus();
                return false;
            }

            if (cmbRole.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a user role.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbRole.Focus();
                return false;
            }

            return true;
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            ClearFields();
            txtUsername.Focus();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!ValidateUser())
                return;

            string hashedPassword = PasswordHelper.HashPassword(txtPassword.Text.Trim());

            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();
                    string query = @"INSERT INTO UserManagement ([Username], [Email], [Password], [Role]) VALUES (?, ?, ?, ?)";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtUsername.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtEmail.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = hashedPassword;
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = cmbRole.Text.Trim();

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields();
                loadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding user: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUsers.Rows[e.RowIndex];

                selectedUserID = Convert.ToInt32(row.Cells[0].Value);

                if (row.Cells[1].Value != DBNull.Value)
                    txtUsername.Text = row.Cells[1].Value.ToString();
                else
                    txtUsername.Clear();

                if (row.Cells[2].Value != DBNull.Value)
                    txtEmail.Text = row.Cells[2].Value.ToString();
                else
                    txtEmail.Clear();

                txtPassword.Clear();
                txtConfirmPassword.Clear();

                if (row.Cells[4].Value != DBNull.Value)
                    cmbRole.Text = row.Cells[4].Value.ToString();
                else
                    cmbRole.SelectedIndex = -1;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedUserID == 0)
            {
                MessageBox.Show("Please select a user row from the list to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateUser())
                return;

            string hashedPassword = PasswordHelper.HashPassword(txtPassword.Text.Trim());

            try
            {
                using (OleDbConnection con = Database.GetConnection())
                {
                    con.Open();
                    string query = @"UPDATE UserManagement SET [Username] = ?, [Email] = ?, [Password] = ?, [Role] = ? WHERE [UserID] = ?";

                    using (OleDbCommand cmd = new OleDbCommand(query, con))
                    {
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtUsername.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = txtEmail.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = hashedPassword;
                        cmd.Parameters.Add("?", OleDbType.VarWChar).Value = cmbRole.Text.Trim();
                        cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedUserID;

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearFields();
                loadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating user: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedUserID == 0)
            {
                MessageBox.Show("Please select a user row to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (OleDbConnection con = Database.GetConnection())
                    {
                        con.Open();
                        string query = "DELETE FROM UserManagement WHERE UserID = ?";

                        using (OleDbCommand cmd = new OleDbCommand(query, con))
                        {
                            cmd.Parameters.Add("?", OleDbType.Integer).Value = selectedUserID;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("User deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    loadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting user: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            loadUsers();
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            DashboardForm dashboard = new DashboardForm();
            dashboard.Show();
            this.Hide();
        }

        private void btnUserManagement_Click(object sender, EventArgs e) { }
        private void txtEmail_TextChanged(object sender, EventArgs e) { }
        private void panelForm_Paint(object sender, PaintEventArgs e) { }
        private void btnConfirm_Click_1(object sender, EventArgs e) { }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DashboardForm dashboard = new DashboardForm();
            dashboard.Show();
            this.Hide();
        }
    }
}