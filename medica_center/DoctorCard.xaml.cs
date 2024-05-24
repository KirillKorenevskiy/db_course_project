using System.Windows;
using System.Windows.Controls;

namespace medica_center
{
    public partial class DoctorCard : UserControl
    {
        public static readonly DependencyProperty FirstNameProperty =
            DependencyProperty.Register("FirstName", typeof(string), typeof(DoctorCard));

        public static readonly DependencyProperty LastNameProperty =
            DependencyProperty.Register("LastName", typeof(string), typeof(DoctorCard));

        public static readonly DependencyProperty SpecializationProperty =
            DependencyProperty.Register("Specialization", typeof(string), typeof(DoctorCard));

        public static readonly DependencyProperty ScheduleProperty =
            DependencyProperty.Register("Schedule", typeof(string), typeof(DoctorCard));

        public string FirstName
        {
            get { return (string)GetValue(FirstNameProperty); }
            set { SetValue(FirstNameProperty, value); }
        }

        public string LastName
        {
            get { return (string)GetValue(LastNameProperty); }
            set { SetValue(LastNameProperty, value); }
        }

        public string Specialization
        {
            get { return (string)GetValue(SpecializationProperty); }
            set { SetValue(SpecializationProperty, value); }
        }

        public string Schedule
        {
            get { return (string)GetValue(ScheduleProperty); }
            set { SetValue(ScheduleProperty, value); }
        }

        public DoctorCard()
        {
            InitializeComponent();
        }
    }
}