using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SynchronisationService.Configuration
{
    [ConfigurationCollection(typeof(SynchroniserElement))]
    internal class SynchroniserCollection : ConfigurationElementCollection
    {

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new SynchroniserElement();
        }

        protected override string ElementName
        {
            get
            {
                return "Synchroniser";
            }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SynchroniserElement)(element)).Name;
        }

        protected override bool IsElementName(string elementName)
        {
            return ElementName.Equals(elementName);
        }
        
    }
}
