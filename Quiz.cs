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
        string quizTitle = "";
        string directory;
        List<QuestionData> questionData = new List<QuestionData>();
        List<Question> questions = new List<Question>();
        Random random = new Random();
        Question lastQuestion;

        public TermKind FromTerm { get; private set; }
        public TermKind ToTerm { get; private set; }

        public int QuestionCount
        {
            get { return questionData.Count;  }
        }

        public Question GetQuestion(int index)
        {
            return questions[index];
        }

        public QuestionData GetQuestionData(int index)
        {
            return questionData[index];
        }

        public void AddQuestion(QuestionData qd)
        {
            questionData.Add(qd);
        }

        public void ReplaceQuestion(int index, QuestionData qd)
        {
            if (index == questionData.Count)
                questionData.Add(qd);
            else
                questionData[index] = qd;
        }

        private string QuestionDataFileName
        {
            get { return Path.Combine(directory, "questionData.txt"); }
        }

        private string AnswerDataFileName
        {
            get { return Path.Combine(directory, string.Format("{0}2{1}_answers.txt", TermText(FromTerm), TermText(ToTerm))); }
        }

        private string TermText(TermKind termKind)
        {
            switch (termKind) {
                case TermKind.ChineseCharacter:
                    return "chin";
                case TermKind.ChineseSound:
                    return "sound";
                case TermKind.EnglishWord:
                default:
                    return "eng";
            }
        }


        public void SaveQuestions()
        {
            List<string> output = new List<string>();
            output.Add(quizTitle);
            foreach (QuestionData qd in questionData) {
                output.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", qd.Id, qd.ChineseImageFileName, qd.ChineseText, qd.EnglishText, qd.SoundFileName));
            }

            File.WriteAllLines(QuestionDataFileName, output, new UTF8Encoding(true));
        }

        public void LoadQuestions(string directoryName)
        {
            directory = directoryName;

            questionData.Clear();
            questions.Clear();

            if (File.Exists(QuestionDataFileName)) {
                string[] input = File.ReadAllLines(QuestionDataFileName, new UTF8Encoding(true));
                quizTitle = input[0];
                for (int i = 1; i < input.Length; ++i) {
                    string line = input[i];
                    string[] fields = line.Split('\t');
                    if (fields.Length >= 5) {
                        QuestionData qd = new QuestionData();
                        qd.Id = fields[0];
                        qd.ChineseImageFileName = fields[1];
                        qd.ChineseText = fields[2];
                        qd.EnglishText = fields[3];
                        qd.SoundFileName = fields[4];
                        questionData.Add(qd);
                    }
                }
            }
            else {
                quizTitle = Path.GetFileName(directoryName);
            }
        }

        public void SaveAnswers()
        {
            List<string> output = new List<string>();

            foreach (Question q in questions) {
                output.Add(q.SaveAnswers());
            }

            File.WriteAllLines(AnswerDataFileName, output);
        }

        void GetQuestionAnswerData(TermKind termKind, QuestionData qd, out TermStyle termStyle, out string termString)
        {
            if (termKind == TermKind.ChineseCharacter) {
                if (string.IsNullOrWhiteSpace(qd.ChineseImageFileName)) {
                    termStyle = TermStyle.Text;
                    termString = qd.ChineseText;
                }
                else {
                    termStyle = TermStyle.Picture;
                    termString = Path.Combine(directory, qd.ChineseImageFileName);
                }
            }
            else if (termKind == TermKind.ChineseSound) {
                termStyle = TermStyle.Sound;
                termString = Path.Combine(directory, qd.SoundFileName);
            }
            else if (termKind == TermKind.EnglishWord) {
                termStyle = TermStyle.Text;
                termString = qd.EnglishText;
            }
            else {
                throw new ArgumentException("termKind");
            }
        }

        public void LoadAnswers(TermKind from, TermKind to)
        {
            FromTerm = from;
            ToTerm = to;

            questions.Clear();

            if (File.Exists(AnswerDataFileName)) {
                string[] input = File.ReadAllLines(AnswerDataFileName);
                for (int i = 0; i < questionData.Count; ++i) {
                    QuestionData qd = questionData[i];
                    Question q = new Question();

                    GetQuestionAnswerData(from, qd, out q.QuestionStyle, out q.QuestionString);
                    GetQuestionAnswerData(to, qd, out q.AnswerStyle, out q.AnswerString);

                    if (i < input.Length) {
                        q.LoadAnswers(input[i]);
                    }

                    questions.Add(q);
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
            Question newQuestion = null;
            QuestionStatus targetStatus;
            int targetIndex;
            int count = 20;

            do {

                if (stats.CountRight > 0 && random.NextDouble() < 0.3) {
                    // 30% chance to review one we know.
                    targetStatus = QuestionStatus.Right;
                    targetIndex = random.Next(stats.CountRight);
                }
                else if (stats.CountWrong + stats.CountPartial < 3 && stats.CountUnseen > 0 && random.NextDouble() < 0.5) {
                    // If we have < 3 that we don't know, add a new one in.
                    targetStatus = QuestionStatus.NotSeen;
                    targetIndex = random.Next(stats.CountUnseen);
                }
                else if (random.Next(2 * stats.CountWrong + stats.CountPartial) < 2 * stats.CountWrong) {
                    // Choose a question we don't know yet, with wrong weighted twice as partial.
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
                        if (targetIndex == 0) {
                            newQuestion = q;
                            break;
                        }
                        else {
                            --targetIndex;
                        }
                    }
                }

                if (newQuestion == null)
                    newQuestion = questions[random.Next(questions.Count)];
            } while (newQuestion == lastQuestion && count-- > 0);  // Don't repeat the same question in a row unless we must.

            lastQuestion = newQuestion;
            return newQuestion;
        }

        public void RecordAnswer(Question question, bool correct)
        {
            question.RecordAnswer(correct);
        }
    }

    public enum TermKind { EnglishWord, ChineseCharacter, ChineseSound }

    public class QuizStats
    {
        public int CountUnseen = 0, CountWrong = 0, CountPartial = 0, CountRight = 0;
        public int Total = 0;
    }
}
