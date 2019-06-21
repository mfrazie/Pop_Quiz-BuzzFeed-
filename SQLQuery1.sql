SELECT *, (SELECT COUNT(*) FROM Answers WHERE QuestionId=1)  AS AperQ  FROM Answers WHERE Questionid=1 ORDER BY AnswerNumber;
                                                