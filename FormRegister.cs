
using System;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Delacruz
{
    public partial class FormRegister : Form
    {
        private OleDbConnection conn = new OleDbConnection(
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=InventoryDB.accdb");

        public FormRegister()
        {
            InitializeComponent();
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, 100000))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return Convert.ToBase64String(salt) + ":" +
                       Convert.ToBase64String(hash);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form1 login = new Form1();
            login.Show();
            this.Hide();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (txtFullName.Text == "" ||
                txtUsername.Text == "" ||
                txtPassword.Text == "" ||
                txtRePassword.Text == "")
            {
                MessageBox.Show("Please complete all fields.");
            }
            else if (txtPassword.Text.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.");
            }
            else if (txtPassword.Text != txtRePassword.Text)
            {
                MessageBox.Show("Passwords do not match!");
            }
            else
            {
                try
                {
                    string hashedPassword = HashPassword(txtPassword.Text);

                    conn.Open();

                    string query = "INSERT INTO Users ([FullName], [Username], [Password]) VALUES (?, ?, ?)";

                    OleDbCommand cmd = new OleDbCommand(query, conn);

                    cmd.Parameters.AddWithValue("@FullName", txtFullName.Text);
                    cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Registration successful!");

                    txtFullName.Clear();
                    txtUsername.Clear();
                    txtPassword.Clear();
                    txtRePassword.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }

        private void FormRegister_Load(object sender, EventArgs e)
        {

        }
    }
}
