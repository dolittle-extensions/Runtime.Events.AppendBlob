using System;
using System.Collections;
using System.Collections.Generic;
using Dolittle.Serialization.Json;
using Dolittle.PropertyBags;
using Dolittle.Collections;
using Dolittle.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dolittle.Runtime.Events.Azure.Store.Persistence
{
    /// <summary>
    /// 
    /// </summary>
    public static class PropertyBagSerializer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyBagAsJson"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static PropertyBag From(string propertyBagAsJson, ISerializer serializer)
        {
            //TODO: this is a hack, so that all our specs don't fail due to loss of type info.
            //We need to revisit types and PropertyBag and serialization.
            //Comparing PropertyBags aside from what they map to is not possible.
            BsonDocument document = BsonDocument.Parse(propertyBagAsJson);
            return PropertyBagBsonSerializer.Deserialize(document);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyBag"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static string Serialize(PropertyBag propertyBag, ISerializer serializer)
        {
            var json = PropertyBagBsonSerializer.Serialize(propertyBag).ToJson();
            return json;
        }

            
        //TODO: this should be in Fundamentals Dolittle.PropertyBags        
        static PropertyBag ToPropertyBag(Dictionary<string,object> dictionary)
        {
            if(dictionary == null)
                return null;

            var values = new NullFreeDictionary<string, object>();

            foreach (var kvp in dictionary )
            {
                if(kvp.Value != null)
                    values.Add(kvp.Key, kvp.Value is Dictionary<string,object> ? (kvp.Value as Dictionary<string,object>).ToPropertyBag() : kvp.Value);
            }
            return new PropertyBag(values);    
        }
    }

    /// <summary>
    /// Defines a set of functions for serializing <see cref="PropertyBag"/> to and from <see cref="BsonDocument"/>
    /// Just copied from the MongoDB implementation
    /// </summary>
    public static class PropertyBagBsonSerializer
    {
        /// <summary>
        /// Serialize a <see cref="BsonDocument"/> to a <see cref="PropertyBag"/>
        /// </summary>
        /// <param name="doc"></param>
        public static PropertyBag Deserialize(BsonDocument doc)
        {
            var bsonAsDictionary = doc.ToDictionary();
            var nonNullDictionary = new NullFreeDictionary<string,object>();
            bsonAsDictionary.ForEach(kvp =>
            {
                if(kvp.Value != null)
                    nonNullDictionary.Add(kvp);
            });
            var propertyBag = new PropertyBag(nonNullDictionary);
            return propertyBag;
        }
        /// <summary>
        /// Serialize a <see cref="PropertyBag"/> to a <see cref="BsonDocument"/>
        /// </summary>
        /// <param name="propertyBag"></param>
        /// <returns></returns>
        public static BsonDocument Serialize(PropertyBag propertyBag)
        {
            var doc = new BsonDocument();
            propertyBag.ForEach(kvp => {
                doc.Add(new BsonElement(kvp.Key, ValueAsBsonValue(kvp.Value)));
            });
            return doc;
        }

        static BsonValue ValueAsBsonValue(object value)
        {
            var valueType = value.GetType();
            if (valueType.IsEnumerable())
            {
                var bsonValue = new BsonArray();
                var enumerableValue = value as IEnumerable;
                foreach (var obj in enumerableValue)
                {
                    if (obj.GetType() == typeof(PropertyBag)) 
                        bsonValue.Add(Serialize((PropertyBag)obj));
                    else 
                        bsonValue.Add(ValueAsBsonValue(obj));
                }
                return bsonValue;
            }
            else if (valueType.Equals(typeof(Guid))) return new BsonBinaryData((Guid)value,GuidRepresentation.CSharpLegacy);
            else if (valueType.Equals(typeof(DateTimeOffset))) return new BsonInt64(((DateTimeOffset)value).UtcTicks);
            else return BsonValue.Create(value);
        }
    }
}