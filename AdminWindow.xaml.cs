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
using Microsoft.Win32;
using Path = System.IO.Path;

namespace VirtualEmily
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow: Window
    {
        Quiz quiz;
        string directory;
        string videoFileName;
        int questionNumber = -1;


        public AdminWindow(string directory)
        {
            InitializeComponent();
            this.quiz = new Quiz();
            this.directory = directory;
            quiz.LoadQuestions(directory);

            comboQuestionNum.Items.Clear();
            for (int i = 0; i < quiz.QuestionCount; ++i) {
                comboQuestionNum.Items.Add(i.ToString());
            }

            questionNumber = -1;
            if (quiz.QuestionCount > 0) {
                comboQuestionNum.SelectedIndex = 0;
            }
            else {
                AddNewQuestion();
            }
        }


        void UpdateQuestion()
        {
            if (questionNumber >= 0) {
                QuestionData qd = new QuestionData();
                qd.Id = textBoxId.Text;
                qd.ChineseImageFileName = textBoxChineseImageFileName.Text;
                qd.ChineseText = textBoxChineseText.Text;
                qd.EnglishText = textBoxEnglishText.Text;
                qd.SoundFileName = textBoxSoundFileName.Text;

                quiz.ReplaceQuestion(questionNumber, qd);
            }
        }

        void LoadQuestionData(QuestionData qd)
        {
            textBoxId.Text = qd.Id;
            textBoxChineseImageFileName.Text = qd.ChineseImageFileName;
            textBoxChineseText.Text = qd.ChineseText;
            textBoxEnglishText.Text = qd.EnglishText;
            textBoxSoundFileName.Text = qd.SoundFileName;
        }



        private void startChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (startTime.Value.HasValue) {
                TimeSpan start = TimeSpan.FromSeconds(startTime.Value.Value);
                mediaElement.Position = start;
                //Util.PlayMedia(mediaElement, start, TimeSpan.FromMilliseconds(50), true);
            }
        }

        private void saveButtonClicked(object sender, RoutedEventArgs e)
        {
            UpdateQuestion();
            quiz.SaveQuestions();
        }



        private void comboQuestionNumSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateQuestion();
            questionNumber = int.Parse((string)comboQuestionNum.SelectedItem);
            LoadQuestionData(quiz.GetQuestionData(questionNumber));
        }

        private void LoadVideoClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true) {
                videoFileName = dialog.FileName;
                mediaElement.Source = new Uri(videoFileName);
            }
        }

        private void TestButtonClicked(object sender, RoutedEventArgs e)
        {
            if (startTime.Value.HasValue && lengthTime.Value.HasValue) {
                TimeSpan start = TimeSpan.FromSeconds(startTime.Value.Value);
                TimeSpan duration = TimeSpan.FromSeconds(lengthTime.Value.Value);
                Util.PlayMedia(mediaElement, start, duration, false);
            }
        }



        private void TakeFromVideoClicked(object sender, RoutedEventArgs e)
        {
            TimeSpan start = TimeSpan.FromSeconds(startTime.Value.Value);
            TimeSpan duration = TimeSpan.FromSeconds(lengthTime.Value.Value);
            string fileName = textBoxId.Text + ".wav";
            string cmdLineArgs = string.Format("-ss {0} -t {1} -i \"{2}\" \"{3}\"", start.TotalSeconds, duration.TotalSeconds, videoFileName, Path.Combine(directory, fileName));
            Process process = Process.Start("ffmpeg.exe", cmdLineArgs);
            process.WaitForExit();
            textBoxSoundFileName.Text = fileName;
        }

        void AddNewQuestion()
        {
            ++questionNumber;

            comboQuestionNum.Items.Add(questionNumber.ToString());
            comboQuestionNum.SelectedIndex = questionNumber;

            textBoxId.Text = "q" + questionNumber.ToString();
            textBoxChineseImageFileName.Text = "";
            textBoxChineseText.Text = "";
            textBoxEnglishText.Text = "";
            textBoxSoundFileName.Text = "";
        }

        private void buttonAddNewQuestion_Click(object sender, RoutedEventArgs e)
        {
            UpdateQuestion();
            AddNewQuestion();
        }
    }
}
