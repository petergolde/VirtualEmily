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
using System.Windows.Threading;

namespace VirtualEmily
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window
    {
        Quiz quiz;
        string fileName = "questionData.txt";
        string answerData = "answerData.txt";
        Question question;
        bool dontKnow = false;

        public MainWindow()
        {
            quiz = new Quiz();

            InitializeComponent();

            wrongButton.Visibility = Visibility.Hidden;
            correctButton.Visibility = Visibility.Hidden;
            checkButton.Visibility = Visibility.Hidden;
            dontKnowButton.Visibility = Visibility.Hidden;
            continueButton.Visibility = Visibility.Hidden;

        }

        void PresentNextQuestion()
        {
            quiz.SaveAnswers(answerData);

            QuizStats stats = quiz.CalcStats();
            UpdateStatusGrid(stats);

            // statusLabel.Content = string.Format("{0} of {1} known", stats.CountRight, stats.Total);

            question = quiz.NextQuestion();
            wrongButton.Visibility = Visibility.Hidden;
            correctButton.Visibility = Visibility.Hidden;
            checkButton.Visibility = Visibility.Visible;
            dontKnowButton.Visibility = Visibility.Visible;
            continueButton.Visibility = Visibility.Hidden;
            dontKnow = false;

            Util.PlayMedia(mediaElement, question.StartTime, question.Duration, true);
        }

        private void UpdateStatusGrid(QuizStats stats)
        {
            statusGrid.ColumnDefinitions[0].Width = new GridLength(stats.CountUnseen, GridUnitType.Star);
            statusGrid.ColumnDefinitions[1].Width = new GridLength(stats.CountWrong, GridUnitType.Star);
            statusGrid.ColumnDefinitions[2].Width = new GridLength(stats.CountPartial, GridUnitType.Star);
            statusGrid.ColumnDefinitions[3].Width = new GridLength(stats.CountRight, GridUnitType.Star);
        }

        private void administerButtonClicked(object sender, RoutedEventArgs e)
        {
            new AdminWindow(quiz, fileName).ShowDialog();
        }

        private void wrongButtonClicked(object sender, RoutedEventArgs e)
        {
            quiz.RecordAnswer(question, false);
            PresentNextQuestion();
        }

        private void correctButtonClicked(object sender, RoutedEventArgs e)
        {
            quiz.RecordAnswer(question, true);
            PresentNextQuestion();
        }

        private void checkAnswerClicked(object sender, RoutedEventArgs e)
        {
            if (!dontKnow) {
                wrongButton.Visibility = Visibility.Visible;
                correctButton.Visibility = Visibility.Visible;
                dontKnowButton.Visibility = Visibility.Hidden;
            }

            Util.PlayMedia(mediaElement, question.StartTime, question.Duration, false);
        }

        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            quiz.LoadQuestions(fileName);
            quiz.LoadAnswers(answerData);
        }

        private void windowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            quiz.SaveAnswers(answerData);
        }

        private void mediaLoaded(object sender, RoutedEventArgs e)
        {
            Util.PlayMedia(mediaElement, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(20), true);
        }

        private void startButtonClicked(object sender, RoutedEventArgs e)
        {
            startButton.Visibility = Visibility.Hidden;
            buttonAdminister.Visibility = Visibility.Hidden;

            PresentNextQuestion();
        }

        private void continueButtonClicked(object sender, RoutedEventArgs e)
        {
            quiz.RecordAnswer(question, false);
            PresentNextQuestion();
        }

        private void dontKnowButtonClicked(object sender, RoutedEventArgs e)
        {
            Util.PlayMedia(mediaElement, question.StartTime, question.Duration, false);
            dontKnowButton.Visibility = Visibility.Hidden;
            continueButton.Visibility = Visibility.Visible;
            dontKnow = true;
        }
    }
}
