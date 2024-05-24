using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static medica_center.ClientWindow;

namespace medica_center
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            LoadDoctorsDataGrid();
            LoadProceduresDataGrid();
            LoadMedicationsDataGrid();
            FillPromotionsDataGrid();
        }

        int doctor_id;
        int procedure_id;
        int medication_id;
        int promotion_id;

        //добавление доктора
        private void Button_Click_Add(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            string firstName = FirstNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("Пожалуйста, введите имя.");
                return;
            }
            if (!Regex.IsMatch(firstName, @"^[a-zA-Zа-яА-Я]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст (буквы) для имени.");
                return;
            }

            string lastName = LastNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Пожалуйста, введите фамилию.");
                return;
            }
            if (!Regex.IsMatch(lastName, @"^[a-zA-Zа-яА-Я]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст (буквы) для фамилии.");
                return;
            }

            string specialization = SpecializationComboBox.Text;
            if (string.IsNullOrWhiteSpace(specialization))
            {
                MessageBox.Show("Пожалуйста, выберите специализацию.");
                return;
            }

            string schedule = ScheduleTextBox.Text;
            if (string.IsNullOrWhiteSpace(schedule))
            {
                MessageBox.Show("Пожалуйста, введите расписание.");
                return;
            }

            string username = NicknameTextBox.Text;
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Пожалуйста, введите имя пользователя.");
                return;
            }

            string password = PasswordTextBox.Text;
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите пароль.");
                return;
            }

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string checkQuery = "SELECT CheckDoctorExists(@FirstName, @LastName, @Username)";
                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@FirstName", firstName);
                        checkCommand.Parameters.AddWithValue("@LastName", lastName);
                        checkCommand.Parameters.AddWithValue("@Username", username);

                        bool doctorExists = (bool)checkCommand.ExecuteScalar();

                        if (doctorExists)
                        {
                            MessageBox.Show("Указанные данные уже существуют.");
                            return;
                        }
                    }

                    string addQuery = "CALL add_doctor(@FirstName, @LastName, @Specialization, @Schedule, @Username, @Password)";
                    using (NpgsqlCommand addCommand = new NpgsqlCommand(addQuery, connection))
                    {
                        addCommand.Parameters.AddWithValue("@FirstName", firstName);
                        addCommand.Parameters.AddWithValue("@LastName", lastName);
                        addCommand.Parameters.AddWithValue("@Specialization", specialization);
                        addCommand.Parameters.AddWithValue("@Schedule", schedule);
                        addCommand.Parameters.AddWithValue("@Username", username);
                        addCommand.Parameters.AddWithValue("@Password", password);

                        addCommand.ExecuteNonQuery();

                        MessageBox.Show("Врач успешно добавлен.");

                        firstName = "";
                        lastName = "";
                        schedule = "";
                        username = "";
                        password = "";
                    }

                    LoadDoctorsDataGrid();
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при добавлении врача: " + ex.Message);
            }
        }


        //удаление врача
        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            string firstName = FirstNameTextBox.Text;
            string lastName = LastNameTextBox.Text;

            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                bool hasReferences = CheckDoctorReference(connection);

                if (hasReferences)
                {
                    MessageBox.Show("Невозможно удалить врача, т.к. он является действующим лечущим врачом.");
                    return;
                }

                using (NpgsqlCommand command = new NpgsqlCommand("delete_doctor_by_name", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("p_first_name", firstName);
                    command.Parameters.AddWithValue("p_last_name", lastName);

                    try
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show("Врач успешно удалён.");
                        LoadDoctorsDataGrid();
                    }
                    catch (NpgsqlException ex)
                    {
                        MessageBox.Show("Ошибка при удалении врача: " + ex.Message);
                    }
                }
            }
        }

        // Метод для проверки наличия ссылок на врача
        private bool CheckDoctorReference(NpgsqlConnection connection)
        {
            int doctorId = doctor_id;

            using (NpgsqlCommand command = new NpgsqlCommand("SELECT CheckDoctorReference(@doctorId)", connection))
            {
                command.Parameters.AddWithValue("doctorId", doctorId);

                return (bool)command.ExecuteScalar();
            }
        }

        //обновление информации о враче
        private void Button_Click_Update(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;

            int doctorId = doctor_id;
            string firstName = FirstNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("Пожалуйста, введите имя.");
                return;
            }
            if (!Regex.IsMatch(firstName, @"^[a-zA-Zа-яА-Я]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст (буквы) для имени.");
                return;
            }

            string lastName = LastNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Пожалуйста, введите фамилию.");
                return;
            }
            if (!Regex.IsMatch(lastName, @"^[a-zA-Zа-яА-Я]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст (буквы) для фамилии.");
                return;
            }

            string specialization = SpecializationComboBox.Text;
            if (string.IsNullOrWhiteSpace(specialization))
            {
                MessageBox.Show("Пожалуйста, выберите специализацию.");
                return;
            }

            string schedule = ScheduleTextBox.Text;
            if (string.IsNullOrWhiteSpace(schedule))
            {
                MessageBox.Show("Пожалуйста, введите расписание.");
                return;
            }

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand("update_doctor", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        // Параметры процедуры
                        command.Parameters.AddWithValue("doctor_id", doctorId);
                        command.Parameters.AddWithValue("first_name", firstName);
                        command.Parameters.AddWithValue("last_name", lastName);
                        command.Parameters.AddWithValue("specialization", specialization);
                        command.Parameters.AddWithValue("schedule", schedule);

                        command.ExecuteNonQuery();

                        firstName = "";
                        lastName = "";
                        schedule = "";
                    }

                    LoadDoctorsDataGrid();
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при добавлении врача: " + ex.Message);
            }
        }



        private void LoadDoctorsDataGrid()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM get_all_doctors_for_admin()";
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);

                    NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    DoctorsDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }


        //получение информации о враче
        private void DoctorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DoctorsDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)DoctorsDataGrid.SelectedItem;

                doctor_id = (int)selectedRow["f_id"];
                string firstName = selectedRow["f_first_name"].ToString();
                string lastName = selectedRow["f_last_name"].ToString();
                string specialization = selectedRow["f_specialization"].ToString();
                string schedule = selectedRow["f_schedule"].ToString();

                FirstNameTextBox.Text = firstName;
                LastNameTextBox.Text = lastName;
                SpecializationComboBox.Text = specialization;
                ScheduleTextBox.Text = schedule;
            }
        }



        private void LoadProceduresDataGrid()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM get_all_procedures_for_admin()";
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);

                    NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    ProceduresDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void Button_Click_AddProcedure(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            string procedureName = ProcedureNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(procedureName))
            {
                MessageBox.Show("Пожалуйста, введите название процедуры.");
                return;
            }
            if (!Regex.IsMatch(procedureName, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я0-9\s]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст и цифры для названия процедуры, при этом хотя бы одна буква обязательна.");
                return;
            }

            string procedureOrientation = ProcedureOrientationComboBox.Text;

            string procedureDescription = ProcedureDescriptionTextBox.Text;
            if (string.IsNullOrWhiteSpace(procedureDescription))
            {
                MessageBox.Show("Пожалуйста, введите описание процедуры.");
                return;
            }
            if (!Regex.IsMatch(procedureDescription, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я0-9\s]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст, цифры, точку, запятую и пробелы для описания процедуры, при этом хотя бы одна буква обязательна.");
                return;
            }

            decimal procedurePrice;
            if (!decimal.TryParse(ProcedurePriceTextBox.Text, out procedurePrice))
            {
                MessageBox.Show("Некорректное значение цены.");
                return;
            }

            string checkQuery = "SELECT check_procedure_exists(@name)";


            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@name", procedureName);

                    bool procedureExists = (bool)command.ExecuteScalar();

                    if (procedureExists)
                    {
                        MessageBox.Show("Процедура с таким именем уже существует.");
                        return;
                    }
                }

                using (var command = new NpgsqlCommand("add_procedure", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("p_name", procedureName);
                    command.Parameters.AddWithValue("p_description", procedureDescription);
                    command.Parameters.AddWithValue("p_price", procedurePrice);
                    command.Parameters.AddWithValue("p_orientation", procedureOrientation);

                    command.ExecuteNonQuery();
                }

                ProcedureNameTextBox.Text = "";
                ProcedureDescriptionTextBox.Text = "";
                ProcedurePriceTextBox.Text = "";

                MessageBox.Show("Процедура успешно добавлена.");
                LoadProceduresDataGrid();
            }
        }

        private void Button_Click_UpdateProcedure(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            int procedureId = procedure_id;
            string procedureName = ProcedureNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(procedureName))
            {
                MessageBox.Show("Пожалуйста, введите название процедуры.");
                return;
            }
            if (!Regex.IsMatch(procedureName, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я0-9\s]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст и цифры для названия процедуры, при этом хотя бы одна буква обязательна.");
                return;
            }

            string procedureOrientation = ProcedureOrientationComboBox.Text;

            string procedureDescription = ProcedureDescriptionTextBox.Text;
            if (string.IsNullOrWhiteSpace(procedureDescription))
            {
                MessageBox.Show("Пожалуйста, введите описание процедуры.");
                return;
            }
            if (!Regex.IsMatch(procedureDescription, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я0-9\s]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст, цифры, точку, запятую и пробелы для описания процедуры, при этом хотя бы одна буква обязательна.");
                return;
            }

            decimal procedurePrice;
            if (!decimal.TryParse(ProcedurePriceTextBox.Text, out procedurePrice))
            {
                MessageBox.Show("Некорректное значение цены.");
                return;
            }

            string updateQuery = "update_procedure";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(updateQuery, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("p_id", procedureId);
                    command.Parameters.AddWithValue("p_name", procedureName);
                    command.Parameters.AddWithValue("p_description", procedureDescription);
                    command.Parameters.AddWithValue("p_price", procedurePrice);
                    command.Parameters.AddWithValue("p_orientation", procedureOrientation);

                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Процедура успешно обновлена.");
            LoadProceduresDataGrid();
        }

        private void Button_Click_DeleteProcedure(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            int procedureId = procedure_id;

            string checkReferenceQuery = "SELECT check_prcedure_reference(@procedureId)";
            string deleteQuery = "delete_procedure";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var checkReferenceCommand = new NpgsqlCommand(checkReferenceQuery, connection))
                {
                    checkReferenceCommand.Parameters.AddWithValue("@procedureId", procedureId);
                    bool referenceExists = (bool)checkReferenceCommand.ExecuteScalar();

                    if (referenceExists)
                    {
                        MessageBox.Show("Невозможно удалить процедуру, так как она уже назначена пациенту.");
                        return;
                    }
                }

                using (var deleteCommand = new NpgsqlCommand(deleteQuery, connection))
                {
                    deleteCommand.CommandType = CommandType.StoredProcedure;
                    deleteCommand.Parameters.AddWithValue("p_id", procedureId);

                    deleteCommand.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Процедура успешно удалена.");
            LoadProceduresDataGrid();
        }

        private void ProceduresDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProceduresDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)ProceduresDataGrid.SelectedItem;

                procedure_id = (int)selectedRow["f_id"];
                string name = selectedRow["f_name"].ToString();
                string description = selectedRow["f_description"].ToString();
                string price = selectedRow["f_price"].ToString();
                string orientation = selectedRow["f_orientation"].ToString();

                ProcedureNameTextBox.Text = name;
                ProcedureDescriptionTextBox.Text = description;
                ProcedurePriceTextBox.Text = price;
                ProcedureOrientationComboBox.Text = orientation;
            }
        }






        private void LoadMedicationsDataGrid()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM get_all_medication_for_admin()";
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);

                    NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    MedicationsDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }


        private void Button_Click_AddMedication(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            string medicationName = MedicationNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(medicationName))
            {
                MessageBox.Show("Пожалуйста, введите название препарата.");
                return;
            }
            if (!Regex.IsMatch(medicationName, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст и цифры для названия препарата, при этом хотя бы одна буква обязательна.");
                return;
            }

            string medicationDescription = MedicationDescriptionTextBox.Text;
            if (string.IsNullOrWhiteSpace(medicationDescription))
            {
                MessageBox.Show("Пожалуйста, введите описание препарата.");
                return;
            }
            if (!Regex.IsMatch(medicationDescription, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я.,\s]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст, цифры, точку, запятую и пробелы для описания препарата, при этом хотя бы одна буква обязательна.");
                return;
            }

            decimal medicationPrice;
            if (!decimal.TryParse(MedicationPriceTextBox.Text, out medicationPrice))
            {
                MessageBox.Show("Некорректное значение цены.");
                return;
            }


            string checkQuery = "SELECT check_medication_exists(@name)";


            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@name", medicationName);

                    bool procedureExists = (bool)command.ExecuteScalar();

                    if (procedureExists)
                    {
                        MessageBox.Show("Препарат с таким именем уже существует.");
                        return;
                    }
                }

                using (var command = new NpgsqlCommand("add_medication", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("p_name", medicationName);
                    command.Parameters.AddWithValue("p_description", medicationDescription);
                    command.Parameters.AddWithValue("p_price", medicationPrice);

                    command.ExecuteNonQuery();
                }

                MedicationNameTextBox.Text = "";
                MedicationDescriptionTextBox.Text = "";
                MedicationPriceTextBox.Text = "";

                MessageBox.Show("Препарат успешно добавлена.");
                LoadMedicationsDataGrid();
            }
        }

        private void Button_Click_UpdateMedication(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            int medicationId = medication_id;
            string medicationName = MedicationNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(medicationName))
            {
                MessageBox.Show("Пожалуйста, введите название препарата.");
                return;
            }
            if (!Regex.IsMatch(medicationName, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я0-9]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст и цифры для названия препарата, при этом хотя бы одна буква обязательна.");
                return;
            }

            string medicationDescription = MedicationDescriptionTextBox.Text;
            if (string.IsNullOrWhiteSpace(medicationDescription))
            {
                MessageBox.Show("Пожалуйста, введите описание препарата.");
                return;
            }
            if (!Regex.IsMatch(medicationDescription, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я0-9.,\s]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст, цифры, точку, запятую и пробелы для описания препарата, при этом хотя бы одна буква обязательна.");
                return;
            }

            decimal medicationPrice;
            if (!decimal.TryParse(MedicationPriceTextBox.Text, out medicationPrice))
            {
                MessageBox.Show("Некорректное значение цены.");
                return;
            }

            string updateQuery = "update_medication";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(updateQuery, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("p_id", medicationId);
                    command.Parameters.AddWithValue("p_name", medicationName);
                    command.Parameters.AddWithValue("p_description", medicationDescription);
                    command.Parameters.AddWithValue("p_price", medicationPrice);

                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Процедура успешно обновлена.");
            LoadMedicationsDataGrid();
        }

        private void Button_Click_DeleteMedication(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            int medicationId = medication_id;

            string checkReferenceQuery = "SELECT check_medication_reference(@medicationId)";
            string deleteQuery = "delete_medication";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var checkReferenceCommand = new NpgsqlCommand(checkReferenceQuery, connection))
                {
                    checkReferenceCommand.Parameters.AddWithValue("@medicationId", medicationId);
                    bool referenceExists = (bool)checkReferenceCommand.ExecuteScalar();

                    if (referenceExists)
                    {
                        MessageBox.Show("Невозможно удалить препарат, так как он уже прописан пациенту.");
                        return;
                    }
                }

                using (var deleteCommand = new NpgsqlCommand(deleteQuery, connection))
                {
                    deleteCommand.CommandType = CommandType.StoredProcedure;
                    deleteCommand.Parameters.AddWithValue("p_id", medicationId);

                    deleteCommand.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Препарат успешно удалён.");
            LoadMedicationsDataGrid();
        }

        private void MedicationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MedicationsDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)MedicationsDataGrid.SelectedItem;

                medication_id = (int)selectedRow["f_id"];
                string name = selectedRow["f_name"].ToString();
                string description = selectedRow["f_description"].ToString();
                string price = selectedRow["f_price"].ToString();

                MedicationNameTextBox.Text = name;
                MedicationDescriptionTextBox.Text = description;
                MedicationPriceTextBox.Text = price;
            }
        }



        private void FillPromotionsDataGrid()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM get_all_promotions_for_admin()";
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);

                    NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    PromotionsDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
        }

        private void Button_Click_AddPromotion(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            string promotionName = PromotionNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(promotionName))
            {
                MessageBox.Show("Пожалуйста, введите название акции.");
                return;
            }
            if (!Regex.IsMatch(promotionName, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я0-9]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст и цифры для названия акции, при этом хотя бы одна буква обязательна.");
                return;
            }

            string promotionDescription = PromotionDescriptionTextBox.Text;
            if (string.IsNullOrWhiteSpace(promotionDescription))
            {
                MessageBox.Show("Пожалуйста, введите описание акции.");
                return;
            }
            if (!Regex.IsMatch(promotionDescription, @"^(?=.*[a-zA-Zа-яА-Я])[a-zA-Zа-яА-Я0-9.,\s]+$"))
            {
                MessageBox.Show("Пожалуйста, введите только текст, цифры, точку, запятую и пробелы для описания акции, при этом хотя бы одна буква обязательна.");
                return;
            }

            DateTime startDate = PromotionStartDatePicker.SelectedDate.GetValueOrDefault();
            DateTime endDate = PromotionEndDatePicker.SelectedDate.GetValueOrDefault();

            if (startDate < DateTime.Now)
            {
                MessageBox.Show("Дата начала акции не может быть меньше текущего момента.");
                return;
            }

            if (endDate < startDate)
            {
                MessageBox.Show("Дата окончания акции не может быть меньше даты начала.");
                return;
            }

            string checkQuery = "SELECT check_promotion_exists(@name)";
            string addPromotionQuery = "CALL add_promotion(@p_Name, @p_Description, @p_Start_Date, @p_End_Date)";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@name", promotionName);

                    bool promotionExists = (bool)command.ExecuteScalar();

                    if (promotionExists)
                    {
                        MessageBox.Show("Акция с таким именем уже существует.");
                        return;
                    }
                }

                using (var command = new NpgsqlCommand(addPromotionQuery, connection))
                {
                    command.Parameters.AddWithValue("@p_Name", promotionName);
                    command.Parameters.AddWithValue("@p_Description", promotionDescription);
                    command.Parameters.AddWithValue("@p_Start_Date", startDate);
                    command.Parameters.AddWithValue("@p_End_Date", endDate);

                    command.ExecuteNonQuery();
                }

                PromotionNameTextBox.Text = "";
                PromotionDescriptionTextBox.Text = "";
                PromotionStartDatePicker.SelectedDate = null;
                PromotionEndDatePicker.SelectedDate = null;

                MessageBox.Show("Акция успешно добавлена.");
                FillPromotionsDataGrid();
            }
        }



        private void Button_Click_DeletePromotion(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AdminConnection"].ConnectionString;
            int promotionID = promotion_id;

            string deleteQuery = "CALL delete_promotion(@p_ID)";

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@p_ID", promotionID);

                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Акция успешно удалена.");
                FillPromotionsDataGrid();
            }
        }


        private void PromotionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PromotionsDataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)PromotionsDataGrid.SelectedItem;

                promotion_id = (int)selectedRow["f_id"];
                string name = selectedRow["f_name"].ToString();
                string description = selectedRow["f_description"].ToString();
                string startDate = selectedRow["f_start_date"].ToString();
                string endDate = selectedRow["f_end_date"].ToString();

                PromotionNameTextBox.Text = name;
                PromotionDescriptionTextBox.Text = description;
                PromotionStartDatePicker.Text = startDate;
                PromotionEndDatePicker.Text = endDate;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.authenticatedUserId = 0;
            LoginWindow.authenticatedDoctorId = 0;
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

    }
}
