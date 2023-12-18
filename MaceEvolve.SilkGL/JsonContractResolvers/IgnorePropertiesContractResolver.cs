using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MaceEvolve.SilkGL.JsonContractResolvers
{
    public class IgnorePropertiesContractResolver : DefaultContractResolver
    {
        public IEnumerable<string> PropertyNamesToIgnore { get; set; }
        public bool IsCaseSensitive { get; set; }
        public IgnorePropertiesContractResolver()
        {
            PropertyNamesToIgnore = Enumerable.Empty<string>();
        }
        public IgnorePropertiesContractResolver(IEnumerable<string> propertyNamesToIgnore)
        {
            PropertyNamesToIgnore = propertyNamesToIgnore;
        }
        public IgnorePropertiesContractResolver(params string[] propertyNamesToIgnore)
        {
            PropertyNamesToIgnore = propertyNamesToIgnore;
        }
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            foreach (var propertyNameToIgnore in PropertyNamesToIgnore)
            {
                bool doPropertyNamesMatch;

                if (IsCaseSensitive)
                {
                    doPropertyNamesMatch = propertyNameToIgnore == property.PropertyName;
                }
                else
                {
                    doPropertyNamesMatch = string.Equals(propertyNameToIgnore, property.PropertyName, StringComparison.InvariantCultureIgnoreCase);
                }

                if (doPropertyNamesMatch)
                {
                    property.ShouldSerialize = _ => false;
                    property.ShouldDeserialize = _ => false;
                    break;
                }
            }

            return property;
        }
    }
}
