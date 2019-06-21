using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalBuzzFeed
{
    //myList[0][1]);
    class Buzzfeed
    {
        static void Main(string[] args)
        {
            SqlConnection c = connection;
            c.Open();
            showMenu(c);
            c.Close();
        }
        static public SqlConnection connection = new SqlConnection(@"Data Source = (LocalDB)\MSSQLLocalDB; 
                                                      AttachDbFilename = C:\Users\Owner\Desktop\BuzzFeedPersonal\Database1.mdf;Integrated Security = True");
        static internal int quizId; //Make sure its set
        static internal int QuestionId;
        static internal int Score;
        static internal int TempScore;
        static internal int totalAnswers;
        static internal int totalQuestions;
        static internal string UserInput;
        static internal List<string[]> questions = new List<string[]>();
        static internal List<string[]> answers = new List<string[]>(); // KEEPS LOADING SAME ANSWER AGAIN AND AGAIN
        static internal void error(){
            Console.Clear();
            Console.WriteLine("Invalid Entry, Please Try Again\nPress[ENTER]To Continue...");
            Console.ReadLine();
        }
        static bool checkChoice(string holder)
        {
            bool keepGoing = true;
            for (int i = 0; i < holder.Split().Length; i++)
            {
                if (UserInput.Split()[0] == holder.Split()[i]) { UserInput = UserInput.Split()[0]; keepGoing = false; }
            }
            if (keepGoing == false) return false;
            else return true;
        }
        static internal int selectCategory(SqlConnection c)
        {
            bool keepGoing = true;
            SqlCommand cmd = new SqlCommand("SELECT * FROM Category",c);
            while (keepGoing){
                SqlDataReader reader = cmd.ExecuteReader(); string holder = "";
                Console.Clear();
                while (reader.Read()){
                    Console.WriteLine($"[{reader["id"]}]{reader["Type"]}\n");
                    holder += (reader["id"].ToString()+ " ");
                } reader.Close();
                Console.WriteLine("\nSelect A[CATEGORY]");
                    UserInput = Console.ReadLine();
                    keepGoing = checkChoice(holder);
                    if (keepGoing) error();
            }
            return Convert.ToInt16(UserInput);
        }
        static internal void showQuizzesByCategory(SqlConnection c){
            int categoryId = selectCategory(c); bool keepGoing = true;
            SqlCommand cmd = new SqlCommand($"SELECT * FROM Quizzes WHERE CategoryId = {categoryId}",c);
            while (keepGoing){
                SqlDataReader reader = cmd.ExecuteReader(); Console.Clear(); string holder = "";
                while (reader.Read()){
                    Console.WriteLine($"[{reader["Id"]}]{reader["Text"]}\n{reader["subtext"]}\n");
                    holder += (reader["id"].ToString() + " ");
                }reader.Close();
                Console.WriteLine("\nSelect A[QUIZ]\tOr Enter[Q] At AnyTime To Quit");
                UserInput = Console.ReadLine();
                keepGoing = checkChoice(holder);
                if (UserInput.ToUpper() == "Q") keepGoing = false;
                if (keepGoing) error();
                else quizId = Convert.ToInt16(UserInput); takeQuiz(c);
                
            }
        }
        static internal void showAllQuizzes(SqlConnection c){
            SqlCommand cmd = new SqlCommand("SELECT * FROM Quizzes",c); bool keepGoing = true;
            while (keepGoing){
                SqlDataReader reader = cmd.ExecuteReader(); Console.Clear(); string holder = "";
                while (reader.Read()){
                    Console.WriteLine($"[{reader["Id"]}]{reader["Text"]}\n{reader["subtext"]}\n");
                    holder += (reader["id"].ToString() + " ");
                }reader.Close();
                Console.WriteLine("\nSelect A[QUIZ]");
                UserInput = Console.ReadLine();
                keepGoing = checkChoice(holder);
                if (keepGoing) error();
                else quizId = Convert.ToInt16(UserInput); takeQuiz(c);
            }
        }
        static internal void loadQuestions(SqlConnection c){
            SqlCommand cmd = new SqlCommand($@"SELECT *, (SELECT COUNT(*) FROM Questions WHERE QuizId={quizId})  AS NumOQ  FROM Questions WHERE QuizId={quizId} ORDER BY QuestionNumber;",c);
            SqlDataReader reader = cmd.ExecuteReader(); 
            string[] question = new string[4];
            while (reader.Read()){
                question[0] = reader["Id"].ToString();
                question[1] = reader["QuestionNumber"].ToString();
                question[2] = reader["Text"].ToString();
                question[3] = reader["NumOQ"].ToString();
                questions.Add(question);
            }
            reader.Close();
        }
        static internal void loadAnswers(int questionId, SqlConnection c){
            SqlCommand cmd = new SqlCommand($@"SELECT *, (SELECT COUNT(*) FROM Answers WHERE QuestionId={questionId})  AS AperQ  FROM Answers WHERE Questionid={questionId} ORDER BY AnswerNumber;",c);
            SqlDataReader reader = cmd.ExecuteReader();
            string[] answer = new string[5];
            while (reader.Read()){
                answer[0] = reader["Id"].ToString();
                answer[1] = reader["AnswerNumber"].ToString();
                answer[2] = reader["Text"].ToString();
                answer[3] = reader["Score"].ToString();
                answer[4] = reader["AperQ"].ToString();
                answers.Add(answer);
            }
            reader.Close();
        }
        static bool recordScore(){
            if (Int32.TryParse(UserInput, out int choice)){
                for (int i = 0; i <= totalAnswers; i++){
                    if (choice == Convert.ToInt16(answers[i][1])){
                        TempScore += Convert.ToInt16(answers[i][3]); return true;
                    }
                }
                if (Score + TempScore == Score && TempScore != 0){
                    error(); return false;
                }
                TempScore = 0;
            }
            else  error(); return false; 
        }
        static void takeQuiz(SqlConnection c){

            loadQuestions(c);   int QuestionNumber = 0;   bool keepGoing = true;
            Console.WriteLine(Convert.ToInt16(questions[QuestionNumber][3])); Console.ReadLine();
            totalQuestions = Convert.ToInt16(questions[QuestionNumber][3]);

            while (QuestionNumber < totalQuestions ){

                QuestionId = Convert.ToInt16(questions[QuestionNumber][0]);

                loadAnswers(QuestionId,c);
                totalAnswers = Convert.ToInt16(answers[QuestionNumber][4]);

                while (keepGoing) {
                    Console.Clear();   int AnswerNumber = 0;
                    
                    Console.WriteLine($"Question {questions[QuestionNumber][1]}) {questions[QuestionNumber][2]}\n");
                    while (AnswerNumber < totalAnswers){
                        Console.WriteLine($"\n[{answers[AnswerNumber][1]}]{answers[AnswerNumber++][2]}");
                    }
                    Console.WriteLine("\n\nSelect An [ANSWER]");
                    UserInput = Console.ReadLine();
                    keepGoing = recordScore();
                }
                QuestionNumber++;
            }
        }
        static internal void clearCache()
        {
            quizId = 0;
            QuestionId = 0;
            Score = 0;
            totalQuestions = 0;
            totalAnswers = 0;
        }
        static void selectQuiz(SqlConnection c) {
            bool keepGoing = true;
            while (keepGoing)
            {
                Console.Clear();
                Console.WriteLine("Would You Like To \n[A]View Quizzes By Category\t[B]View All Quizzes\t[C]Go Back To Last Page");
                switch (UserInput = Console.ReadLine().ToUpper())
                {
                    case "A":
                        showQuizzesByCategory(c); keepGoing = false;
                        break;
                    case "B":
                        showAllQuizzes(c); keepGoing = false;
                        break;
                    case "C":
                        keepGoing = false;
                        break;
                    default:
                        error();
                        break;
                } 
            }
        }
        static void showMenu(SqlConnection c){
            bool keepGoing = true;
            while (keepGoing){
                Console.Clear();
                Console.WriteLine("Welcome To PopQuiz!");
                Console.WriteLine("Would You Like To \n[A]Take A Quiz\t[B]Make A Quiz\t[Q]Quit");
                switch (UserInput = Console.ReadLine().ToUpper()){
                    case "A":
                        selectQuiz(c);
                        break;
                    case "B":
                        //makeQuiz();
                        break;
                    case "Q":
                        clearCache();
                        keepGoing = false;
                        break;
                    default:
                        error();
                        break;
                } 
            }
        }
    }
}