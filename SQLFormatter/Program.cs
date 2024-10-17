using System;
using System.IO;
using System.Text;
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
            {
                Console.WriteLine($"Detected encoding: {DetectEncoding(filePath)}");

                //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                //Encoding windows1251 = Encoding.GetEncoding(1251);
                //string fileContent = File.ReadAllText(filePath);

                FormatFile(filePath);
            }
            //using (StreamReader reader = new StreamReader(sqlFiles[0], true)) // true - для автоопределения кодировки
            //{
            //    string fileContent = reader.ReadToEnd();
            //    System.Console.WriteLine("Содержимое файла:");
            //    System.Console.WriteLine(fileContent);
            //    System.Console.WriteLine("Кодировка: " + reader.CurrentEncoding);
            //}
        }

        static string DetectEncoding(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string detectedEncoding = "Unknown or ANSI";

            if (fileBytes.Length >= 3 && fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF)
            {
                detectedEncoding = "UTF-8 with BOM";
            }
            else if (fileBytes.Length >= 2 && fileBytes[0] == 0xFF && fileBytes[1] == 0xFE)
            {
                detectedEncoding = "UTF-16 LE";
            }
            else if (fileBytes.Length >= 2 && fileBytes[0] == 0xFE && fileBytes[1] == 0xFF)
            {
                detectedEncoding = "UTF-16 BE";
            }
            return detectedEncoding;
        }

        private static void FormatFile(string filePath)
        {
            try
            {
                string sqlContent = File.ReadAllText(filePath, Encoding.UTF8);

                int createProcedureIndex = Regex.Match(sqlContent, @"\b(create|alter)\b\s+\b(procedure|view)\b", RegexOptions.IgnoreCase).Index;

                if (createProcedureIndex == -1)
                {
                    Console.WriteLine("Команды создания или изменения объекта не найдены в файле.");
                    return;
                }

                string beforeCreateProcedure = sqlContent.Substring(0, createProcedureIndex);
                string afterCreateProcedure = sqlContent.Substring(createProcedureIndex);

                #region обработка комментариев к скрипту в шапке
                string todayDate = DateTime.Now.ToString("yyyyMMdd");

                // Находим все блоки комментариев, начинающиеся с "Ticket No" и заканчивающиеся перед следующим таким блоком или концом
                string pattern = @"(--\s*Ticket No\s*:.*?)(?=(--\s*Ticket No\s*:|\*/|$))";

                // Получаем все совпадения блоков
                var matches = Regex.Matches(beforeCreateProcedure, pattern, RegexOptions.Singleline);

                if (matches.Count > 1)
                {
                    // Берём последний блок
                    var lastBlock = matches[matches.Count - 1];

                    // Выполняем замену внутри последнего блока комментариев к скрипту
                    string modifiedBlock = lastBlock.Groups[1].Value;
                    modifiedBlock = Regex.Replace(modifiedBlock, @"--\s*Modified\s+on\s*:\s*\d{8}", $"-- Modified on      : {Regex.Match(modifiedBlock, @"\d{8}").Value}, {todayDate} (prod)", RegexOptions.IgnoreCase);
                    modifiedBlock = Regex.Replace(modifiedBlock, @"--\s*Created\s+on\s*:\s*\d{8}", $"-- Modified on      : {Regex.Match(modifiedBlock, @"\d{8}").Value}, {todayDate} (prod)", RegexOptions.IgnoreCase);
                    modifiedBlock = Regex.Replace(modifiedBlock, @"--\s*Created\s+by\s*:", $"-- Modified by      :", RegexOptions.IgnoreCase);

                    // Заменяем исходный текст на модифицированный последний блок
                    beforeCreateProcedure = beforeCreateProcedure.Remove(lastBlock.Index, lastBlock.Length).Insert(lastBlock.Index, modifiedBlock);
                }
                else
                {
                    beforeCreateProcedure = Regex.Replace(beforeCreateProcedure, @"--\s*Created\s+on\s*:\s*\d{8}", $"-- Created on       : {Regex.Match(beforeCreateProcedure, @"\d{8}").Value}, {todayDate} (prod)", RegexOptions.IgnoreCase);
                    beforeCreateProcedure = Regex.Replace(beforeCreateProcedure, @"--\s*Modified\s+on\s*:\s*\d{8}", $"-- Created on       : {Regex.Match(beforeCreateProcedure, @"\d{8}").Value}, {todayDate} (prod)", RegexOptions.IgnoreCase);
                    beforeCreateProcedure = Regex.Replace(beforeCreateProcedure, @"--\s*Modified\s+by\s*:", $"-- Created by       :", RegexOptions.IgnoreCase);
                }
                #endregion


                #region удаление комментариев в теле скрипта
                // Удаление всех однострочных комментариев (начинаются с "--")
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"^[\t\s]*--(?!.*\b(Step|DM-)\b).*?(?:\r?\n|$)", "\r\n", RegexOptions.Multiline);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"--(?!.*\b(Step|DM-)\b).*$", "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"(\r?\n){4,}", "\r\n\r\n");


                // Удаление всех многострочных комментариев (начинаются с "/*" и заканчиваются "*/")
                //afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"/\*(?!.*\bDM-\d+\b).*?\*/", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                //afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"/\*(?!.*?\bDM-\d+\b).*?\*/", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                //afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"/\*.*?\*/", "", RegexOptions.Singleline);
                //afterCreateProcedure = Regex.Replace(afterCreateProcedure, @" {2,}", " ");

                // Удаление всех многострочных комментариев (начинаются с "/*" и заканчиваются "*/")
                var multiLineComments = Regex.Matches(afterCreateProcedure, @"/\*.*?\*/", RegexOptions.Singleline);

                foreach (Match match in multiLineComments)
                {
                    if (!Regex.IsMatch(match.Value, @"\bDM-\d+\b", RegexOptions.IgnoreCase))
                    {
                        afterCreateProcedure = afterCreateProcedure.Replace(match.Value, "");
                    }
                }
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @" {2,}", " ");
                #endregion


                #region замена sql-команд на верхний регистр
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
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bcase\b", "CASE", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bwhen\b", "WHEN", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\belse\b", "ELSE", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bthen\b", "THEN", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\band\b", "AND", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bor\b", "OR", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bnot\b", "NOT", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bdelete\b", "DELETE", RegexOptions.IgnoreCase);
                afterCreateProcedure = Regex.Replace(afterCreateProcedure, @"\bprint\b", "PRINT", RegexOptions.IgnoreCase);
                #endregion


                string updatedContent = beforeCreateProcedure + afterCreateProcedure;

                File.WriteAllText(filePath, updatedContent, Encoding.Unicode);

                Console.WriteLine("SQL команды успешно заменены на верхний регистр, комментарии удалены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}