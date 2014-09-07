using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using FlowLibrary;

namespace SynchronisationService.Configuration
{
    internal class SynchroniserElement: ConfigurationElement
    {

        public SynchroniserElement() 
        { }
        
        [ConfigurationProperty("ClientDirectory", IsRequired = true)]
        public string ClientDirectory
        {
            get
            {
                return (string)this["ClientDirectory"];
            }
            set
            {
                this["ClientDirectory"] = value;
            }
        }

        [ConfigurationProperty("DeleteUnmatchedEnabled", DefaultValue = Synchroniser.Default.DeleteUnmatchedEnabled)]
        public bool DeleteUnmatchedEnabled
        {
            get
            {
                return (bool)this["DeleteUnmatchedEnabled"];
            }
            set
            {
                this["DeleteUnmatchedEnabled"] = value;
            }
        }
        
        [ConfigurationProperty("FilePattern", DefaultValue = null)]
        public string FilePattern
        {
            get
            {
                return (string)this["FilePattern"];
            }
            set
            {
                this["FilePattern"] = value;
            }
        }
        
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

        [ConfigurationProperty("ServerDirectory", IsRequired = true)]
        public string ServerDirectory
        {
            get
            {
                return (string)this["ServerDirectory"];
            }
            set
            {
                this["ServerDirectory"] = value;
            }
        }

    }
}
