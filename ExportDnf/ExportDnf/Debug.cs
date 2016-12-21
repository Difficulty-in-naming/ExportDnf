using System;

namespace ExportDnf
{
    public static class Debug
    {

        public static bool LogAsset(bool condition)
        {
            if (condition)
                return true;
            LogError("检查条件,发现无法通过.");
            return false;
        }

        private static string WrapString(params object[] args)
        {
            int length = args.Length;
            string str = string.Empty;
            for (int i = 0; i < length; i++)
            {
                str += "{" + i + "}        ";
            }
            return str;
        }


        public static void Log(params object[] args) => Log(string.Format(WrapString(args), args));

        public static void LogWarning(params object[] args) => Log(string.Format(WrapString(args), args));

        public static void LogError(params object[] args) => Log(string.Format(WrapString(args), args));

        private static void Log(string str) => Console.WriteLine(str);
    }
}
