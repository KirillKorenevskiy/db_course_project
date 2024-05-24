using System.Windows;
using System.Windows.Controls;

namespace medica_center
{
    public partial class MedResultAndDiagnos : UserControl
    {
        // Свойство для привязки результата обследования
        public string ResultDescription
        {
            get { return (string)GetValue(ResultDescriptionProperty); }
            set { SetValue(ResultDescriptionProperty, value); }
        }

        public static readonly DependencyProperty ResultDescriptionProperty =
            DependencyProperty.Register("ResultDescription", typeof(string), typeof(MedResultAndDiagnos), new PropertyMetadata(null));

        // Свойство для привязки даты результатов обследования
        public string Date
        {
            get { return (string)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }

        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register("Date", typeof(string), typeof(MedResultAndDiagnos), new PropertyMetadata(null));

        // Свойство для привязки имени врача, установившего диагноз
        public string DiagnosingDoctorName
        {
            get { return (string)GetValue(DiagnosingDoctorNameProperty); }
            set { SetValue(DiagnosingDoctorNameProperty, value); }
        }

        public static readonly DependencyProperty DiagnosingDoctorNameProperty =
            DependencyProperty.Register("DiagnosingDoctorName", typeof(string), typeof(MedResultAndDiagnos), new PropertyMetadata(null));

        // Свойство для привязки названия диагноза
        public string DiagnosisName
        {
            get { return (string)GetValue(DiagnosisNameProperty); }
            set { SetValue(DiagnosisNameProperty, value); }
        }

        public static readonly DependencyProperty DiagnosisNameProperty =
            DependencyProperty.Register("DiagnosisName", typeof(string), typeof(MedResultAndDiagnos), new PropertyMetadata(null));

        // Свойство для привязки даты установки диагноза
        public string DiagnosisDate
        {
            get { return (string)GetValue(DiagnosisDateProperty); }
            set { SetValue(DiagnosisDateProperty, value); }
        }

        public static readonly DependencyProperty DiagnosisDateProperty =
            DependencyProperty.Register("DiagnosisDate", typeof(string), typeof(MedResultAndDiagnos), new PropertyMetadata(null));

        public MedResultAndDiagnos()
        {
            InitializeComponent();
        }
    }
}