using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Numerics;
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
using System.Configuration;
using Npgsql;
using System.Globalization;
using static medica_center.ClientWindow;
using System.Windows.Media.Media3D;

namespace medica_center
{
    public partial class ClientWindow : Window
    {
        public ClientWindow()
        {
            InitializeComponent();
            FillDoctorComboBox();
            DisplayDoctors();
            DisplayServices();
            DisplayPromotions();
            DisplayReviews();
            DisplayMedicalResultAndDiagnosis(LoginWindow.authenticatedUserId);
            FillPrescribes(LoginWindow.authenticatedUserId);
        }

        private int offset = 0;
        private int limit = 3;

        public class PrescribedMedication
        {
            public int Medication_ID_pres_diag { get; set; }
            public string Medication_Name_pres_diag { get; set; }
            public string Dosage_pres_diag { get; set; }
            public int Duration_pres_diag { get; set; }
            public decimal Medication_Price { get; set; }
        }

        public class PrescribedProcedure
        {
            public int Procedure_ID_pres_proc { get; set; }
            public string Procedure_Name_pres_proc { get; set; }
            public int Duration_pres_proc { get; set; }
        }

        public class Doctor
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class Appointment
        {
            public int appointment_id { get; set; }
            public int user_id { get; set; }
            public int doctor_id { get; set; }
            public DateTime appointment_date_time { get; set; }
        }


        private void FillDoctorComboBox()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ClientConnection"].ConnectionString;

                string sqlQuery = "SELECT * from get_doctors_for_combobox()";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                    {
                        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            List<Doctor> doctors = new List<Doctor>();

                            foreach (DataRow row in dataTable.Rows)
                            {
                                int id = Convert.ToInt32(row["d_ID"]);
                                string firstName = row["d_Name"].ToString();
                                string lastName = row["d_Last_Name"].ToString();
                                string fullName = $"{firstName} {lastName}";
                                Doctor doctor = new Doctor { ID = id, Name = fullName };
                                doctors.Add(doctor);
                            }

                            doctorComboBox.ItemsSource = doctors;
                            doctorComboBox.DisplayMemberPath = "Name";
                            doctorComboBox.SelectedValuePath = "ID";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении списка врачей: " + ex.Message);
            }
        }

        private void BookAppointment_Click(object sender, RoutedEventArgs e)
        {
            int userId = LoginWindow.authenticatedUserId;
            int doctorId = (int)doctorComboBox.SelectedValue;

            if (doctorComboBox.SelectedValue == null || timeComboBox.SelectedItem == null || datePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            string selectedTimeStr = ((ComboBoxItem)timeComboBox.SelectedItem).Content.ToString();
            TimeSpan selectedTime = TimeSpan.Parse(selectedTimeStr);
            DateTime selectedDate = datePicker.SelectedDate.Value.Date;

            if (selectedDate < DateTime.Now.Date)
            {
                MessageBox.Show("Выбранная дата не может быть меньше текущего момента.");
                return;
            }

            DateTime appointmentDateTime = selectedDate.Add(selectedTime);

            string connectionString = ConfigurationManager.ConnectionStrings["ClientConnection"].ConnectionString;

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand("make_appointment", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("user_id", userId);
                        command.Parameters.AddWithValue("doctor_id", doctorId);
                        command.Parameters.AddWithValue("appointment_date_time", appointmentDateTime);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Запись на приём прошла успешно.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при записи на приём: " + ex.Message);
            }
        }

        private void CancelAppointment_Click(object sender, RoutedEventArgs e)
        {

            if (appointmentsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для отмены приёма.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Appointment selectedAppointment = (Appointment)appointmentsDataGrid.SelectedItem;
            int appointmentId = selectedAppointment.appointment_id;

            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand("CALL cancel_appointment(@appointment_id)", conn);
            cmd.Parameters.AddWithValue("appointment_id", appointmentId);
            cmd.ExecuteNonQuery();

            conn.Close();

            LoadAppointmentsData();

        }

        private void LoadAppointmentsData()
        {
            int userId = LoginWindow.authenticatedUserId;

            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM get_user_appointments(@p_user_id)", conn);
            cmd.Parameters.AddWithValue("p_user_id", userId);

            NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            List<Appointment> appointments = new List<Appointment>();
            foreach (DataRow row in dt.Rows)
            {
                Appointment appointment = new Appointment();
                appointment.appointment_id = Convert.ToInt32(row["id"]);
                appointment.user_id = Convert.ToInt32(row["user_id"]);
                appointment.doctor_id = Convert.ToInt32(row["doctor_id"]);
                appointment.appointment_date_time = Convert.ToDateTime(row["appointment_date_time"]);

                appointments.Add(appointment);
            }

            appointmentsDataGrid.ItemsSource = appointments;

            conn.Close();
        }

        private void ShowAppointments_Click(object sender, RoutedEventArgs e)
        {
            LoadAppointmentsData();
        }

        private void DisplayDoctors()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM get_all_doctors()", conn);
            NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string firstName = reader.GetString(reader.GetOrdinal("First_Name"));
                string lastName = reader.GetString(reader.GetOrdinal("Last_Name"));
                string specialization = reader.GetString(reader.GetOrdinal("Specialization"));
                string schedule = reader.GetString(reader.GetOrdinal("Schedule"));

                DoctorCard doctorCard = new DoctorCard();
                doctorCard.FirstName = firstName;
                doctorCard.LastName = lastName;
                doctorCard.Specialization = specialization;
                doctorCard.Schedule = schedule;

                doctorsContainer.Children.Add(doctorCard);
            }

            conn.Close();
        }

