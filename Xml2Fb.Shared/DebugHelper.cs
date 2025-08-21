using System;
using System.Runtime.CompilerServices;

namespace Xml2Fb.Shared
{
    public static class DebugHelper
    {
        internal class ScopeConColor : IDisposable
        {
            internal ScopeConColor() { Console.ResetColor(); }
            ~ScopeConColor() { Console.ResetColor(); }
            void IDisposable.Dispose() { Console.ResetColor(); }

            internal ScopeConColor SetForegroundColor( ConsoleColor color )
            {
                Console.ForegroundColor = color;
                return this;
            }
            internal ScopeConColor SetBackgroundColor( ConsoleColor color )
            {
                Console.BackgroundColor = color;
                return this;
            }
            internal ScopeConColor Println( string prefix, string text )
            {
                Console.WriteLine( $"[{prefix} {DateTime.Now}] {text}" );
                return this;
            }
        }

        public static void PrintDebug( string msg )
        {
            using ScopeConColor scc = new ScopeConColor();
            scc.SetForegroundColor( ConsoleColor.Blue )
                .Println( "DEBUG", msg );
        }

        public static void PrintError( string msg )
        {
            using ScopeConColor scc = new ScopeConColor();
            scc.SetForegroundColor( ConsoleColor.Red )
                .Println( "ERROR", msg );

        }
        public static void PrintDanger( string msg )
        {
            using ScopeConColor scc = new ScopeConColor();
            scc.SetForegroundColor( ConsoleColor.Red )
                .Println( "DANGER", msg );

        }
        public static void PrintWarning( string msg )
        {
            using ScopeConColor scc = new ScopeConColor();
            scc.SetForegroundColor( ConsoleColor.Yellow )
                .Println( "WARN", msg );
        }
        public static void Print( string msg )
        {
            using ScopeConColor scc = new ScopeConColor();
            scc.Println( "INFO", msg );
        }

        public static void PrintOk( string msg )
        {
            using ScopeConColor scc = new ScopeConColor();
            scc.SetForegroundColor( ConsoleColor.Green )
                .Println( "OK", msg );
        }



    }


    public static class SourceInfo
    {
        // 获取当前行号
        public static int Line( [CallerLineNumber] int line = 0 ) => line;

        // 获取当前文件名
        public static string File( [CallerFilePath] string file = "" ) => file;

        // 获取当前方法名
        public static string Method( [CallerMemberName] string method = "" ) => method;

        // 完整位置信息（格式：文件:行号）
        public static string Location(
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0 )
        {
            return $"{System.IO.Path.GetFileName( file )}:{line}";
        }
    }
}
