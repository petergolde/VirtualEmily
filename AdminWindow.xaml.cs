using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
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
        string videoFileName = "radicals2.wmv";

        public AdminWindow(Quiz quiz, string filename)
        {
            InitializeComponent();
            this.quiz = quiz;
            this.fileName = filename;

            comboQuestionNum.Items.Clear();
            for (int i = 0; i < quiz.QuestionCount; ++i) {
                comboQuestionNum.Items.Add(i.ToString());
            }

            comboQuestionNum.SelectedIndex = 0;
        }

        public int QuestionNumber
        {
            get { return int.Parse((string) comboQuestionNum.SelectedItem); }
        }

        private void testButtonClicked(object sender, RoutedEventArgs e)
        {
            if (startTime.Value.HasValue && lengthTime.Value.HasValue) {
                TimeSpan start = TimeSpan.FromSeconds(startTime.Value.Value);
                TimeSpan duration = TimeSpan.FromSeconds(lengthTime.Value.Value);
                Util.PlayMedia(mediaElement, start, duration, false);
            }
        }


        private void buttonUpdateClicked(object sender, RoutedEventArgs e)
        {
            if (startTime.Value.HasValue && lengthTime.Value.HasValue) {
                TimeSpan start = TimeSpan.FromSeconds(startTime.Value.Value);
                TimeSpan duration = TimeSpan.FromSeconds(lengthTime.Value.Value);
                string soundFile = CreateSoundFile(QuestionNumber, start, duration);
                SoundPlayer soundPlayer = new SoundPlayer(soundFile);
                soundPlayer.Play();
                quiz.ReplaceQuestion(QuestionNumber, textBoxPicture.Text, soundFile);
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

        private string CreateSoundFile(int questionNumber, TimeSpan start, TimeSpan duration)
        {
            string cmdLineArgs = string.Format("-ss {0} -t {1} -i {2} sound{3}.wav", start.TotalSeconds, duration.TotalSeconds, videoFileName, questionNumber);
            Process process = Process.Start("ffmpeg.exe", cmdLineArgs);
            process.WaitForExit();
            return String.Format("sound{0}.wav", questionNumber);
        }

        private void comboQuestionNumSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Question question = quiz.GetQuestion(QuestionNumber);
            startTime.Value = question.StartTime.TotalSeconds;
            lengthTime.Value = question.Duration.TotalSeconds;
        }
    }
}
