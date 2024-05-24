using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string role = "Client";

            SaveUser(firstName, lastName, username, password, role);

            MessageBox.Show("Регистрация прошла успешно.");

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void SaveUser(string firstName, string lastName, string username, string password, string role)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            string procedureName = "AddUser";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    if (connection.State == System.Data.ConnectionState.Open)
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand(procedureName, connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("in_first_name", firstName);
                            command.Parameters.AddWithValue("in_last_name", lastName);
                            command.Parameters.AddWithValue("in_username", username);
                            command.Parameters.AddWithValue("in_password", password);
                            command.Parameters.AddWithValue("in_role", role);

                            command.ExecuteNonQuery();
                        }

                        Console.WriteLine("Данные пользователя успешно сохранены.");
                    }
                    else
                    {
                        Console.WriteLine("Не удалось установить подключение к базе данных.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при сохранении данных пользователя: " + ex.Message);
                }
            }
        }
    }
}
