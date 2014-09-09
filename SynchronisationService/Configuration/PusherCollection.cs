using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SynchronisationService.Configuration
{
    [ConfigurationCollection(typeof(PusherElement))]
    internal class PusherCollection : ConfigurationElementCollection
    {

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PusherElement();
        }

        protected override string ElementName
        {
            get
            {
                return "Pusher";
            }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PusherElement)(element)).Name;
        }

        protected override bool IsElementName(string elementName)
        {
            return ElementName.Equals(elementName);
        }
        
    }
}
