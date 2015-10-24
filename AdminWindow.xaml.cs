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
using System.Windows.Shapes;

namespace VirtualEmily
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow: Window
    {
        Quiz quiz;
        string fileName;

        public AdminWindow(Quiz quiz, string filename)
        {
            InitializeComponent();
            this.quiz = quiz;
            this.fileName = filename;
        }

        private void testButtonClicked(object sender, RoutedEventArgs e)
        {
            if (startTime.Value.HasValue && lengthTime.Value.HasValue) {
                TimeSpan start = TimeSpan.FromSeconds(startTime.Value.Value);
                TimeSpan duration = TimeSpan.FromSeconds(lengthTime.Value.Value);
                Util.PlayMedia(mediaElement, start, duration, false);
            }
        }

        private void startChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (startTime.Value.HasValue) {
                TimeSpan start = TimeSpan.FromSeconds(startTime.Value.Value);
                Util.PlayMedia(mediaElement, start, TimeSpan.FromMilliseconds(50), true);
            }
        }

        private void saveButtonClicked(object sender, RoutedEventArgs e)
        {
            quiz.SaveQuestions(fileName);
        }

        private void addButtonClicked(object sender, RoutedEventArgs e)
        {
            if (startTime.Value.HasValue && lengthTime.Value.HasValue) {
                TimeSpan start = TimeSpan.FromSeconds(startTime.Value.Value);
                TimeSpan duration = TimeSpan.FromSeconds(lengthTime.Value.Value);
                quiz.AddQuestion(start, duration);
            }
        }
    }
}
