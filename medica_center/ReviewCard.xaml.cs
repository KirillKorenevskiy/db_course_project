using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace medica_center
{
    /// <summary>
    /// Логика взаимодействия для ReviewCard.xaml
    /// </summary>
    public partial class ReviewCard : UserControl
    {
        public ReviewCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NicknameProperty =
        DependencyProperty.Register("Nickname", typeof(string), typeof(ReviewCard));

        public static readonly DependencyProperty ReviewTextProperty =
            DependencyProperty.Register("ReviewText", typeof(string), typeof(ReviewCard));

        public static readonly DependencyProperty ReviewDateProperty =
            DependencyProperty.Register("ReviewDate", typeof(string), typeof(ReviewCard));

        public string Nickname
        {
            get { return (string)GetValue(NicknameProperty); }
            set { SetValue(NicknameProperty, value); }
        }

        public string ReviewText
        {
            get { return (string)GetValue(ReviewTextProperty); }
            set { SetValue(ReviewTextProperty, value); }
        }

        public string ReviewDate
        {
            get { return (string)GetValue(ReviewDateProperty); }
            set { SetValue(ReviewDateProperty, value); }
        }
    }
}
