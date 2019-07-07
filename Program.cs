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
            keepGoing = false;

            c.Open();
            while (keepGoing) {
                continueAs(c);
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
        static internal string userInput;
        static internal string[] userName = new string[2];
        static internal string password;
        static bool account = false;
        static internal List<Dictionary<string, string>> questions = new List<Dictionary<string, string>>();
        static internal List<Dictionary<string, string>> answers = new List<Dictionary<string, string>>();
        static internal List<string[]> results = new List<string[]>();
        static internal bool clearCache() {
            quizId = 0;
            QuestionId = 0;
            Score = 0;
            totalAnswers = 0;
            totalQuestions = 0;
            userInput = "";
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
                if (userInput.Split()[0] == holder.Split()[i]) userInput = userInput.Split()[0]; keepGoing = false;
            }
            if (keepGoing == false) return false;
            else return true;
        }
        static bool recordScore(int start) {
            if (Int32.TryParse(userInput, out int choice)) {
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
                userInput = Console.ReadLine().ToUpper();
                if (userInput == "Y") { keepGoing = false; result = true; }
                else if (userInput == "N") { keepGoing = false; result = false; }
                else error();
            }
            return result;
        }
        static void showMenu(SqlConnection c) {
            bool keepGoing = true;
            while (keepGoing) {
                Console.Clear();
                Console.WriteLine("Would You Like To \n[A]Take A Quiz\t[B]Make A Quiz\t[Q]Quit");
                switch (userInput = Console.ReadLine().ToUpper()) {
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
                switch (userInput = Console.ReadLine().ToUpper())
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
                userInput = Console.ReadLine();
                keepGoing = checkChoice(holder);
                if (keepGoing) error();
            }
            return Convert.ToInt16(userInput);
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
                userInput = Console.ReadLine();
                keepGoing = checkChoice(holder);
                if (userInput.ToUpper() == "Q") keepGoing = false;
                if (keepGoing) error();
                else quizId = Convert.ToInt16(userInput); takeQuiz(c);
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
                userInput = Console.ReadLine();
                keepGoing = checkChoice(holder);
                if (keepGoing) error();
                else quizId = Convert.ToInt16(userInput); takeQuiz(c);
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
                    userInput = Console.ReadLine();
                    keepGoing = recordScore(placeHolder);
                    if (keepGoing) AnswerNumber = AnswerHistory; 
                }
                QuestionNumber++;
            }
        }
        //======WIP======\\
        //quit();
        //MORE COMMENTS
        static public bool save(string[]final){
            Console.WriteLine("Would You Like To Save Your Results?");
            userInput = Console.ReadLine();
            if (account == false) { Console.WriteLine($"Write Down Your Id\nYour Result Id: {final[0]}"); }
            else if (account == true){
                SqlCommand cmd = new SqlCommand($@"INSERT INTO History (userId,resultId)
                                                              VALUES ({userName[1]},{final[0]})");
            }
            return true;
        }
        static bool logIn(SqlConnection c)
        {
            bool matching = true;
            while (matching){
                //Enter User Name
                Console.WriteLine("Username:\nPassword:\nEnter Your UserName");
                Console.WriteLine("\nEnter[Q]At Any Time To Quit");//Function?
                userName[0] = Console.ReadLine();
                Console.Clear();
                if (userName[0].ToUpper().Split()[0] == "Q") return false;
                //Enter Password
                Console.WriteLine($"Username: {userName}\nPassword:\nEnter Your Password");
                Console.WriteLine("\nEnter[Q]At Any Time To Quit");//Function?
                password = Console.ReadLine();
                Console.Clear();
                if (password.ToUpper().Split()[0] == "Q") return false;
                SqlCommand cmd = new SqlCommand("SELECT * FROM User",c);
                SqlDataReader reader = cmd.ExecuteReader();
                //Checking User Name & Password Against DataBase
                while (reader.Read()){
                    if (userName[0].ToUpper() == reader["userName"].ToString().ToUpper() && password.ToUpper() == reader["password"].ToString().ToUpper()){//Login Matches
                        matching = false; userName[1] = reader["Id"].ToString(); }
                }
                reader.Close();
                if (matching) {
                    Console.Clear(); //Login Does Not Match
                    Console.WriteLine("UserName Or Password Incorrect Try Again\nPress[ENTER]To Continue...");
                    Console.ReadLine();
                }
                Console.Clear();
            }
            return false;
        }
        static void continueAs(SqlConnection c)
        {
            bool keepGoing = true;
            while (keepGoing){
                Console.WriteLine("Welcome To PopQuiz!");
                Console.WriteLine("Would You Like To \n[A]Log In\t[B]Create Account\t[C]Continue As Guest");
                Console.WriteLine("Enter[Q]At Anytime To Quit");
                userInput = Console.ReadLine();
                switch (userInput){
                    case "A":
                        keepGoing = logIn(c);
                        break;
                    case "B":
                        keepGoing = createAccount(c);
                        break;
                    case "C":
                        keepGoing = false;
                        break;
                    case "Q":
                        Console.Clear();
                        Console.WriteLine("Are You Sure You Want To Quit?");
                        if ((userInput = Console.ReadLine().ToUpper()) == "Y") keepGoing = false;
                        break;
                    default:
                        error();
                        break;
                }
                Console.Clear();
            }
        }
        static bool invalidUserName(SqlConnection c){
            SqlCommand cmd = new SqlCommand("SELECT * FROM User",c);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()){
                //Checking To See If Given User Name Has Spaces
                if (userInput.Split().Length > 1){
                    reader.Close(); Console.Clear();
                    Console.WriteLine("User Name Cannot Contain Spaces\nPress[ENTER]To Continue...");
                    Console.ReadLine(); Console.Clear(); return true;
                }
                //Checking To See If Given User Name Already Exist
                else if (userInput == reader["userName"].ToString()){
                    reader.Close(); Console.Clear();
                    Console.WriteLine("User Name Alread Exists\nPress[ENTER]To Continue...");
                    Console.ReadLine(); Console.Clear();  return true;
                }
            }
            reader.Close();
            //If No Conditionals Reached User Input Is Set To User Name
            userName[0] = userInput;
            return false;
        }
        static bool createAccount(SqlConnection c){
            bool keepGoing = true;
            while (keepGoing){
                bool exist = true;
                while (exist){
                    Console.WriteLine("New Account\n\nEnter Your Desired User Name:\n");
                    Console.WriteLine("Enter[Q]At Anytime To Quit\n");
                    userInput = Console.ReadLine();
                    if (userInput.ToUpper().Split()[0] == "Q") return true;
                    exist = invalidUserName(c);
                }
                //Sets Valid userInput To userName
                Console.Clear();
                Console.WriteLine($"New Account\n\nUser Name: {userName}\nEnter Your Desired Password:\n");
                Console.WriteLine("Enter[Q]At Anytime To Quit\n");
                password = Console.ReadLine();
                if (userInput.ToUpper().Split()[0] == "Q") return true;
                //Verifying Information
                bool notValid = true;
                while (notValid){
                    Console.WriteLine($"New Account\n\nUser Name: {userName}\nPassword: {password}");
                    Console.WriteLine("Is This Information Correct? [Y/N]\n");
                    switch (userInput = Console.ReadLine().ToUpper()){
                        case "Y": account = true; keepGoing = false; notValid = false;
                            break;
                        case "N":
                            bool incorrect = true;
                            while (incorrect){
                                Console.WriteLine("What Would You Like To Change?");
                                Console.WriteLine("[A]User Name\t[B]Password\t[C]Start Over?\n");
                                switch (userInput = Console.ReadLine().ToUpper()){
                                    case "A"://Updates User Name
                                        Console.WriteLine("Enter Your Desired User Name:\n");
                                        userInput = Console.ReadLine();
                                        incorrect = invalidUserName(c);
                                        break;
                                    case "B"://Updates Password
                                        Console.WriteLine($"Current Password: {password}\nEnter Your New Password:\n");
                                        password = Console.ReadLine();
                                        break;
                                    case "C"://Restarts Creating Account 
                                        incorrect = false;
                                        notValid = false;
                                        break;
                                    default:  error();
                                        break;
                                } 
                            }
                            break;
                        default:  error();
                            break;
                    }  
                }
            }
            if (account){//Creates Account
                SqlCommand cmd = new SqlCommand("INSERT INTO \"User\" (userName,password)" +
                                                $"VALUES ('{userName[0]}','{password})';"+
                                                "SELECT @@Identity AS UId FROM \"User\"", c);
                SqlDataReader reader = cmd.ExecuteReader();
                //Grabs The UserId Of The Account Created And Saves It To The User Name Variable
                reader.Read();    userName[1] = reader["UId"].ToString();    reader.Close();
            }
            return false;
        }

    }
}