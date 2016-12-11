using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace tcpproj
{
    /// <summary> MyPrint:
    /// This class optionally prints to 3 locations: File, Console, and Form.
    /// </summary>
    public static class MyPrint
    {
        private static Boolean file_opt = false;    // Print to file
        private static Boolean system_opt = true;  // Print to console
        private static Boolean form_opt = false;   // Print to form
        private static String formout;
        private static TextWriter smout = null;

        /// <summary>Open the file to write to: pass in filename </summary>
        public static void SmartOpen(string s) {
            file_opt = true;
            smout = new StreamWriter(s);
        }

        /// <summary>Write to file, console, and/or form </summary>
        public static void SmartWrite(string s)
        {
            if (file_opt) smout.Write(s);
            if (system_opt) Console.Out.Write(s);
            if (form_opt) formout = s;
        }
        /// <summary>WriteLine (incl CR) to file, console, and/or form </summary>
        public static void SmartWriteLine(String s)
        {
            if (file_opt)  smout.WriteLine(s); 
            if (system_opt) Console.Out.WriteLine(s);
            if (form_opt) formout = s;
        }
        /// <summary>Close file to write to.  Prevent further file writes. </summary>
        public static void SmartClose()
        {
            if (file_opt) smout.Close();
            file_opt = false;
        }
    }
}
