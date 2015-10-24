using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEmily
{
    public class Quiz
    {
        List<Question> questions = new List<Question>();
        Random random = new Random();

        public void AddQuestion(TimeSpan start, TimeSpan duration)
        {
            questions.Add(new Question(start, duration));
        }

        public void SaveQuestions(string fileName)
        {
            List<string> output = new List<string>();
            foreach (Question question in questions) {
                output.Add(string.Format("{0},{1}", question.StartTime.TotalSeconds, question.Duration.TotalSeconds));
            }

            File.WriteAllLines(fileName, output);
        }

        public void LoadQuestions(string fileName)
        {
            if (File.Exists(fileName)) {
                string[] input = File.ReadAllLines(fileName);
                foreach (string line in input) {
                    string[] fields = line.Split(',');
                    if (fields.Length >= 2) {
                        AddQuestion(TimeSpan.FromSeconds(double.Parse(fields[0])), TimeSpan.FromSeconds(double.Parse(fields[1])));
                    }
                }
            }
        }

        public void SaveAnswers(string fileName)
        {
            List<string> output = new List<string>();

            foreach (Question q in questions) {
                output.Add(q.SaveAnswers());
            }

            File.WriteAllLines(fileName, output);
        }

        public void LoadAnswers(string fileName)
        {
            if (File.Exists(fileName)) {
                string[] input = File.ReadAllLines(fileName);
                for (int i = 0; i < questions.Count; ++i) {
                    if (i < input.Length) {
                        questions[i].LoadAnswers(input[i]);
                    }
                }
            }
        }

        public QuizStats CalcStats()
        {
            QuizStats stats = new QuizStats();

            foreach (Question q in questions) {
                QuestionStatus status = q.Status;
                switch (status) {
                    case QuestionStatus.NotSeen: stats.CountUnseen++; break;
                    case QuestionStatus.Wrong: stats.CountWrong++; break;
                    case QuestionStatus.Partial: stats.CountPartial++; break;
                    case QuestionStatus.Right: stats.CountRight++; break;
                }
            }

            stats.Total = questions.Count;
            return stats;
        }

        public Question NextQuestion()
        {
            QuizStats stats = CalcStats();

            QuestionStatus targetStatus;
            int targetIndex;

            if (stats.CountRight > 0 && random.NextDouble() < 0.3) {
                targetStatus = QuestionStatus.Right;
                targetIndex = random.Next(stats.CountRight);
            }
            else if (stats.CountWrong == 0 && stats.CountPartial <= 3 && stats.CountUnseen > 0) {
                targetStatus = QuestionStatus.NotSeen;
                targetIndex = random.Next(stats.CountUnseen);
            }
            else if (random.Next(2 * stats.CountWrong + stats.CountPartial) < 2 * stats.CountWrong) {
                targetStatus = QuestionStatus.Wrong;
                targetIndex = random.Next(stats.CountWrong);
            }
            else {
                targetStatus = QuestionStatus.Partial;
                targetIndex = random.Next(stats.CountPartial);
            }

            foreach (Question q in questions) {
                QuestionStatus status = q.Status;
                if (status == targetStatus) {
                    if (targetIndex == 0)
                        return q;
                    else
                        --targetIndex;
                }
            }

            return questions[random.Next(questions.Count)];
        }

        public void RecordAnswer(Question question, bool correct)
        {
            question.RecordAnswer(correct);
        }
    }

    public class QuizStats
    {
        public int CountUnseen = 0, CountWrong = 0, CountPartial = 0, CountRight = 0;
        public int Total = 0;
    }
}
