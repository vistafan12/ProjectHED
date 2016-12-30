﻿using System;
using System.IO;
using System.Diagnostics;

namespace ProjectHEDio
{
    public static class LogHelper
    {
        private static string LogFilepath = Environment.CurrentDirectory + "\\" + "ProjectHEDLog.txt";

        public static void ClearLogFile()
        {
            File.WriteAllText(LogFilepath, "");
        }

        public enum LogType
        {
            Normal,
            Warning,
            Error
        }

        public static void Log(string text, LogType type = LogType.Normal)
        {
            try
            {
                string result = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                result += " ";
                switch (type)
                {
                    case LogType.Normal:
                        result += "INFO:  ";
                        break;
                    case LogType.Warning:
                        result += "WARN:  ";
                        break;
                    case LogType.Error:
                        result += "ERROR: ";
                        break;
                }
                result += text;
                File.AppendAllText(LogFilepath, result + Environment.NewLine);
                Debug.WriteLine(result);
            }
            catch (Exception)
            {
                // ??? :l
            }
        }
    }
}
