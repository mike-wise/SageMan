
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;


namespace SageMan
{
    public class SageOpts
    {
        public string VersionString { get; set; }
        public bool DoAll { get; set; }
        public bool Execute { get; set; }
        public bool Verbose { get; set; }
        public bool UseList { get; set; }
        public SageCmd Cmd { get; set; }
        public string ModelName { get; set; }
        public string BuildiId { get; set; }
        public string Filepath { get; set; }
        public string Seedlist { get; set; }
        public string Builddesc { get; set; }
        public string User { get; set; }
        public string Akey { get; set; }
        public int Nrecs { get; set; }
        public SageOpts()
        {
            Cmd = SageCmd.getmodels;
            Execute = Verbose = DoAll = UseList = false;
            VersionString = ModelName = BuildiId = Filepath = Seedlist = Builddesc = "";
            //"2406e770-769c-4189-89de-1c9283f93a96,552a1940-21e4-4399-82bb-594b46d7ed54";
            Nrecs = 4;
        }
    }
    public enum SageCmd
    {
        getuserinfo,
        deluserinfo,
        setuser,
        setaccountkey,
        getmodels,
        getmodel,
        createmodel,
        delmodel,
        importcatalog,
        importusage,
        buildmodel,
        buildstatus,
        getcatalog,
        notifications,
        getrecos,
        getuserbuildsstatus
    }

    public class CmdLineOptProcessor
    {
        public static OutputLevel OutLev = OutputLevel.info;
        public static Random Random = null;

        private static string verstr = "0.5 - 24 Feb 2015";
        public List<string> Cmds = new List<string>();
        public Dictionary<string, string> Opts = new Dictionary<string, string>();

        public bool WaitReturn = false;
        public bool HelpMe = false;
        public SageOpts SageOpts = new SageOpts();


        private string helpString =
            "Sage Command Line Runner " + verstr + "\n" +
            "  Info, Create, Delete\n" +
            "     -gm                 => get list of models\n" +
            "     -cm:modelname       => create model\n" +
            "     -dm:modelname       => delete model name (needs -x to actually execute)\n" +
            "  Build Model\n" +
            "     -ic:modelname       => import catalog\n" +
            "     -iu:modelname       => import usage\n" +
            "     -bm:modelname       => build model\n" +
            "  Status\n" +
            "     -guinfo             => get user info from app properties\n" +
            "     -duinfo             => delete user info from app properties\n" +
            "     -gbus               => get user builds status\n" +
            "     -qbm:modelname      => get build status\n" +
            "     -qn:modelname       => get notifications\n" +
            "     -qc:modelname       => get catalog\n" +
            "     -grm:modelname      => get recommendations\n" +
            "  Parameters\n" +
            "     -fp:filepath.csv    => filepath\n" +
            "     -bd:description     => build description\n" +
            "     -dbm:buildid        => delete a build id\n" +
            "     -rsl:nr             => recommendation seed list\n" +
            "     -rul                => recommendation use list\n" +
            "     -nr:nr              => number of recommendations to make\n" +
            "     -sus:user           => set user\n" +
            "     -sak:accountkey     => set account key\n" +
            "     -v                  => verbose output\n" +
            "     -a                  => do all items (builds,etc.)\n" +
            "     -x                  => execute dangerous commands (those that delete things)n\n" +
            "     -?  or -h           => This help message\n" +
            "     -w                  => wait until user presses enter before exiting\n" +
            "Note that all commands should be case insensitive\n\n" +
            "Examples:\n" +
            "    sageman -gm -v                                => get list of models with full data\n" +
            "    sageman -cm:modelname -x                      => create modelname\n" +
            "    sageman -dm:modelname -x                      => delete modelname\n" +
            "    sageman -ic:modelname -fp:catalog.csv -x      => import catalog file catalog.csv into modelname\n" +
            "    sageman -iu:modelname -fp:usasge.csv -x       => import usage file useag.csv into modelname\n" +
            "    sageman -bm:modelname -x                      => build a model for modelname\n" +
            "    sageman -qbm:modelname                        => get build status for modelname\n"
            ;

        private void DoHelp()
        {
            Console.WriteLine(helpString);
            // Environment.Exit(0);
        }

