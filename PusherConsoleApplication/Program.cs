﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FlowLibrary;
using NDesk.Options;

namespace PusherConsoleApplication
{

    class Program
    {

        static void Main(string[] args)
        {
            string sourcePath = null;
            string targetPath = null;
            string filePattern = null;
            FileAttributes fileAttributes = FileAttributes.Normal;
            bool help = false;

            // Create the options.
            OptionSet optionSet = new OptionSet();
            optionSet.Add("?", "Displays this help message.", v => help = true);
            optionSet.Add("sd:", "Source Directory.", v => sourcePath = v);
            optionSet.Add("td:", "Target Directory.", v => targetPath = v);
            optionSet.Add("fp:", @"File Pattern. Optional. Defaults to ""*.*"".", v => filePattern = v);
            optionSet.Add("fa:", @"File Attributes. Optional. Comma delimited list. Defaults to ""Any"".",
                v => fileAttributes = v == null ? FileAttributes.Normal :
                (FileAttributes)Enum.Parse(typeof(FileAttributes), v));

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
                if (targetPath == null)
                {
                    consoleWriter.WriteError("Target Directory not specified.");
                    consoleWriter.WriteInformation(GetForHelpMessage());
                    return;
                }
                if (sourcePath == null)
                {
                    consoleWriter.WriteError("Source Directory not specified.");
                    consoleWriter.WriteInformation(GetForHelpMessage());
                    return;
                }
                var fileSystem = new FileSystem();
                Pusher.PushDirectory(fileSystem, sourcePath, targetPath, filePattern, fileAttributes);
            }
        }

        private static string GetHelpMessage(OptionSet optionSet)
        {

            List<string> optionSyntaxes = new List<string>();
            optionSyntaxes.AddRange(optionSet.Where(o => o.Prototype != "?").Select(GetOptionSyntax));            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Pushes the contents of a source directory to a target directory.");
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
