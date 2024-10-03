using System;
using System.IO;
using System.Text.RegularExpressions;

namespace SQLFormatter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path of sql-files folder:");
            string folderPath = Console.ReadLine();
            string[] sqlFiles = Directory.GetFiles(folderPath, "*.sql");

            foreach (string filePath in sqlFiles)
                FormatFile(filePath);

        }

        private static void FormatFile(string filePath)
        {
            try
            {
                string sqlContent = File.ReadAllText(filePath);

                int createProcedureIndex = Regex.Match(sqlContent, @"\bcreate\b\s+\bprocedure\b", RegexOptions.IgnoreCase).Index;

                if (createProcedureIndex == -1)
                {
                    Console.WriteLine("CREATE PROCEDURE не найдено в файле.");
                    return;
                }

                string beforeCreateProcedure = sqlContent.Substring(0, createProcedureIndex);
                string afterCreateProcedure = sqlContent.Substring(createProcedureIndex);

                string todayDate = DateTime.Now.ToString("yyyyMMdd");
                beforeCreateProcedure = Regex.Replace(beforeCreateProcedure, @"--\s*Modified\s+on\s*:\s*\d{8}", $"-- Created on       : {Regex.Match(beforeCreateProcedure, @"\d{8}").Value}, {todayDate} (prod)", RegexOptions.IgnoreCase);
                beforeCreateProcedure = Regex.Replace(beforeCreateProcedure, @"--\s*Modified\s+by\s*:", $"-- Created by       :", RegexOptions.IgnoreCase);

                // Удаление всех однострочных комментариев (начинаются с "--")
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"^[\t\s]*--(?!.*\bStep\b).*?(?:\r?\n|$)", "\n", RegexOptions.Multiline);

                // Удаление всех многострочных комментариев (начинаются с "/*" и заканчиваются "*/")
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"/\*.*?\*/", "", RegexOptions.Singleline);

                // Замена команд SQL на верхний регистр в части после "create procedure"
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bcreate\b", "CREATE", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bprocedure\b", "PROCEDURE", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bset\b", "SET", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bdeclare\b", "DECLARE", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bbegin\b", "BEGIN", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bend\b", "END", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bdrop\b", "DROP", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\btable\b", "TABLE", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bif\b", "IF", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bexists\b", "EXISTS", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bselect\b", "SELECT", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bfrom\b", "FROM", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bwhere\b", "WHERE", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bjoin\b", "JOIN", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bon\b", "ON", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bas\b", "AS", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bvalues\b", "VALUES", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bupdate\b", "UPDATE", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\binsert\b", "INSERT", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bnocount\b", "NOCOUNT", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\btop\b", "TOP", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bexec\b", "EXEC", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bwith\b", "WITH", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bouter\b", "OUTER", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bcross\b", "CROSS", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\binner\b", "INNER", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bleft\b", "LEFT", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bright\b", "RIGHT", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bapply\b", "APPLY", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bunion\b", "UNION", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bunion[\t\s]*all\b", "UNION ALL", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\border[\t\s]*by\b", "ORDER BY", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bdistinct\b", "DISTINCT", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\binto\b", "INTO", RegexOptions.IgnoreCase);

                string updatedContent = beforeCreateProcedure + afterCreateProcedure;

                File.WriteAllText(filePath, updatedContent);

                Console.WriteLine("SQL команды успешно заменены на верхний регистр, комментарии удалены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}