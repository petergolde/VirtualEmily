using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

using Path = System.IO.Path;

namespace VirtualEmily
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window
    {
        Quiz quiz;
        string rootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Chinese Quizzes";
        TimeSpan timeLimit;
        DateTime finishTime;
        Question question;
        DispatcherTimer timer = new DispatcherTimer();
        bool dontKnow = false;

        public MainWindow()
        {
            quiz = new Quiz();

            InitializeComponent();

            quizGrid.Visibility = Visibility.Hidden;
            doneGrid.Visibility = Visibility.Hidden;
            initialGrid.Visibility = Visibility.Visible;

            wrongButton.Visibility = Visibility.Hidden;
            correctButton.Visibility = Visibility.Hidden;
            checkButton.Visibility = Visibility.Hidden;
            dontKnowButton.Visibility = Visibility.Hidden;
            continueButton.Visibility = Visibility.Hidden;

            buttonAdminister.Visibility = Visibility.Visible;

            FindAllQuizzes();

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Start();
        }

        void FindAllQuizzes()
        {
            foreach (string dir in Directory.EnumerateDirectories(rootDirectory)) {
                string dirName = Path.GetFileName(dir);
                comboBoxQuizNames.Items.Add(dirName);
            }

            comboBoxQuizNames.SelectedIndex = 0;
        }

        string SelectedQuizDirectory()
        {
            string dirName = (string)comboBoxQuizNames.SelectedItem;
            return Path.Combine(rootDirectory, dirName);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (timeLimit != default(TimeSpan)) {
                TimeSpan timeLeft = finishTime - DateTime.Now;
                timeLeftLabel.Content = timeLeft < default(TimeSpan) ? "Done" : timeLeft.ToString(@"m\:ss");
            }
        }

        void PresentNextQuestion()
        {
            if (timeLimit != default(TimeSpan) && DateTime.Now > finishTime) {
                Done();
                return;
            }

            quiz.SaveAnswers();

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

            ShowQuestion();
        }

        void ShowQuestion()
        {
            if (question.QuestionStyle == TermStyle.Picture) {
                string path = question.QuestionString;
                imageDisplay.Source =  new BitmapImage(new Uri(path));
                imageDisplay.Visibility = Visibility.Visible;
                textDisplay.Visibility = Visibility.Hidden;
                buttonReplaySound.Visibility = Visibility.Hidden;
            }
            else if (question.QuestionStyle == TermStyle.Text) {
                textDisplay.Content = question.QuestionString;
                imageDisplay.Visibility = Visibility.Hidden;
                textDisplay.Visibility = Visibility.Visible;
                buttonReplaySound.Visibility = Visibility.Hidden;
            }
            else if (question.QuestionStyle == TermStyle.Sound) {
                string path = question.QuestionString;
                imageDisplay.Visibility = Visibility.Hidden;
                textDisplay.Visibility = Visibility.Hidden;
                buttonReplaySound.Visibility = Visibility.Visible;

                SoundPlayer soundPlayer = new SoundPlayer(path);
                soundPlayer.Play();
            }
        }

        private void ReplaySoundClicked(object sender, RoutedEventArgs e)
        {
            string path = question.QuestionString;

            SoundPlayer soundPlayer = new SoundPlayer(path);
            soundPlayer.Play();
        }

        void Done()
        {
            quizGrid.Visibility = Visibility.Hidden;
            doneGrid.Visibility = Visibility.Visible;
            QuizStats stats = quiz.CalcStats();
            resultsText.Text = string.Format("Known: {0}\r\nPartially Known: {1}\r\nNot known: {2}\r\nNot seen: {3}", stats.CountRight, stats.CountPartial, stats.CountWrong, stats.CountUnseen);

        }

        void ShowAnswer()
        {
            if (question.AnswerStyle == TermStyle.Sound) {
                string path = question.AnswerString;
                SoundPlayer soundPlayer = new SoundPlayer(path);
                soundPlayer.Play();
            }
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
            new AdminWindow(SelectedQuizDirectory()).ShowDialog();
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

            ShowAnswer();
        }

        private void windowLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void windowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            quiz.SaveAnswers();
        }


        private void startButtonClicked(object sender, RoutedEventArgs e)
        {
            TermKind from, to;
            if (comboBoxQuizType.SelectedIndex == 0) {
                from = TermKind.ChineseSound; to = TermKind.EnglishWord;
            }
            else if (comboBoxQuizType.SelectedIndex == 1) {
                from = TermKind.EnglishWord; to = TermKind.ChineseSound;
            }
            else if (comboBoxQuizType.SelectedIndex == 2) {
                from = TermKind.ChineseCharacter; to = TermKind.ChineseSound;
            }
            else if (comboBoxQuizType.SelectedIndex == 3) {
                from = TermKind.ChineseCharacter; to = TermKind.EnglishWord;
            }
            else {
                MessageBox.Show("Unsupported type of quiz!");
                return;
            }

            initialGrid.Visibility = Visibility.Hidden;
            quizGrid.Visibility = Visibility.Visible;

            quiz.LoadQuestions(SelectedQuizDirectory());

            quiz.LoadAnswers(from, to);

            if (timeLimitSetting.Value.HasValue && timeLimitSetting.Value.Value > 0) {
                timeLimit = TimeSpan.FromMinutes(timeLimitSetting.Value.Value);
                finishTime = DateTime.Now + timeLimit;
            }
            else {
                timeLimit = default(TimeSpan);
            }

            PresentNextQuestion();
        }

        private void continueButtonClicked(object sender, RoutedEventArgs e)
        {
            quiz.RecordAnswer(question, false);
            PresentNextQuestion();
        }

        private void dontKnowButtonClicked(object sender, RoutedEventArgs e)
        {
            ShowAnswer();

            dontKnowButton.Visibility = Visibility.Hidden;
            continueButton.Visibility = Visibility.Visible;
            dontKnow = true;
        }

    }
}