        public void SetOpts(string[] args)
        {
            SageOpts.VersionString = "SageMan " + verstr;
            int mode = 0;         // don't move mode and lastarg into the loop dummy!
            string lastarg = "";
            if (args.Length == 0)
            {
                HelpMe = true;
                DoHelp();
            }
            foreach (string argu in args)
            {
                string swarg = "";
                string arg = argu;
                // string arg = argu.ToLower(System.Globalization.CultureInfo.InvariantCulture);
                if (mode == 0)
                {
                    swarg = arg;
                }
                else
                {
                    swarg = lastarg;
                }
                string[] sar = swarg.Split(':');
                string sar1 = "";
                if (sar.Length > 1)
                {
                    sar1 = sar[1];
                }
                switch (sar[0])
                {
                    case "-h":
                    case "-?":
                        {
                            HelpMe = true;
                            DoHelp();
                            break;
                        }
                    case "-w":
                        {
                            WaitReturn = true;
                            break;
                        }
                    case "-v":
                        {
                            SageOpts.Verbose = true;
                            break;
                        }
                    case "-a":
                        {
                            SageOpts.DoAll = true;
                            break;
                        }
                    case "-x":
                        {
                            SageOpts.Execute = true;
                            break;
                        }
                    // start of variable command line parameters
                    case "-guinfo":
                    case "-guser":
                        {
                            SageOpts.Cmd = SageCmd.getuserinfo;
                            break;
                        }
                    case "-duinfo":
                    case "-duser":
                        {
                            SageOpts.Cmd = SageCmd.deluserinfo;
                            break;
                        }
                    case "-gm":
                        {
                            SageOpts.Cmd = SageCmd.getmodels;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-cm":
                        {
                            SageOpts.Cmd = SageCmd.createmodel;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-dm":
                        {
                            SageOpts.Cmd = SageCmd.delmodel;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-ic":
                        {
                            SageOpts.Cmd = SageCmd.importcatalog;
                            if (sar.Length > 1)
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-iu":
                        {
                            SageOpts.Cmd = SageCmd.importusage;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-bm":
                        {
                            SageOpts.Cmd = SageCmd.buildmodel;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-grm":
                        {
                            SageOpts.Cmd = SageCmd.getrecos;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-qb":
                    case "-qbm":
                        {
                            SageOpts.Cmd = SageCmd.buildstatus;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-qc":
                    case "-qcm":
                        {
                            SageOpts.Cmd = SageCmd.getcatalog;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-gb":
                    case "-gbs":
                    case "-gbus":
                        {
                            SageOpts.Cmd = SageCmd.getuserbuildsstatus;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-qn":
                        {
                            SageOpts.Cmd = SageCmd.notifications;
                            SageOpts.ModelName = sar1;
                            break;
                        }
                    case "-fp":
                        {
                            SageOpts.Filepath = sar1;
                            break;
                        }
                    case "-bd":
                        {
                            SageOpts.Builddesc = sar1;
                            break;
                        }
                    case "-sl":
                    case "-rsl":
                        {
                            SageOpts.Seedlist = sar1;
                            break;
                        }
                    case "-slx":
                    case "-rslx":
                        {
                            SageOpts.Seedlist = "2406e770-769c-4189-89de-1c9283f93a96,552a1940-21e4-4399-82bb-594b46d7ed54";
                            break;
                        }
                    case "-ul":
                    case "-rul":
                        {
                            SageOpts.UseList = true;
                            break;
                        }
                    case "-sus":
                        {
                            SageOpts.Cmd = SageCmd.setuser;
                            SageOpts.User = sar1;
                            break;
                        }
                    case "-sak":
                        {
                            SageOpts.Cmd = SageCmd.setaccountkey;
                            SageOpts.Akey = sar1;
                            break;
                        }
                    case "-nr":
                        {
                            SageOpts.Nrecs = Utils.ParseInt(sar1, SageOpts.Nrecs);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                lastarg = arg;
                if (mode >= 2) mode = 0;
            }
        }
    }
    public class SageCmdLineProcessor
    {
        private SageOpts sopt;
        public SageCmdLineProcessor(SageOpts sageOpts)
        {
            this.sopt = sageOpts;
        }
        private bool NeedsCredentials(SageCmd cmd)
        {
            if (cmd == SageCmd.deluserinfo) return false;
            if (cmd == SageCmd.getuserinfo) return false;
            if (cmd == SageCmd.setuser) return false;
            if (cmd == SageCmd.setaccountkey) return false;
            return true;
        }
        private void ErrorOut(string emsg)
        {
            System.Console.WriteLine(emsg);
        }
        public bool ProcessCommand()
        {
            bool rv = true;
            if (sopt.Verbose)
            {
                Console.WriteLine(sopt.VersionString);
            }
            Console.WriteLine("Processing with command:{0}", sopt.Cmd);
            var sage = new SageService();
            var modelList = new List<SageMan.Model>();
            if (NeedsCredentials(sopt.Cmd))
            {
                rv = sage.SetCredentials(sopt.Verbose);
                if (!rv)
                {
                    Console.WriteLine("Bad User Credentials User:{0} AccountKey:{1}", sage.User, sage.AccountKey);
                    return rv;
                }
                modelList = sage.GetInventory();
                if (modelList == null) return rv;
            }
            string command = sopt.Cmd.ToString();
            switch (sopt.Cmd)
            {
                default:
                case SageCmd.getuserinfo:
                    {
                        Console.WriteLine("         us:{0} ", sage.User);
                        Console.WriteLine("         ak:{0} ", sage.AccountKey);
                        break;
                    }
                case SageCmd.deluserinfo:
                    {
                        sage.SetUser("");
                        sage.SetAccountKey("");
                        break;
                    }
                case SageCmd.setuser:
                    {
                        sage.SetUser(sopt.User);
                        break;
                    }
                case SageCmd.setaccountkey:
                    {
                        sage.SetAccountKey(sopt.Akey);
                        break;
                    }
                case SageCmd.getmodels:
                    {
                        int imod = 0;
                        foreach (var model in modelList)
                        {
                            if (sopt.ModelName == "" || sopt.ModelName == model.Name)
                            {
                                Console.WriteLine("   " + imod + " " + model);
                                if (sopt.Verbose || sopt.DoAll)
                                {
                                    Console.WriteLine("         id:{0} ", model.Id);
                                    Console.WriteLine("       date:{0} ", model.Date);
                                    Console.WriteLine("       stat:{0} ", model.Status);
                                    Console.WriteLine("        bid:{0} ", model.BuildId);
                                    Console.WriteLine("      usefn:{0} ", model.UsageFileNames);
                                    Console.WriteLine("        cid:{0} ", model.CatalogId);
                                    Console.WriteLine("       desc:{0} ", model.Description);
                                    Console.WriteLine("      catfn:{0} ", model.CatalogFileName);
                                }
                            }
                            imod++;
                        }
                        break;
                    }
                case SageCmd.createmodel:
                    {
                        if (sopt.Execute)
                        {
                            Console.WriteLine("creating model:{0}", sopt.ModelName);
                            var mid = sage.CreateModel(sopt.ModelName);
                            Console.WriteLine("Created model:{0} modelid:{1}", sopt.ModelName, mid);
                        }
                        else
                        {
                            Console.WriteLine("create mode:{0}  -x not specified so not creating", sopt.ModelName);
                        }
                        break;
                    }
                case SageCmd.delmodel:
                    {
                        if (!sage.chkmname(sopt.ModelName)) return false;
                        string mid = sage.MidMap[sopt.ModelName];
                        if (mid != "")
                        {
                            if (sopt.Execute)
                            {
                                Console.WriteLine("delmodel:{0}", sopt.ModelName);
                                sage.DeleteModel(mid);
                            }
                            else
                            {
                                Console.WriteLine("delmodel:{0}  -x not specified so not deleting", sopt.ModelName);
                            }
                        }
                        break;
                    }
                case SageCmd.importcatalog:
                    {
                        if (!sage.chkmname(sopt.ModelName)) return false;
                        var res = "";
                        string mid = sage.MidMap[sopt.ModelName];
                        if (mid != "")
                        {
                            if (sopt.Execute)
                            {
                                Console.WriteLine("importcat:{0}", sopt.ModelName);
                                res = sage.ImportCatalog(mid, sopt.Filepath);
                            }
                            else
                            {
                                var exists = File.Exists(sopt.Filepath);
                                Console.WriteLine("importcat:{0} file:{1} exists:{3} -x not specified so not executing",
                                                    sopt.ModelName, sopt.Filepath, exists);
                            }
                        }
                        Console.WriteLine("Imported catalog:{0} modelid:{1}", sopt.ModelName, mid);
                        Console.WriteLine(res);
                        break;
                    }
                case SageCmd.importusage:
                    {
                        if (!sage.chkmname(sopt.ModelName)) return false;
                        var res = "";
                        string mid = sage.MidMap[sopt.ModelName];
                        if (mid != "")
                        {
                            if (sopt.Execute)
                            {
                                Console.WriteLine("importuseage:{0}", sopt.ModelName);
                                res = sage.ImportUsage(mid, sopt.Filepath);
                            }
                            else
                            {
                                var exists = File.Exists(sopt.Filepath);
                                Console.WriteLine("importusage:{0} file:{1} exists:{3} -x not specified so not executing",
                                                    sopt.ModelName, sopt.Filepath, exists);
                            }
                        }
                        Console.WriteLine("Imported usage:{0} modelid:{1}", sopt.ModelName, mid);
                        Console.WriteLine(res);
                        break;
                    }
                case SageCmd.buildmodel:
                    {
                        if (!sage.chkmname(sopt.ModelName)) return false;
                        if (sopt.Builddesc == "")
                        {
                            ErrorOut("No Build Description specified");
                            return false;
                        }
                        var res = "";
                        string mid = sage.MidMap[sopt.ModelName];
                        if (mid != "")
                        {
                            if (sopt.Execute)
                            {
                                Console.WriteLine("buildmodel:{0}", sopt.ModelName);
                                res = sage.BuildModel(mid, sopt.Builddesc);
                            }
                            else
                            {
                                Console.WriteLine("buildmodel:{0} -x not specified so not executing", sopt.ModelName);
                            }
                        }
                        Console.WriteLine("Building Model:{0}  buildid:{1}", sopt.ModelName, res);
                        break;
                    }
                case SageCmd.buildstatus:
                    {
                        if (!sage.chkmname(sopt.ModelName)) return false;
                        var res = "";
                        Model model = sage.ModelMnmMap[sopt.ModelName];
                        string mid = model.Id;
                        string bid = model.BuildId;
                        if (mid != "" && bid != "")
                        {
                            res = sage.GetBuidStatus(mid, bid).ToString();
                            Console.WriteLine("Build Status for:{0} bid:{1} stat:{2}", sopt.ModelName, bid, res);
                        }
                        break;
                    }
                case SageCmd.getcatalog:
                    {
                        if (!sage.chkmname(sopt.ModelName)) return false;

                        var mid = sage.MidMap[sopt.ModelName];
                        var catList = sage.GetCatalog(mid);
                        Console.WriteLine("{0} Catalog items returned for {1}", catList.Count, sopt.ModelName);
                        foreach (var catit in catList)
                        {
                            Console.WriteLine(catit);
                        }
                        break;
                    }
                case SageCmd.notifications:
                    {
                        string mid = "";
                        if (sopt.ModelName != "")
                        {
                            mid = sage.MidMap[sopt.ModelName];
                        }
                        var noteList = sage.GetNotifications(mid);
                        Console.WriteLine("{0} Notifications returned for {1}", noteList.Count, sopt.ModelName);
                        foreach (var note in noteList)
                        {
                            Console.WriteLine(note);
                        }
                        break;
                    }
                case SageCmd.getrecos:
                    {
                        if (!sage.chkmname(sopt.ModelName)) return false;
                        var sar = sopt.Seedlist.Split(',');
                        var itemList = new List<string>(sar);
                        var mid = sage.MidMap[sopt.ModelName];
                        if (sopt.Verbose)
                        {
                            Console.WriteLine("ItemList:{0} UseList:{1}", itemList.Count, sopt.UseList);
                            foreach (var item in itemList)
                            {
                                Console.WriteLine(" {0}", item);
                            }
                        }
                        if (mid != "")
                        {
                            var results = sage.GetRecommendation(mid, itemList, sopt.Nrecs, sopt.UseList);
                            int i = 0;
                            foreach (var ri in results)
                            {
                                var frate = Utils.ParseDouble(ri.Rating, 0);
                                //Console.WriteLine(i + " id:" + ri.Id  + " "+ ri.Name + " rating:" + frate.ToString("f5") + " reason:" + ri.Reasoning);
                                Console.WriteLine("{0} id:{1} {2}  rating:{3}", i, ri.Id, ri.Name, frate.ToString("f5"));
                                i++;
                            }
                        }
                        break;
                    }
                case SageCmd.getuserbuildsstatus:
                    {
                        var bldList = sage.GetUserBuildsStatus(sopt.DoAll);
                        foreach (var binst in bldList)
                        {
                            Console.WriteLine(binst);
                        }
                        break;
                    }
            }
            return rv;
        }
    }
}