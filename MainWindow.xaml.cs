using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using NAudio.Wave;

namespace chatbotp3
{
    public partial class MainWindow : Window
    {
        private ChatbotLogic chatbot;

        public MainWindow()
        {
            InitializeComponent();
            chatbot = new ChatbotLogic();
            DisplayAsciiArt();
            DisplayAsciiComputer();
            PlayVoiceGreeting();
            AddChat("Bot", "Welcome to the Cybersecurity Awareness Chatbot!\nType a question or command.");
        }

        private void DisplayAsciiArt()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@" 
       
         CYBERSECURITY AWARENESS 
                 BOT             
        
        ");
            Console.ResetColor();
        }
        private void DisplayAsciiComputer()
        {
            Console.WriteLine(@"
      .__________________________.
      | .______________________. |
      | |      ^  -  ^         | |
      | |      (o)  (o)        | |
      | |         --           | |
      | |      \_______/       | |
      | |______________________| |
      |__________________________|
    
    ");
        }

        private void PlayVoiceGreeting()
        {
            string path = "greeting.wav";

            if (File.Exists(path))
            {
                using (var audioFile = new AudioFileReader(path))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            else
            {
                Console.WriteLine("Warning: Sound file not found. Skipping greeting sound...");
            }
        }

        private void AddChat(string sender, string message)
        {
            ChatList.Items.Add($"{sender}: {message}");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string input = UserInput.Text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            AddChat("You", input);
            string response = chatbot.GetResponse(input);
            AddChat("Bot", response);
            UserInput.Clear();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string task = TaskInput.Text.Trim();
            if (!string.IsNullOrEmpty(task))
            {
                TaskManager.AddTask(task);
                TaskList.Items.Add(task);
                AddChat("Bot", $"Task added: {task}");
                TaskInput.Clear();
            }
            else
            {
                AddChat("Bot", "Please enter a task.");
            }
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            string question = QuizManager.StartQuiz();
            QuizQuestion.Text = question;
            AddChat("Bot", "Quiz started!");
        }

        private void SubmitQuizAnswer_Click(object sender, RoutedEventArgs e)
        {
            string answer = QuizAnswer.Text.Trim();
            if (!string.IsNullOrEmpty(answer))
            {
                string result = QuizManager.CheckAnswer(answer);
                AddChat("Bot", result);
                QuizQuestion.Text = QuizManager.AskNext();
                QuizAnswer.Clear();
            }
        }
    }

    public class ChatbotLogic
    {
        private string userName = "";
        private string userInterest = "";
        private List<string> logs = new();
        private Random random = new();

        private Dictionary<string, List<string>> keywordResponses = new()
        {
            ["password"] = new List<string>
            {
                "Use strong passwords with symbols and numbers.",
                "Never reuse passwords across different accounts.",
                "Avoid using personal info in your passwords."
            },
            ["phishing"] = new List<string>
            {
                "Be cautious with suspicious emails and links.",
                "Don’t click unknown attachments.",
                "Report suspicious emails to IT/security."
            },
            ["privacy"] = new List<string>
            {
                "Adjust your social media privacy settings regularly.",
                "Be aware of what permissions your apps ask for.",
                "Use incognito mode when needed."
            }
        };

        public string GetResponse(string input)
        {
            string lower = input.ToLower();

            if (lower.Contains("name is "))
            {
                userName = input.Substring(input.IndexOf("name is ") + 8);
                logs.Add($"Stored name: {userName}");
                return $"Nice to meet you, {userName}!";
            }

            if (lower.Contains("interested in "))
            {
                userInterest = input.Substring(input.IndexOf("interested in ") + 14);
                logs.Add($"Stored interest: {userInterest}");
                return $"Great! I’ll remember that you're interested in {userInterest}.";
            }

            if (lower.Contains("how are you")) return "I'm functioning well and secured!";
            if (lower.Contains("purpose")) return "I'm here to help you stay safe online.";
            if (lower.Contains("what can i ask")) return "Ask me about phishing, password safety, scams, or privacy tips.";

            foreach (var keyword in keywordResponses.Keys)
            {
                if (lower.Contains(keyword))
                {
                    logs.Add($"Responded to keyword: {keyword}");
                    var responses = keywordResponses[keyword];
                    return responses[random.Next(responses.Count)];
                }
            }

            if (lower.Contains("worried") || lower.Contains("nervous"))
                return "I understand your concern. Cyber threats are real but manageable. Let's secure your data.";

            if (lower.Contains("activity log"))
                return string.Join("\n", logs.TakeLast(5));

            if (lower.Contains("quiz"))
                return QuizManager.StartQuiz();

            if (lower.Contains("task"))
                return "Use the task panel to manage cybersecurity to-dos!";

            return "I didn’t quite catch that. Could you rephrase?";
        }
    }

    public static class TaskManager
    {
        private static List<string> tasks = new();

        public static void AddTask(string task)
        {
            tasks.Add(task);
        }

        public static List<string> GetTasks()
        {
            return tasks;
        }
    }

    public static class QuizManager
    {
        private static int score = 0;
        private static int current = 0;

        private static List<(string, string)> questions = new()
        {
            ("What should you do if you receive a suspicious email?\nA) Click it\nB) Ignore it\nC) Report it", "C"),
            ("True or False: Password123 is a secure password.", "False"),
            ("Which is safest?\nA) Free WiFi\nB) VPN\nC) Public USB chargers", "B")
        };

        public static string StartQuiz()
        {
            score = 0;
            current = 0;
            return AskNext();
        }

        public static string AskNext()
        {
            if (current < questions.Count)
                return questions[current++].Item1;
            return $"Quiz complete! Final score: {score}/{questions.Count}";
        }

        public static string CheckAnswer(string userAnswer)
        {
            string correct = questions[current - 1].Item2;
            if (userAnswer.Equals(correct, StringComparison.OrdinalIgnoreCase))
            {
                score++;
                return "Correct!";
            }
            return $"Incorrect. Correct answer: {correct}";
        }
    }
}