        private void DisplayServices()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM get_services_and_procedures()", conn);
            NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(reader.GetOrdinal("ID_serv"));
                string name = reader.GetString(reader.GetOrdinal("Name_serv"));
                string description = reader.GetString(reader.GetOrdinal("Description_serv"));
                decimal price = reader.GetDecimal(reader.GetOrdinal("Price_serv"));
                string orientation = reader.GetString(reader.GetOrdinal("Orientation_serv"));

                ServiceCard serviceCard = new ServiceCard();
                serviceCard.Name = name;
                serviceCard.Description = description;
                serviceCard.Price = price;
                serviceCard.Orientation = orientation;

                servicesContainer.Children.Add(serviceCard);
            }

            conn.Close();
        }

        private void DisplayPromotions()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM get_promotions(@offset, @limit)", conn);
            cmd.Parameters.AddWithValue("offset", offset);
            cmd.Parameters.AddWithValue("limit", limit);
            NpgsqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(reader.GetOrdinal("ID_prom"));
                string name = reader.GetString(reader.GetOrdinal("Name_prom"));
                string description = reader.GetString(reader.GetOrdinal("Description_prom"));
                DateTime startDate = reader.GetDateTime(reader.GetOrdinal("Start_Date_prom"));
                DateTime endDate = reader.GetDateTime(reader.GetOrdinal("End_Date_prom"));

                PromotionCard promotionCard = new PromotionCard();
                promotionCard.Name = name;
                promotionCard.Description = description;
                promotionCard.StartDate = startDate;
                promotionCard.EndDate = endDate;

                promotionsContainer.Children.Add(promotionCard);
            }

            conn.Close();
        }


        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DisplayPromotions();

            offset += limit;
        }


        private void DisplayReviews()
        {
            try
            {
                using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand("SELECT * FROM get_recent_reviews_with_nickname()", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nickname = reader.GetString(0);
                                string reviewText = reader.GetString(1);
                                DateTime reviewDate = reader.GetDateTime(2);

                                ReviewCard reviewCard = new ReviewCard();
                                reviewCard.Nickname = nickname;
                                reviewCard.ReviewText = reviewText;
                                reviewCard.ReviewDate = reviewDate.ToShortDateString();

                                reviewContainer.Children.Add(reviewCard);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении отзывов: " + ex.Message);
            }
        }





        private void LeaveReviewButton_Click(object sender, RoutedEventArgs e)
        {
            string reviewText = reviewTextBox.Text;
            int userId = LoginWindow.authenticatedUserId;

            try
            {
                using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand("leave_review", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("user_id", userId);
                        command.Parameters.AddWithValue("review_text", reviewText);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Отзыв успешно оставлен.");
                        reviewTextBox.Text = "";
                        reviewContainer.Children.Clear();
                        DisplayReviews();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оставлении отзыва: " + ex.Message);
            }
        }


        private void DisplayMedicalResultAndDiagnosis(int userId)
        {
            List<MedicalResult> medicalResults = GetMedicalResults(userId);

            List<Diagnosis> diagnoses = GetDiagnoses(userId);

            MedResultAndDiagnos medResultAndDiagnos = new MedResultAndDiagnos();
            resultsContainer.Children.Add(medResultAndDiagnos);

            if (medicalResults.Count > 0)
            {
                MedicalResult latestResult = medicalResults[0];
                medResultAndDiagnos.ResultDescription = latestResult.ResultDescription;
                medResultAndDiagnos.Date = latestResult.Date.ToString("dd.MM.yyyy");
            }

            if (diagnoses.Count > 0)
            {
                Diagnosis latestDiagnosis = diagnoses[0];
                medResultAndDiagnos.DiagnosingDoctorName = GetDoctorName(latestDiagnosis.DoctorId);
                medResultAndDiagnos.DiagnosisName = latestDiagnosis.DiagnosisDescription;
                medResultAndDiagnos.DiagnosisDate = latestDiagnosis.Date.ToString("dd.MM.yyyy");
            }
        }

        private List<MedicalResult> GetMedicalResults(int userId)
        {
            List<MedicalResult> medicalResults = new List<MedicalResult>();

            using (NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT ID_res, Doctor_ID_res, Result_Description_res, Date_res FROM get_medical_results(@UserId)", connection))
                {
                    command.Parameters.AddWithValue("UserId", userId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            int doctorId = reader.GetInt32(1);
                            string resultDescription = reader.GetString(2);
                            DateTime date = reader.GetDateTime(3);

                            MedicalResult medicalResult = new MedicalResult(id, doctorId, resultDescription, date);
                            medicalResults.Add(medicalResult);
                        }
                    }
                }
            }

            return medicalResults;
        }

        private List<Diagnosis> GetDiagnoses(int userId)
        {
            List<Diagnosis> diagnoses = new List<Diagnosis>();

            using (NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT ID_diag, Doctor_ID_diag, Diagnosis_Description_diag, Date_diag FROM get_diagnoses(@UserId)", connection))
                {
                    command.Parameters.AddWithValue("UserId", userId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            int doctorId = reader.GetInt32(1);
                            string diagnosisDescription = reader.GetString(2);
                            DateTime date = reader.GetDateTime(3);

                            Diagnosis diagnosis = new Diagnosis(id, doctorId, diagnosisDescription, date);
                            diagnoses.Add(diagnosis);
                        }
                    }
                }
            }

            return diagnoses;
        }

        private string GetDoctorName(int doctorId)
        {
            string doctorName = string.Empty;

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ClientConnection"].ConnectionString;

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand("SELECT GetDoctorName_for_client(@DoctorId)", connection))
                    {
                        command.Parameters.AddWithValue("DoctorId", doctorId);
                        doctorName = command.ExecuteScalar()?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении имени врача: " + ex.Message);
            }

            return doctorName;
        }

        public class MedicalResult
        {
            public int Id { get; set; }
            public int DoctorId { get; set; }
            public string ResultDescription { get; set; }
            public DateTime Date { get; set; }

            public MedicalResult(int id, int doctorId, string resultDescription, DateTime date)
            {
                Id = id;
                DoctorId = doctorId;
                ResultDescription = resultDescription;
                Date = date;
            }
        }

        public class Diagnosis
        {
            public int Id { get; set; }
            public int DoctorId { get; set; }
            public string DiagnosisDescription { get; set; }
            public DateTime Date { get; set; }

            public Diagnosis(int id, int doctorId, string diagnosisDescription, DateTime date)
            {
                Id = id;
                DoctorId = doctorId;
                DiagnosisDescription = diagnosisDescription;
                Date = date;
            }
        }

        private void FillPrescribes(int userId)
        {
            List<Diagnosis> diagnoses = GetDiagnoses(userId);

            foreach (Diagnosis diagnosis in diagnoses)
            {
                var prescribedMedications = GetPrescribedMedications(diagnosis.Id);
                var prescribedProcedures = GetPrescribedProcedures(diagnosis.Id);

                medicationDataGrid.ItemsSource = prescribedMedications;
                procedureDataGrid.ItemsSource = prescribedProcedures;

            }
        }



        private List<PrescribedMedication> GetPrescribedMedications(int diagnosisId)
        {
            List<PrescribedMedication> medications = new List<PrescribedMedication>();

            using (NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM get_prescribed_medications(@diagnosiss_id)", connection))
                {
                    command.Parameters.AddWithValue("@diagnosiss_id", diagnosisId);
                    command.CommandType = CommandType.Text;

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PrescribedMedication medication = new PrescribedMedication
                            {
                                Medication_ID_pres_diag = reader.GetInt32(0),
                                Medication_Name_pres_diag = reader.GetString(1),
                                Dosage_pres_diag = reader.GetString(2),
                                Duration_pres_diag = reader.GetInt32(3),
                                Medication_Price = reader.GetDecimal(4)
                            };

                            medications.Add(medication);
                        }
                    }
                }
            }

            return medications;
        }

        private List<PrescribedProcedure> GetPrescribedProcedures(int diagnosisId)
        {
            List<PrescribedProcedure> procedures = new List<PrescribedProcedure>();

            using (NpgsqlConnection connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM get_prescribed_procedures(@diagnosiss_id)", connection))
                {
                    command.Parameters.AddWithValue("@diagnosiss_id", diagnosisId);
                    command.CommandType = CommandType.Text;

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PrescribedProcedure procedure = new PrescribedProcedure
                            {
                                Procedure_ID_pres_proc = reader.GetInt32(0),
                                Duration_pres_proc = reader.GetInt32(1),
                                Procedure_Name_pres_proc = reader.GetString(2)
                            };

                            procedures.Add(procedure);
                        }
                    }
                }
            }

            return procedures;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.authenticatedUserId = 0;
            LoginWindow.authenticatedDoctorId = 0;
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void ConfirmFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            decimal? minPrice = null;
            decimal? maxPrice = null;
            string searchQuery = null;
            string procedure_orientation = null;

            if (!string.IsNullOrEmpty(MinPriceTextBox.Text))
            {
                minPrice = decimal.Parse(MinPriceTextBox.Text);
            }

            if (!string.IsNullOrEmpty(MaxPriceTextBox.Text))
            {
                maxPrice = decimal.Parse(MaxPriceTextBox.Text);
            }

            if (!string.IsNullOrEmpty(NameTextBox.Text))
            {
                searchQuery = NameTextBox.Text;
            }

            if (!string.IsNullOrEmpty(ProcedureOrientationComboBox.Text))
            {
                procedure_orientation = ProcedureOrientationComboBox.Text;
            }

            using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
            {
                connection.Open();

                using (var command = new NpgsqlCommand("SELECT * FROM filter_services(@minPrice, @maxPrice, @searchQuery, @procedure_orientation)", connection))
                {
                    command.Parameters.AddWithValue("@minPrice", minPrice.HasValue ? (object)minPrice.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@maxPrice", maxPrice.HasValue ? (object)maxPrice.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@searchQuery", string.IsNullOrEmpty(searchQuery) ? DBNull.Value : (object)searchQuery);
                    command.Parameters.AddWithValue("@procedure_orientation", string.IsNullOrEmpty(procedure_orientation) ? DBNull.Value : (object)procedure_orientation);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            servicesContainer.Children.Clear();
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(reader.GetOrdinal("id"));
                                string name = reader.GetString(reader.GetOrdinal("name"));
                                string description = reader.GetString(reader.GetOrdinal("description"));
                                decimal price = reader.GetDecimal(reader.GetOrdinal("price"));
                                string orientation = reader.GetString(reader.GetOrdinal("orientation"));

                                ServiceCard serviceCard = new ServiceCard();
                                serviceCard.Name = name;
                                serviceCard.Description = description;
                                serviceCard.Price = price;
                                serviceCard.Orientation = orientation;

                                servicesContainer.Children.Add(serviceCard);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Ничего не найдено.");
                        }
                    }
                }
            }
        }

        //выгрузка комментариев из файла
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                ImportJsonData();

                using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand("SELECT * FROM get_recent_reviews_with_nickname()", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string nickname = reader.GetString(0);
                                string reviewText = reader.GetString(1);
                                DateTime reviewDate = reader.GetDateTime(2);

                                ReviewCard reviewCard = new ReviewCard();
                                reviewCard.Nickname = nickname;
                                reviewCard.ReviewText = reviewText;
                                reviewCard.ReviewDate = reviewDate.ToShortDateString();

                                reviewContainer.Children.Add(reviewCard);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении отзывов: " + ex.Message);
            }
        }

        private void ImportJsonData()
        {
            try
            {
                using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand("SELECT import_json_data()", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при импорте данных из JSON: " + ex.Message);
            }
        }

        
    }
}
