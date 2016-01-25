using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;

/*** 
 * Change log:
 * ------------------
 * Dec 30, 2012 : Merged in user submitted patch (ID: 10344) to support non-English builds. Thanks jenda__. 
 * 
 */

namespace ScpControl.Driver.PNPUtilLib
{
    public class PnpUtil : IDriverStore
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool PnpUtilHelper(PnpUtilOptions option, string infName, ref string output)
        {
            var retVal = true;
            var fDebugPrintOutput = false;
            //
            // Setup the process with the ProcessStartInfo class.
            //
            var start = new ProcessStartInfo
            {
                FileName = @"pnputil.exe" /* exe name.*/,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            switch (option)
            {
                //
                // [jenda_] I also had problems with some arguments starting "-". "/" works fine
                //
                case PnpUtilOptions.Enumerate:
                    start.Arguments = @"/e";
                    break;
                case PnpUtilOptions.Delete:
                    start.Arguments = @"/d " + infName;
                    break;
                case PnpUtilOptions.ForceDelete:
                    start.Arguments = @"/f /d " + infName;
                    break;
                case PnpUtilOptions.Add:
                    fDebugPrintOutput = true;
                    start.WorkingDirectory = Path.GetDirectoryName(infName);
                    start.Arguments = @"/a " + Path.GetFileName(infName);
                    Log.DebugFormat("[Add] workDir = {0}, arguments = {1}", start.WorkingDirectory,
                        start.Arguments);
                    break;
                case PnpUtilOptions.AddInstall:
                    fDebugPrintOutput = true;
                    start.WorkingDirectory = Path.GetDirectoryName(infName);
                    start.Arguments = @"/i /a " + Path.GetFileName(infName);
                    Log.DebugFormat("[AddInstall] workDir = {0}, arguments = {1}", start.WorkingDirectory,
                        start.Arguments);

                    break;
            }


            //
            // Start the process.
            //
            var result = "";
            try
            {
                using (var process = Process.Start(start))
                {
                    //
                    // Read in all the text from the process with the StreamReader.
                    //
                    using (var reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                        output = result;
                        if (fDebugPrintOutput)
                            Log.DebugFormat("[Result_start] ---- {0}{1}[----- Result_End]{0}", Environment.NewLine,
                                result);

                        if (option == PnpUtilOptions.Delete || option == PnpUtilOptions.ForceDelete)
                        {
                            // [jenda_] Really don't know, how to recognize error without language-specific string recognition :(
                            // [jenda_] But those errors should contain ":"
                            if (output.Contains(@":")) //"Deleting the driver package failed"
                            {
                                retVal = false;
                            }
                        }

                        if ((option == PnpUtilOptions.Add || option == PnpUtilOptions.AddInstall))
                        {
                            /* [jenda_]
                             This regex should recognize (~) this pattern:
                             * MS PnP blah blah
                             * 
                             * blah blah blah
                             * blah blah (...)
                             * 
                             * blah blah:    *number*
                             * blah blah blah:    *number*
                             * 
                             */
                            var MatchResult = Regex.Match(output, @".+: +([0-9]+)[\r\n].+: +([0-9]+)[\r\n ]+",
                                RegexOptions.Singleline);

                            if (MatchResult.Success) // [jenda_] regex recognized successfully
                            {
                                // [jenda_] if trying to add "0" packages or if number packages and number added packages differs
                                if (MatchResult.Groups[1].Value == "0" ||
                                    MatchResult.Groups[1].Value != MatchResult.Groups[2].Value)
                                {
                                    Log.ErrorFormat("[Error] failed to add " + infName);
                                    retVal = false;
                                }
                            }
                            else
                            {
                                Log.ErrorFormat("[Error] unknown response while trying to add " + infName);
                                retVal = false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // dont catch all exceptions -- but will do for our needs!
                Log.ErrorFormat(@"{0}\n{1}" + Environment.NewLine, e.Message, e.StackTrace);
                output = "";
                retVal = false;
            }
            return retVal;
        }

        private enum PnpUtilOptions
        {
            Enumerate,
            Delete,
            ForceDelete,
            Add,
            AddInstall
        };

        #region IDriverStore Members

        public List<DriverStoreEntry> EnumeratePackages()
        {
            var ldse = new List<DriverStoreEntry>();
            var output = "";

            var result = PnpUtilHelper(PnpUtilOptions.Enumerate, "", ref output);
            if (result)
            {
                // Trace.TraceInformation("O/P of Enumeration : " + Environment.NewLine + output + Environment.NewLine);

                // Parse the output
                // [jenda_] Didn't work on non-english Windows - changed from string recognition to counting lines
                using (var sr = new StringReader(output))
                {
                    var currentLine = "";
                    string[] currentLineDivided = {};
                    var dse = new DriverStoreEntry();
                    byte lineNum = 0;
                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        currentLineDivided = currentLine.Split(':');
                        if (currentLineDivided.Length == 2)
                        {
                            currentLineDivided[1] = currentLineDivided[1].Trim();
                            switch (lineNum)
                            {
                                case 0: // [jenda_] Published name :
                                    dse.DriverPublishedName = currentLineDivided[1];
                                    break;
                                case 1: //Driver package provider :
                                    dse.DriverPkgProvider = currentLine.Split(':')[1].Trim();
                                    break;
                                case 2: // [jenda_] Class :
                                    dse.DriverClass = currentLine.Split(':')[1].Trim();
                                    break;
                                case 3: // [jenda_] Driver date and version :
                                    var DateAndVersion = currentLine.Split(':')[1].Trim();
                                    // date and version may be empty
                                    if(DateAndVersion.Length > 0)
                                    {
                                        dse.DriverDate = DateAndVersion.Split(' ')[0].Trim();
                                        dse.DriverVersion = DateAndVersion.Split(' ')[1].Trim();
                                    }
                                    break;
                                case 4: // [jenda_] Signer name :
                                    dse.DriverSignerName = currentLine.Split(':')[1].Trim();

                                    ldse.Add(dse);
                                    dse = new DriverStoreEntry();
                                    break;
                                default:
                                    continue;
                            }
                            lineNum++;
                            if (lineNum > 4)
                                lineNum = 0;
                        }
                    }
                }
            }
            return ldse;
        }

        public bool DeletePackage(DriverStoreEntry dse, bool forceDelete)
        {
            var dummy = "";
            return PnpUtilHelper(forceDelete ? PnpUtilOptions.ForceDelete : PnpUtilOptions.Delete,
                dse.DriverPublishedName,
                ref dummy);
        }

        public bool AddPackage(string infFullPath, bool install)
        {
            var dummy = "";
            return PnpUtilHelper(install ? PnpUtilOptions.AddInstall : PnpUtilOptions.Add,
                infFullPath, ref dummy);
        }

        #endregion
    }
}