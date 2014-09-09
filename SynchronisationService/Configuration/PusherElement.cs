using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace SynchronisationService.Configuration
{
    internal class PusherElement : ConfigurationElement
    {

        [ConfigurationProperty("Name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return (string)this["Name"];
            }
            set
            {
                this["Name"] = value;
            }
        }

        [ConfigurationProperty("SourcePath", IsRequired = true)]
        public string SourcePath
        {
            get
            {
                return (string)this["SourcePath"];
            }
            set
            {
                this["SourcePath"] = value;
            }
        }

        [ConfigurationProperty("TargetPath", IsRequired = true)]
        public string TargetPath
        {
            get
            {
                return (string)this["TargetPath"];
            }
            set
            {
                this["TargetPath"] = value;
            }
        }

        [ConfigurationProperty("Pattern", DefaultValue = "*.*")]
        public string Pattern
        {
            get
            {
                return (string)this["Pattern"];
            }
            set
            {
                this["Pattern"] = value;
            }
        }

        [ConfigurationProperty("Attributes", DefaultValue = FileAttributes.Normal)]
        public FileAttributes Attributes
        {
            get
            {
                return (FileAttributes)this["Attributes"];
            }
            set
            {
                this["Attributes"] = value;
            }
        }
        
    }
}
