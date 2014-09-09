using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SynchronisationService;
using FlowLibrary;

namespace SynchronisationService.Configuration
{
    internal class ServiceSection : ConfigurationSection
    {

        [ConfigurationProperty("Pushers")]
        public PusherCollection Pushers
        {
            get { return ((PusherCollection)(base["Pushers"])); }
            set { base["Pushers"] = value; }
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
