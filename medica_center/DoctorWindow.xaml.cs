using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
using static medica_center.ClientWindow;

namespace medica_center
{
    public partial class DoctorWindow : Window
    {
        public DoctorWindow()
        {
            InitializeComponent();
            DisplayDoctorInfo();
            FillUsersDataGridView();
            
        }


        private void DisplayDoctorInfo()
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=doctorr;Password=doctor;");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM GetDoctorById(@doctorId)", conn);
            cmd.Parameters.AddWithValue("doctorId", LoginWindow.authenticatedDoctorId);
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

                doctorContainer.Children.Add(doctorCard);
            }

            conn.Close();
        }

        private void ConfirmNewSchedule_Click(object sender, RoutedEventArgs e)
        {
            string newSchedule = newScheduleTextBox.Text;

            if (string.IsNullOrEmpty(newSchedule))
            {
                MessageBox.Show("Для начала заполните поле с новым расписанием.");
                return;
            }

            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=doctorr;Password=doctor;");
            conn.Open();

            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("update_doctor_schedule", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("doctor_id", LoginWindow.authenticatedDoctorId);
                cmd.Parameters.AddWithValue("new_schedule", newSchedule);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Расписание успешно обновлено.");
                
                newScheduleTextBox.Text = "";
                doctorContainer.Children.Clear();
                DisplayDoctorInfo();

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении расписания: " + ex.Message);
            }
        }


        private void FillUsersDataGridView()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=doctorr;Password=doctor;"))
            {
                conn.Open();

                NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM get_all_users()", conn);
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                adapter.Fill(dt);
                usersDataGrid.ItemsSource = dt.DefaultView;

                conn.Close();
            }
        }

        private void usersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (usersDataGrid.SelectedItem != null)
            {

                resultsContainer.Children.Clear();

                DataRowView selectedRow = (DataRowView)usersDataGrid.SelectedItem;
                int userId = (int)selectedRow["userId"];

                DisplayMedicalResultAndDiagnosis(userId);
                FillPrescribes(userId);
                LoadMedications();
                LoadProcedures();
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

        private void ConfirmDiagnosisButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)usersDataGrid.SelectedItem;
            int userId = (int)selectedRow["userId"];
            if (selectedRow != null)
            {
                int doctorId = LoginWindow.authenticatedDoctorId;
                string diagnosisDescription = diagnosisTextBox.Text;

                if (string.IsNullOrWhiteSpace(diagnosisDescription))
                {
                    MessageBox.Show("Введите описание диагноза.");
                    return;
                }

                bool hasExistingDiagnosis = CheckExistingDiagnosis(userId);

                if (hasExistingDiagnosis)
                {
                    MessageBox.Show("У пациента уже есть диагноз. Невозможно установить новый диагноз.");
                }
                else
                {
                    SetDiagnosis(userId, doctorId, diagnosisDescription);
                    MessageBox.Show("Диагноз успешно назначен.");
                    resultsContainer.Children.Clear();
                    DisplayMedicalResultAndDiagnosis(userId);
                }
            }
            else
            {
                MessageBox.Show("Выберите клиента из списка.");
            }
        }

        private bool CheckExistingDiagnosis(int userId)
        {
            using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=doctorr;Password=doctor;"))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT check_existing_diagnosis(@user_id)", connection))
                {
                    command.Parameters.AddWithValue("user_id", userId);
                    bool hasExistingDiagnosis = (bool)command.ExecuteScalar();
                    return hasExistingDiagnosis;
                }
            }
        }

        private void SetDiagnosis(int userId, int doctorId, string diagnosisDescription)
        {
            using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=doctorr;Password=doctor;"))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("set_diagnoses", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("user_id", userId);
                    command.Parameters.AddWithValue("doctor_id", doctorId);
                    command.Parameters.AddWithValue("diagnosis_description", diagnosisDescription);

                    try
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine($"Установлен диагноз для пользователя {userId}: {diagnosisDescription}");
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine("Ошибка при установке диагноза: " + ex.Message);
                    }
                }
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


        private void LoadMedications()
        {
            using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=doctorr;Password=doctor;"))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM get_all_medications()", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        allMedicationDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
        }


        private void LoadProcedures()
        {
            using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=doctorr;Password=doctor;"))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM get_all_procedures()", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(reader);
                        allProcedureDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
            }
        }

        private void PrescribeNewMedicationsButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)allMedicationDataGrid.SelectedItem;

            DataRowView selectedRoww = (DataRowView)usersDataGrid.SelectedItem;
            int userId = (int)selectedRoww["userId"];
            if (selectedRow != null)
            {
                List<Diagnosis> userDiagnoses = GetDiagnoses(userId);
                int diagnosisId = userDiagnoses[0].Id;

                int medicationId = Convert.ToInt32(selectedRow["med_ID"]);
                string medicationName = (string)selectedRow["med_name"];

                string dosage = MedDosageTextBox.Text;

                if (!int.TryParse(MedDurationTextBox.Text, out int duration))
                {
                    MessageBox.Show("Значение длительности должно быть целым числом.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(MedDosageTextBox.Text))
                {
                    MessageBox.Show("Введите дозировку лекарства.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(MedDurationTextBox.Text))
                {
                    MessageBox.Show("Введите продолжительность приема лекарства.");
                    return;
                }

                using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=doctorr;Password=doctor;"))
                {
                    connection.Open();

                    using (var checkCommand = new NpgsqlCommand("SELECT CheckPrescribedMedication(@medicationName)", connection))
                    {
                        checkCommand.Parameters.AddWithValue("medicationName", medicationName);
                        bool isPrescribed = (bool)checkCommand.ExecuteScalar();

                        if (isPrescribed)
                        {
                            MessageBox.Show("Этот препарат уже выписан.");
                            return;
                        }
                    }

                    using (var command = new NpgsqlCommand("prescribe_medications", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("diagnosis_id", diagnosisId);
                        command.Parameters.AddWithValue("medication_id", medicationId);
                        command.Parameters.AddWithValue("dosage", dosage);
                        command.Parameters.AddWithValue("duration", duration);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Лекарство успешно назначено.");
                        FillPrescribes(userId);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите препарат из списка.");
            }
        }

        private void PrescribeNewProceduresButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)allProcedureDataGrid.SelectedItem;

            DataRowView selectedRoww = (DataRowView)usersDataGrid.SelectedItem;
            int userId = (int)selectedRoww["userId"];
            if (selectedRow != null)
            {
                List<Diagnosis> userDiagnoses = GetDiagnoses(userId);
                int diagnosisId = userDiagnoses[0].Id;

                int procedureId = Convert.ToInt32(selectedRow["proc_ID"]);
                string procedureName = (string)selectedRow["proc_name"];

                if (!int.TryParse(ProcDurationTextBox.Text, out int duration))
                {
                    MessageBox.Show("Значение продолжительности должно быть целым числом.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(ProcDurationTextBox.Text))
                {
                    MessageBox.Show("Введите продолжительность процедуры.");
                    return;
                }

                using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=doctorr;Password=doctor;"))
                {
                    connection.Open();

                    using (var checkCommand = new NpgsqlCommand("SELECT CheckPrescribedProcedure(@procedureName)", connection))
                    {
                        checkCommand.Parameters.AddWithValue("procedureName", procedureName);
                        bool isPrescribed = (bool)checkCommand.ExecuteScalar();

                        if (isPrescribed)
                        {
                            MessageBox.Show("Эта процедура уже назначена.");
                            return;
                        }
                    }


                    using (var command = new NpgsqlCommand("prescribe_procedures", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("diagnosis_id", diagnosisId);
                        command.Parameters.AddWithValue("procedure_id", procedureId);
                        command.Parameters.AddWithValue("duration", duration);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Процедура успешно назначена.");
                        FillPrescribes(userId);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите процедуру из списка.");
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (usersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя.");
                return;
            }

            DataRowView selectedRow = (DataRowView)usersDataGrid.SelectedItem;
            int userId = (int)selectedRow["userId"];

            try
            {
                using (var connection = new NpgsqlConnection("Server=localhost;Port=5432;Database=medical_center;User Id=clientt;Password=client;"))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand("SELECT export_user_statistics(@userId)", connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Статистика пользователя успешно экспортирована.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при экспорте статистики пользователя: " + ex.Message);
            }

        }
    }
}
