using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SynchronisationService;
using SynchronisationLibrary;

namespace SynchronisationService.Configuration
{
    internal class ServiceSection : ConfigurationSection
    {

        [ConfigurationProperty("Synchronisers")]
        public SynchroniserCollection Synchronisers
        {
            get { return ((SynchroniserCollection)(base["Synchronisers"])); }
            set { base["Synchronisers"] = value; }
        }
        
        [ConfigurationProperty("Mode", DefaultValue = SynchronisationService.DefaultMode)]
        public SynchronisationService.Mode Mode
        {
            get { return ((SynchronisationService.Mode)(base["Mode"])); }
            set { base["Mode"] = value; }
        }  

        [ConfigurationProperty("TimerInterval", DefaultValue = SynchronisationService.DefaultTimerInterval)]
        public double TimerInterval
        {
            get { return ((double)(base["TimerInterval"])); }
            set { base["TimerInterval"] = value; }
        }
        
        [ConfigurationProperty("LoggingLevel", DefaultValue = SubscriptionLevel.Synchroniser)]
        public SubscriptionLevel LoggingLevel
        {
            get { return ((SubscriptionLevel)(base["LoggingLevel"])); }
            set { base["LoggingLevel"] = value; }
        }

    }
}
