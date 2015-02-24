using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;


namespace SageMan
{
    public enum OutputLevel { quiet, info, verbose, debug };

    public class Utils
    {
        static bool buildtimeInited = false;
        static DateTime buildtime;
        static public DateTime BuildTime
        {
            get
            {
                if (!buildtimeInited)
                {
                    AssemblyName name = Assembly.GetExecutingAssembly().GetName();
                    DateTime bldday = new DateTime(2000, 1, 1).AddDays(Convert.ToDouble(name.Version.Build));
                    int secs = name.Version.Revision;
                    TimeSpan span = new TimeSpan(0, 0, 2 * secs);
                    buildtime = bldday + span;
                    buildtimeInited = true;
                }
                return buildtime;
            }
        }
        public static double ParseDouble(string s, double def)
        {
            double dval = 0;
            var ok = double.TryParse(s, out dval);
            if (!ok) return def;
            return dval;
        }
        public static float ParseFloat(string s, float def)
        {
            float dval = 0;
            var ok = float.TryParse(s, out dval);
            if (!ok) return def;
            return dval;
        }
        public static int ParseInt(string s, int def)
        {
            int ival = 0;
            var ok = int.TryParse(s, out ival);
            if (!ok) return def;
            return ival;
        }
        public static long ParseLong(string s, long def)
        {
            long ival = 0;
            var ok = long.TryParse(s, out ival);
            if (!ok) return def;
            return ival;
        }
        public static bool ParseBool(string s, bool def)
        {
            bool bval = false;
            var ok = bool.TryParse(s, out bval);
            if (!ok) return def;
            return bval;
        }
        static bool verStringInited = false;
        static string verString;
        static public String VersionString
        {
            get
            {
                if (!verStringInited)
                {
                    Assembly assem = Assembly.GetEntryAssembly();
                    AssemblyName assemName = assem.GetName();
                    Version ver = assemName.Version;

                    verString = ver.ToString();
                    verStringInited = true;
                }
                return verString;
            }
        }
    }

    public class StopWatch
    {
        DateTime start;
        double elap;
        public StopWatch()
        {
            start = DateTime.Now;
            elap = 0;
        }
        public double Stop()
        {
            DateTime end = DateTime.Now;
            TimeSpan tsElap = end - start;
            elap = tsElap.TotalSeconds;
            return elap;
        }
        public double Elap
        {
            get { return elap; }
        }
        public double Mark()
        {
            TimeSpan ts = DateTime.Now - start;
            elap = ts.TotalSeconds;
            return elap;
        }
    }

  

  
    public class MainClass
    {
          /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var opt = new CmdLineOptProcessor();

            opt.SetOpts(args);
            var stw = new StopWatch();
            var sage = new SageCmdLineProcessor(opt.SageOpts);
            try
            {
                if (!opt.HelpMe)
                {
                    sage.ProcessCommand();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception:"+ex.Message);
                Console.WriteLine("Stack Trace:" + ex.StackTrace);
            }
            stw.Stop();
            Console.WriteLine("Took " + stw.Elap.ToString("F2") + " secs");
            if (opt.WaitReturn)
            {
                Console.ReadLine();
            }
        }
    }
}
