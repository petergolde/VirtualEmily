using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualEmily
{
    public class Question
    {
        public TermStyle QuestionStyle;
        public TermStyle AnswerStyle;
        public string QuestionString;
        public string AnswerString;

        private List<bool> answers = new List<bool>();

        public Question()
        {
        }

        public QuestionStatus Status
        {
            get
            {
                int right = 0, wrong = 0;

                if (answers.Count == 0)
                    return QuestionStatus.NotSeen;

                // Check the last 7 answers.
                for (int i = Math.Max(0, answers.Count - 7); i < answers.Count; ++i) {
                    if (answers[i])
                        ++right;
                    else
                        ++wrong;
                }

                // To be considered that we fully know it:
                //  - Have seend at least 3 answers.
                //  - 3 or more right than wrong.
                //  - We got it right the last time.
                // We partially know if we got it at least as right as wrong.
                if (answers.Count >= 3 && right - wrong >= 3 && answers[answers.Count - 1])
                    return QuestionStatus.Right;
                else if (right >= wrong)
                    return QuestionStatus.Partial;
                else
                    return QuestionStatus.Wrong;
            }
        }

        internal string SaveAnswers()
        {
            StringBuilder builder = new StringBuilder();
            foreach (bool answer in answers) {
                builder.Append(answer ? "1" : "0");
            }
            return builder.ToString();
        }

        internal void LoadAnswers(string text)
        {
            answers.Clear();
            foreach (char c in text) {
                if (c == '1')
                    answers.Add(true);
                else if (c == '0')
                    answers.Add(false);
            }
        }

        internal void RecordAnswer(bool correct)
        {
            answers.Add(correct);
        }
    }

    public enum TermStyle { Text, Picture, Sound};

    public enum QuestionStatus { NotSeen, Wrong, Partial, Right }
}
