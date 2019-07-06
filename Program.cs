using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalBuzzFeed
{

    //SHARE RESULTS
    //QuizMaker
    //CLEAR CACHE
    //Create UserName
    //Authenticate?
    //Pictures?
    class Buzzfeed
    {
        static void Main(string[] args) {
            SqlConnection c = connection; bool keepGoing = true;
            
            Console.ReadLine();
            keepGoing = false;
            c.Open();
            while (keepGoing) {
                showMenu(c);
                string[] finalResult = calculateResult(c);//What if they make a quiz?
                displayResults(finalResult);
                keepGoing = Continue();
                clearCache();
            } c.Close();
        }


        static public SqlConnection connection = new SqlConnection(@"
                                    Data Source=(LocalDB)\MSSQLLocalDB;
                                    AttachDbFilename=C:\Code\Projects\BuzzFeedPersonal\Database1.mdf;
                                    Integrated Security=  True");
        static internal int quizId;
        static internal int QuestionId;
        static internal int Score;
        static internal int totalAnswers;
        static internal int totalQuestions;
        static internal string UserInput;
        static internal string userName;
        static internal string password;
        static bool account;
        static internal List<Dictionary<string, string>> questions = new List<Dictionary<string, string>>();
        static internal List<Dictionary<string, string>> answers = new List<Dictionary<string, string>>();
        static internal List<string[]> results = new List<string[]>();
        static internal bool clearCache() {
            quizId = 0;
            QuestionId = 0;
            Score = 0;
            totalAnswers = 0;
            totalQuestions = 0;
            UserInput = "";
            questions = null;
            answers = null;
            Console.Clear();
            return false;
        }
        static internal void error() {
            Console.Clear();
            Console.WriteLine("Invalid Entry, Please Try Again\nPress[ENTER]To Continue...");
            Console.ReadLine();
        }
        static bool checkChoice(string holder) {
            bool keepGoing = true;
            for (int i = 0; i < holder.Split().Length; i++) {
                if (UserInput.Split()[0] == holder.Split()[i]) UserInput = UserInput.Split()[0]; keepGoing = false;
            }
            if (keepGoing == false) return false;
            else return true;
        }
        static bool recordScore(int start) {
            if (Int32.TryParse(UserInput, out int choice)) {
                for (int i = start; i <= totalAnswers + start; i++) {
                    if (choice == Convert.ToInt16(answers[i]["AnswerNumber"])) {
                        choice = Convert.ToInt16(answers[i]["Score"]);
                        Score += choice;
                        return false;
                    }
                }
            }
            else error(); return true;
        }
        static bool Continue() {
            bool keepGoing = true; bool result = true;
            while (keepGoing) {
                Console.Clear();
                Console.WriteLine("Would You Like To Take Or Make A Quiz?\n[Y/N]");
                UserInput = Console.ReadLine().ToUpper();
                if (UserInput == "Y") { keepGoing = false; result = true; }
                else if (UserInput == "N") { keepGoing = false; result = false; }
                else error();
            }
            return result;
        }
        static void showMenu(SqlConnection c) {
            bool keepGoing = true;
            while (keepGoing) {
                Console.Clear();
                Console.WriteLine("Welcome To PopQuiz!");
                Console.WriteLine("Would You Like To \n[A]Take A Quiz\t[B]Make A Quiz\t[Q]Quit");
                switch (UserInput = Console.ReadLine().ToUpper()) {
                    case "A":
                        keepGoing = selectQuiz(c); break;
                    case "B":
                        //makeQuiz();   
                        break;
                    case "Q":
                        keepGoing = clearCache(); break;
                    default:
                        error(); break;
                }
            }
        }
        static bool selectQuiz(SqlConnection c) {
            bool keepGoing = true;
            while (keepGoing)
            {
                Console.Clear();
                Console.WriteLine("Would You Like To \n[A]View Quizzes By Category\t[B]View All Quizzes\t[C]Go Back To Last Page");
                switch (UserInput = Console.ReadLine().ToUpper())
                {
                    case "A":
                        keepGoing = showQuizzesByCategory(c);
                        break;
                    case "B":
                        keepGoing = showAllQuizzes(c); break;
                    case "C":
                        keepGoing = false; break;
                    default:
                        error(); break;
                }
            }
            return false;
        }
        static internal int selectCategory(SqlConnection c) {
            bool keepGoing = true;
            SqlCommand cmd = new SqlCommand("SELECT * FROM Category", c);
            while (keepGoing) {
                SqlDataReader reader = cmd.ExecuteReader(); string holder = "";
                Console.Clear();
                while (reader.Read()) {
                    Console.WriteLine($"[{reader["id"]}]{reader["Type"]}\n");
                    holder += (reader["id"].ToString() + " ");
                } reader.Close();
                Console.WriteLine("\nSelect A[CATEGORY]");
                UserInput = Console.ReadLine();
                keepGoing = checkChoice(holder);
                if (keepGoing) error();
            }
            return Convert.ToInt16(UserInput);
        }
        static internal bool showQuizzesByCategory(SqlConnection c) {
            int categoryId = selectCategory(c); bool keepGoing = true;
            SqlCommand cmd = new SqlCommand($"SELECT * FROM Quizzes WHERE CategoryId = {categoryId}", c);
            while (keepGoing) {
                SqlDataReader reader = cmd.ExecuteReader(); Console.Clear(); string holder = "";
                while (reader.Read()) {
                    Console.WriteLine($"[{reader["Id"]}]{reader["Text"]}\n\t({reader["subtext"]})\n");
                    holder += (reader["id"].ToString() + " ");
                } reader.Close();
                Console.WriteLine("\nSelect A[QUIZ]\tOr Enter[Q] At AnyTime To Quit");
                UserInput = Console.ReadLine();
                keepGoing = checkChoice(holder);
                if (UserInput.ToUpper() == "Q") keepGoing = false;
                if (keepGoing) error();
                else quizId = Convert.ToInt16(UserInput); takeQuiz(c);
            }
            return false;
        }
        static internal bool showAllQuizzes(SqlConnection c) {
            SqlCommand cmd = new SqlCommand("SELECT * FROM Quizzes", c); bool keepGoing = true;
            while (keepGoing) {
                SqlDataReader reader = cmd.ExecuteReader(); Console.Clear(); string holder = "";
                while (reader.Read()) {
                    Console.WriteLine($"[{reader["Id"]}]{reader["Text"]}\n{reader["subtext"]}\n");
                    holder += (reader["id"].ToString() + " ");
                } reader.Close();
                Console.WriteLine("\nSelect A[QUIZ]");
                UserInput = Console.ReadLine();
                keepGoing = checkChoice(holder);
                if (keepGoing) error();
                else quizId = Convert.ToInt16(UserInput); takeQuiz(c);
            }
            return false;
        }
        static internal void loadQuestions(SqlConnection c) {
            SqlCommand cmd = new SqlCommand($@"SELECT *, (SELECT COUNT(*) FROM Questions WHERE QuizId={quizId})  AS NumOQ  FROM Questions WHERE QuizId={quizId} ORDER BY QuestionNumber;", c);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                Dictionary<string, string> q = new Dictionary<string, string>() {
                    { "Id", reader["Id"].ToString()},
                    { "QuestionNumber",reader["QuestionNumber"].ToString()},
                    { "Text", reader["Text"].ToString()},
                    { "NumOQ", reader["NumOQ"].ToString()}
                };

                questions.Add(q);
            }
            reader.Close();
        }
        static internal void loadAnswers(SqlConnection c) {
            SqlCommand cmd = new SqlCommand($@"SELECT *, (SELECT COUNT(*) FROM Answers WHERE QuestionId={QuestionId})  AS AperQ  FROM Answers WHERE Questionid={QuestionId} ORDER BY AnswerNumber;", c);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {

                Dictionary<string, string> a = new Dictionary<string, string>() {
                    { "Id", reader["Id"].ToString()},
                    { "AnswerNumber",reader["AnswerNumber"].ToString()},
                    { "Text", reader["Text"].ToString()},
                    { "Score", reader["Score"].ToString()},
                    { "AperQ", reader["AperQ"].ToString()}
                };
                answers.Add(a);
                totalAnswers++;
            }
            reader.Close();
        }
        static internal string[] calculateResult(SqlConnection c) {
            SqlCommand cmd = new SqlCommand($@"Select * FROM Results WHERE QuizId={quizId}", c);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {//Transitioned
                string[] result = new string[5];
                result[0] = reader["Id"].ToString();
                result[1] = reader["Text"].ToString();
                result[2] = reader["SubText"].ToString();
                result[3] = reader["Max"].ToString();
                result[4] = reader["Min"].ToString();
                results.Add(result);
            };
            reader.Close();

            int j = 0; int i = 0; bool keepGoing = true;

            string[] final = new string[3];

            while (keepGoing) {
                int max = Convert.ToInt16(results[i][3]);
                int min = Convert.ToInt16(results[i][4]);
                if (Score <= max && Score >= min) {
                    if (j == 3) keepGoing = false;
                    else final[j] = results[i][j++];
                }
                else i++;
            }
            return final;
        }
        static public void displayResults(string[] finalResult)
        {
            Console.Clear();
            Console.WriteLine($"You Scored: {Score}\n\tYour Grade: {finalResult[2]}\n\n\t{finalResult[1]}");
            Console.ReadLine();
        }
        static void takeQuiz(SqlConnection c){
            loadQuestions(c);   int QuestionNumber = 0; 
            totalQuestions = Convert.ToInt16(questions[QuestionNumber]["NumOQ"]);
            int AnswerNumber = 0; 
            while (QuestionNumber <= totalQuestions - 1 ){
                QuestionId = Convert.ToInt16(questions[QuestionNumber]["Id"]); 
                loadAnswers(c); bool keepGoing = true;
                while (keepGoing) {
                    Console.Clear();
                    int placeHolder = AnswerNumber;
                    Console.WriteLine($"Question {questions[QuestionNumber]["QuestionNumber"]}) {questions[QuestionNumber]["Text"]}\n");
                    int AnswerHistory = AnswerNumber;
                    while (AnswerNumber < totalAnswers) {
                        Console.WriteLine($"\n\t[{answers[AnswerNumber]["AnswerNumber"]}]{answers[AnswerNumber++]["Text"]}");
                    }
                    Console.WriteLine("\n\nSelect An [ANSWER]");
                    UserInput = Console.ReadLine();
                    keepGoing = recordScore(placeHolder);
                    if (keepGoing) AnswerNumber = AnswerHistory; 
                }
                QuestionNumber++;
            }
        }
        //======WIP======
        //logIn();
        //createAccount();
        //quit();
        static public bool save(){
            Console.WriteLine("Would You Like To Save Your Results?");
            UserInput = Console.ReadLine();
            return true;
        }
        static bool logIn(SqlConnection c){
            bool matching = true;
            while (matching){
                Console.WriteLine("Username:\nPassword:\nEnter Your UserName");
                Console.WriteLine("\nEnter[Q]At Any Time To Quit");//Function?
                userName = Console.ReadLine().ToUpper();
                Console.Clear();
                if (userName == "Q") return false;
                Console.WriteLine($"Username: {userName}\nPassword:\nEnter Your Password");
                Console.WriteLine("\nEnter[Q]At Any Time To Quit");//Function?
                password = Console.ReadLine().ToUpper();
                Console.Clear();
                if (password == "Q") return false;
                SqlCommand cmd = new SqlCommand("SELECT * FROM User",c);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()){
                    if (userName == reader["userName"].ToString() && password == reader["password"].ToString()) matching = false;
                    else Console.Clear(); Console.WriteLine("UserName Or Password Incorrect\nPress[ENTER]To Continue..."); Console.ReadLine();
                }
                reader.Close();   Console.Clear();
            }
            return false;
        }
        static void continueAs(SqlConnection c)
        {
            bool keepGoing = true;
            while (keepGoing){
                Console.WriteLine("Would You Like To \n[A]Log In\t[B]Create Account\t[C]Continue As Guest");
                Console.WriteLine("Enter[Q]At Anytime To Quit");
                UserInput = Console.ReadLine();
                switch (UserInput){
                    case "A":
                        keepGoing = logIn(c);
                        break;
                    case "B":
                        //keepGoing = createAccount();
                        break;
                    case "C":
                        keepGoing = false;
                        break;
                    case "Q":
                        //keepGoing = quit();
                        break;
                    default:
                        error();
                        break;
                }
                Console.Clear();
            }
        }
    }
}