using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SynchronisationLibrary;
using NDesk.Options;
using WrapperLibrary;

namespace PusherConsoleApplication
{

    class Program
    {

        static void Main(string[] args)
        {
            DirectoryWrapper sourceDirectory = null;
            DirectoryWrapper targetDirectory = null;
            string sourceFilePattern = null;
            FileAttributesWrapper sourceFileAttributes = FileAttributesWrapper.Any;
            bool help = false;

            // Create the options.
            OptionSet optionSet = new OptionSet();
            optionSet.Add("?", "Displays this help message.", v => help = true);
            optionSet.Add("sd:", "Source Directory.", v => sourceDirectory = new DirectoryWrapper(v));
            optionSet.Add("td:", "Target Directory.", v => targetDirectory = new DirectoryWrapper(v));
            optionSet.Add("sfp:", @"Source File Pattern. Optional. Defaults to ""*.*"".", v => sourceFilePattern = v);
            optionSet.Add("sfa:", @"Source File Attributes. Optional. Comma delimited list. Defaults to ""Any"".",
                v => sourceFileAttributes = v == null ? FileAttributesWrapper.Any :
                (FileAttributesWrapper)Enum.Parse(typeof(FileAttributesWrapper), v));

            //
            List<string> unknownOptions = optionSet.Parse(args);
            using (ConsoleWriter consoleWriter = new ConsoleWriter())
            {
                if (unknownOptions != null && unknownOptions.Any())
                {
                    unknownOptions.ForEach(o => consoleWriter.WriteWarning(string.Format("Option not known: \"{0}\".", o)));
                    consoleWriter.WriteInformation(GetForHelpMessage());
                    return;
                }
                if (help)
                {
                    consoleWriter.WriteInformation(GetHelpMessage(optionSet));
                    return;
                }
                if (targetDirectory == null)
                {
                    consoleWriter.WriteError("Target Directory not specified.");
                    consoleWriter.WriteInformation(GetForHelpMessage());
                    return;
                }
                if (sourceDirectory == null)
                {
                    consoleWriter.WriteError("Source Directory not specified.");
                    consoleWriter.WriteInformation(GetForHelpMessage());
                    return;
                }
                Pusher.Push(sourceDirectory, targetDirectory, sourceFilePattern, sourceFileAttributes);
            }
        }

        private static string GetHelpMessage(OptionSet optionSet)
        {

            List<string> optionSyntaxes = new List<string>();
            optionSyntaxes.AddRange(optionSet.Where(o => o.Prototype != "?").Select(GetOptionSyntax));            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Pushes the contents of a source directory to a target directory.");
            // Updates a backup directory (ie no deletions)
            // Synchronises a target directory with a source one.  (implies deletions)          //  
            // Copies the contents of a source directory to a target directory.
            // SELECTIVE ASPECT IS WHAT I WANT TO CAPTURE, VERIFICATION
            // Writer? Creater/Updater Pusher [ pushes contents of sourcce direcctory to target

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Syntax:");
            stringBuilder.AppendLine(string.Format("\t{0} /? | ({1})", typeof(Program).Assembly.GetName().Name,
                string.Join(" ", optionSyntaxes)));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Arguments:");
            foreach (Option option in optionSet)
            {
                stringBuilder.AppendLine(string.Format("\t/{0}\t{1}", option.Prototype, option.Description));
            }
            return stringBuilder.ToString();
        }
        
        private static string GetOptionSyntax(Option option)
        {
            bool optional = option.Description.Contains("Optional");
            return string.Format("{0}/{1}{2}{3}", (optional ? "[" : ""), option.Prototype,
                    (option.Prototype.EndsWith(":") ? "value" : ""), (optional ? "]" : ""));
        }

        private static string GetForHelpMessage()
        {
            return String.Format("For help, run \"{0} /?\".", typeof(Program).Assembly.GetName().Name);
        }              
    }
}
