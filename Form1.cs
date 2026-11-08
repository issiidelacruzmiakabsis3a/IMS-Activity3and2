
using System;
using System.Data.OleDb;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Delacruz
{
    public partial class Form1 : Form
    {
        private OleDbConnection conn = new OleDbConnection(
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=InventoryDB.accdb");

        public Form1()
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

        private bool VerifyPassword(string password, string storedPassword,
            out bool needsUpgrade)
        {
            needsUpgrade = false;

            if (!storedPassword.Contains(":"))
            {
                needsUpgrade = true;
                return password == storedPassword;
            }

            try
            {
                string[] parts = storedPassword.Split(':');

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, 100000))
                {
                    byte[] enteredHash = pbkdf2.GetBytes(32);

                    if (enteredHash.Length != storedHash.Length)
                    {
                        return false;
                    }

                    int difference = 0;

                    for (int i = 0; i < enteredHash.Length; i++)
                    {
                        difference |= enteredHash[i] ^ storedHash[i];
                    }

                    return difference == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text == "" || txtPassword.Text == "")
            {
                MessageBox.Show("Please enter your username and password.");
            }
            else
            {
                try
                {
                    conn.Open();

                    string query = "SELECT [Password] FROM Users WHERE [Username] = ?";

                    OleDbCommand cmd = new OleDbCommand(query, conn);

                    cmd.Parameters.AddWithValue("@Username", txtUsername.Text);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string storedPassword = result.ToString();
                        bool needsUpgrade;

                        bool isValid = VerifyPassword(
                            txtPassword.Text,
                            storedPassword,
                            out needsUpgrade);

                        if (isValid)
                        {
                            if (needsUpgrade)
                            {
                                string hashedPassword = HashPassword(txtPassword.Text);

                                string updateQuery = "UPDATE Users SET [Password] = ? WHERE [Username] = ?";

                                OleDbCommand updateCmd = new OleDbCommand(updateQuery, conn);

                                updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                                updateCmd.Parameters.AddWithValue("@Username", txtUsername.Text);

                                updateCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Login successful!");

                            FormDashboard dashboard = new FormDashboard();
                            dashboard.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password.");
                    }
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

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormRegister register = new FormRegister();
            register.Show();
            this.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
