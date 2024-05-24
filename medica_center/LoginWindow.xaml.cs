using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace medica_center
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public static int authenticatedUserId;
        public static int authenticatedDoctorId;

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string role = ((ComboBoxItem)RoleComboBox.SelectedItem)?.Content.ToString();

            if (CheckCredentials(username, password, role))
            {
                if (role == "Administrator")
                {
                    var adminWindow = new AdminWindow();
                    adminWindow.Show();
                }
                else if (role == "Client")
                {
                    var clientWindow = new ClientWindow();
                    clientWindow.Show();
                }
                else if (role == "Doctor")
                {
                    authenticatedDoctorId = GetDoctorIdByUserId(authenticatedUserId);
                    var doctorWindow = new DoctorWindow();
                    doctorWindow.Show();
                }

                Close();
            }
            else
            {
                MessageBox.Show("Неверные учетные данные или роль.");
            }
        }

        private bool CheckCredentials(string username, string password, string role)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            string functionName = "check_credentials";
            bool result = false;

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = $"SELECT {functionName}(@in_username, @in_password, @in_role)";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@in_username", username);
                        command.Parameters.AddWithValue("@in_password", password);
                        command.Parameters.AddWithValue("@in_role", role);

                        result = (bool)command.ExecuteScalar();

                        if (result)
                        {
                            authenticatedUserId = GetUserIdByUsername(username);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при проверке учетных данных: " + ex.Message);
                }
            }

            return result;
        }

        private static int GetUserIdByUsername(string username)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            int userId = -1;

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT get_user_id_by_username(@username)";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);

                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            userId = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при получении идентификатора пользователя: " + ex.Message);
                }
            }

            return userId;
        }


        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            Close();
        }



        private static int GetDoctorIdByUserId(int userId)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            int doctorId = 0;

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand("SELECT GetDoctorIdByUserId(@userId)", connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);

                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            doctorId = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Ошибка при выполнении запроса: " + ex.Message);
                }
            }

            return doctorId;
        }
    }
}
