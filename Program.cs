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
        static void Main(string[] args)
        {
            SqlConnection c = connection;   bool keepGoing = true;
            c.Open();
            while (keepGoing)
            {
                showMenu(c);
                string[] finalResult = calculateResult(c);
                displayResults(finalResult);
                Console.WriteLine("Keep Going?");
                string userInput = Console.ReadLine().ToLower();
                if (userInput == "n") keepGoing = false;//PlaceHolder
                clearCache();
            }
            c.Close();
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
        static internal List<string[]> questions = new List<string[]>();
        static internal List<string[]> answers = new List<string[]>();
        static internal List<string[]> results = new List<string[]>();
        static internal void clearCache()
        {
            quizId = 0;
            QuestionId = 0;
            Score = 0;
            totalAnswers = 0;
            totalQuestions = 0;
            UserInput = "";
            questions = null;
            answers = null;
            Console.Clear();
        }
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
        static bool recordScore(int start){//Not Adding Last Score
            if (Int32.TryParse(UserInput, out int choice)){
                for (int i = start; i <= totalAnswers + start; i++){
                    if (choice == Convert.ToInt16(answers[i][1])){
                        choice = Convert.ToInt16(answers[i][3]);
                        Score += choice;
                        return false;
                    }
                }
            }
            else  error(); return true; 
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
                        keepGoing = false;
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
        static void selectQuiz(SqlConnection c) {
            bool keepGoing = true;
            while (keepGoing)
            {
                Console.Clear();
                Console.WriteLine("Would You Like To \n[A]View Quizzes By Category\t[B]View All Quizzes\t[C]Go Back To Last Page");
                switch (UserInput = Console.ReadLine().ToUpper())
                {
                    case "A":
                        showQuizzesByCategory(c);
                        keepGoing = false;
                        break;
                    case "B":
                        showAllQuizzes(c);
                        keepGoing = false;
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
                    Console.WriteLine($"[{reader["Id"]}]{reader["Text"]}\n\t({reader["subtext"]})\n");
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
            while (reader.Read()) { 
            string[] question = new string[4];
                question[0] = reader["Id"].ToString();
                question[1] = reader["QuestionNumber"].ToString();
                question[2] = reader["Text"].ToString();
                question[3] = reader["NumOQ"].ToString();
                questions.Add(question);
            }
            reader.Close();
        }
        static internal void loadAnswers(SqlConnection c){
            SqlCommand cmd = new SqlCommand($@"SELECT *, (SELECT COUNT(*) FROM Answers WHERE QuestionId={QuestionId})  AS AperQ  FROM Answers WHERE Questionid={QuestionId} ORDER BY AnswerNumber;",c);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()){ 
                string[] answer = new string[5];
                answer[0] = reader["Id"].ToString();
                answer[1] = reader["AnswerNumber"].ToString();
                answer[2] = reader["Text"].ToString();
                answer[3] = reader["Score"].ToString();
                answer[4] = reader["AperQ"].ToString();
                answers.Add(answer);
                totalAnswers++;
            }
            reader.Close();
        }
        static internal string[] calculateResult(SqlConnection c){
            SqlCommand cmd = new SqlCommand($@"Select * FROM Results WHERE QuizId={quizId}",c);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()){
                string[] result = new string[5];
                result[0] = reader["Id"].ToString();
                result[1] = reader["Text"].ToString();
                result[2] = reader["SubText"].ToString();
                result[3] = reader["Max"].ToString();
                result[4] = reader["Min"].ToString();
                results.Add(result);
            }
            string[] final = new string[3];

            int j = 0;   int i = 0;   bool keepGoing = true;

            while (keepGoing){
                int max = Convert.ToInt16(results[i][3]); //Index Out Of Range
                int min = Convert.ToInt16(results[i][4]);
                if (Score <= max && Score >= min){
                    if (j == 2) keepGoing = false;
                    else final[j] = results[i][j++];
                }
                else i++;
            }
            return final;
        }
        static public void displayResults(string[] finalResult)
        {
            Console.Clear();
            Console.WriteLine($"You Scored: {Score}\n\t{finalResult[1]}\n\t{finalResult[2]}");
            Console.ReadLine();
            //Save Result? If User = true, save to DB ....If User = False, Give Result Id
        }
        static void takeQuiz(SqlConnection c){
            loadQuestions(c);   int QuestionNumber = 0; 
            totalQuestions = Convert.ToInt16(questions[QuestionNumber][3]);
            int AnswerNumber = 0; 
            while (QuestionNumber <= totalQuestions - 1 ){
                QuestionId = Convert.ToInt16(questions[QuestionNumber][0]); 
                loadAnswers(c); bool keepGoing = true;
                while (keepGoing) {
                    Console.Clear();
                    Console.WriteLine("Score: "+ Score);//SCORE LOG
                    int placeHolder = AnswerNumber;
                    Console.WriteLine($"Question {questions[QuestionNumber][1]}) {questions[QuestionNumber][2]}\n");
                    while (AnswerNumber < totalAnswers) {
                        Console.WriteLine($"\n\t[{answers[AnswerNumber][1]}]{answers[AnswerNumber++][2]}");
                    }
                    Console.WriteLine("\n\nSelect An [ANSWER]");
                    UserInput = Console.ReadLine();
                    keepGoing = recordScore(placeHolder);
                    if (keepGoing) error();
                }
                QuestionNumber++;
            }
        }
    }
}