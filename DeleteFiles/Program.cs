using System;
using System.Collections.Generic;
using System.IO;

namespace DeleteFiles {
     internal class Program {
        /*
         This program is designed to be ran in the task scheduler periodically to clean up files and log the results.
         The main function calls the Navigate Directory function which accepts an argument that is provided in the
         Task Scheduler. The argument expects a directory. If the directory does not exist, the failed attempt is logged.
         If the directory exists the program establishes two arrays, one for files and one for directories. Each file in
         the directory is looped through. There is an optional second argument that can be added to remove only files that
         haven't been written to x number of days. If this is ommitted, the files will attempt to delete without checking
         the last write date. The result of the deletion is logged. After the files have been iterated through, a recursive
         call is made to the Navigate Directroy function which ensures each directory and subdirectory is iterated through
         and a file deletion is attempted in each subddirectory. The WriteLog function writes to two separate locations:
         one for each day and the permanent log which is all days. The Format Row function ensures the rows are readable.

         Author: Joe Foley
           Date: 9/20/2022
        */


        static void Main(string[] args) {
            if (Directory.Exists(args[0])) {
                try {
                    NavigateDirectory(args[0], args);
                } catch (Exception ex) {
                    WriteLog("Unable to complete the deletion" + ex.GetType().ToString());
                }
            } else {
                WriteLog("Unable to complete the deletion as the directory does not exist.");
            }
        }

        private static void NavigateDirectory(string target, string[] args) {
            string[] files = Directory.GetFiles(target);
            string[] directories = Directory.GetDirectories(args[0]);

            foreach (string file in files) {
                ParseFile(file, args);
            }

            foreach (string directory in directories) {
                NavigateDirectory(directory, args);
            }
        }

        private static void ParseFile(string target, string[] args) {
            FileInfo info = new FileInfo(target);
            if (args[1] != null) {
                try {
                    if (info.LastWriteTime < DateTime.Now.AddDays(Convert.ToDouble(args[1]) * -1)) {
                        info.Delete();
                        WriteLog("SUCCESSFULLY DELETED FILE");
                    }
                } catch (Exception ex) {
                    WriteLog(String.Format("FAILED {0}", ex.GetType()));
                }
            } else {
                try {
                    info.Delete();
                    WriteLog("SUCCESSFULLY DELETED FILE");
                } catch (Exception ex) {
                    WriteLog(String.Format("FAILED {0}", ex.GetType()));
                }
            }
        }

        private static void WriteLog(string log) {
            string filename = String.Format(@"C:\FileDeletionLog\DeletionLog_{0}.txt", DateTime.Now.ToString("MM-dd-yyyy"));
            string permalog = @"C:\FileDeletionLog\PermaLog\log.txt";

            CompileFile(filename, log);
            CompileFile(permalog, log);
        }

        private static void CompileFile(string filename, string log) {
            List<string> filetext = new List<string>();

            if (File.Exists(filename)) {
                filetext.Add(FormatRow(log));
                filetext.Add("+---------------+------------------------------------------------------------------------------------------------------------------------+");

            } else {
                filetext.Add("+---------------+------------------------------------------------------------------------------------------------------------------------+");
                filetext.Add("| DATE          | STATUS                                                                                                                 |");
                filetext.Add("+---------------+------------------------------------------------------------------------------------------------------------------------+");
                filetext.Add(FormatRow(log));
                filetext.Add("+---------------+------------------------------------------------------------------------------------------------------------------------+");
            }

            File.AppendAllLines(filename, filetext);
        }

        private static string FormatRow(string text) {
            string row = "|  " + DateTime.Now.ToString("MM-dd-yyyy") + "   | " + text;

            for (int i = text.Length; i < 118; i++) {
                row += " ";
            }

            row += " |";

            return row;
        }
    }
}
