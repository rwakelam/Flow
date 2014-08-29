using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynchronisationLibrary;
using NDesk.Options;
using WrapperLibrary;

namespace CleanerConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryWrapper sourceDirectory = null;
            DirectoryWrapper targetDirectory = null;
            string targetFilePattern = null;
            FileAttributesWrapper targetFileAttributes = FileAttributesWrapper.Any;
            bool help = false;

            // Create the command line option set.
            OptionSet optionSet = new OptionSet();
            optionSet.Add("?", "Displays this help message.", v => help = true);
            optionSet.Add("td:", "Target Directory.", v => targetDirectory = new DirectoryWrapper(v));
            optionSet.Add("sd:", "Source Directory.", v => sourceDirectory = new DirectoryWrapper(v));
            optionSet.Add("tfp:", @"Target File Pattern. Optional. Defaults to ""*.*"".", v => targetFilePattern = v ?? "*.*");
            optionSet.Add("tfa:", @"Target File Attributes. Optional. Comma delimited list. Defaults to ""Any"".",
                v => targetFileAttributes = v == null ? FileAttributesWrapper.Any : 
                    (FileAttributesWrapper)Enum.Parse(typeof(FileAttributesWrapper), v));
            List<string> unknownOptions = optionSet.Parse(args);

            //
            using(ConsoleWriter consoleWriter = new ConsoleWriter())
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
                Cleaner.Clean(targetDirectory, sourceDirectory, targetFilePattern, targetFileAttributes);
            }        
        }
        
        private static string GetHelpMessage(OptionSet optionSet)
        {
            List<string> optionSyntaxes = new List<string>();
            optionSyntaxes.AddRange(optionSet.Where(o => o.Prototype != "?").Select(GetOptionSyntax));
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Cleans a directory of files which are not found in another directory.");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Syntax:");
            stringBuilder.AppendLine(string.Format("\t{0} /? | ({1})", typeof(Program).Assembly.GetName().Name, 
                string.Join(" ", optionSyntaxes)));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Arguments:");          
            foreach(Option option in optionSet)
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
