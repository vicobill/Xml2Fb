using System;

namespace Xml2Fb.Shared
{
    public static class DebugHelper
    {
        internal class ScopeConColor:IDisposable
        {
            internal ScopeConColor() { Console.ResetColor(); }
            ~ScopeConColor() { Console.ResetColor(); }
            void IDisposable.Dispose() { Console.ResetColor(); }

            internal ScopeConColor SetForegroundColor(ConsoleColor color)
            {
                Console.ForegroundColor = color;
                return this;
            }
            internal ScopeConColor SetBackgroundColor( ConsoleColor color )
            {
                Console.BackgroundColor = color;
                return this;
            }
            internal ScopeConColor Println(string prefix,string text )
            {
                Console.WriteLine($"[{prefix} {DateTime.Now}] {text}");
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


    }
}
