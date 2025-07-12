using System;
using System.IO;

namespace AirQualityMonitor
{
    public class Tools
    {
        public static string GetSqlConnectionString()
        {
            string CurrentDir = Directory.GetCurrentDirectory();
            DirectoryInfo? ParentDirInfo = Directory.GetParent(CurrentDir);
            if (ParentDirInfo == null)
            {
                throw new Exception("There isn't a parent directory that I could find the Azure SQL connection string in!");
            }
            string ParentDir = ParentDirInfo.FullName;
            string FilePath = Path.Combine(ParentDir, "SQL_CONSTR.txt");
            if (System.IO.File.Exists(FilePath) == false)
            {
                throw new Exception("File '" + FilePath + "' does not exist! That is where the SQL Connection String was expected. Please place it in that file.");
            }
            string content = System.IO.File.ReadAllText(FilePath);
            if (content == "")
            {
                throw new Exception("File '" + FilePath + "' was empty and did not contain a SQL connection string! Please place it in there.");
            }
            return content;
        }
    }
}