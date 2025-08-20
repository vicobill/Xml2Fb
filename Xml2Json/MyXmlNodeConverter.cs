using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb
{
    public class MyXmlNodeConverter : XmlNodeConverter
    {
        public override bool CanRead => base.CanRead;

        public override bool CanWrite => base.CanWrite;

        public override bool CanConvert( Type valueType )
        {
            return base.CanConvert( valueType );
        }

        public override bool Equals( object? obj )
        {
            return base.Equals( obj );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override object? ReadJson( JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer )
        {
            return base.ReadJson( reader, objectType, existingValue, serializer );
        }

        public override string? ToString()
        {
            return base.ToString();
        }

        public override void WriteJson( JsonWriter writer, object? value, JsonSerializer serializer )
        {
            base.WriteJson( writer, value, serializer );
        }

        protected override bool IsNodeAsArray( string nodeNamePath )
        {
            string lower = nodeNamePath.ToLower();
            if (SchemaGenerator.IsNodeAsArray( nodeNamePath ))
                return true;
            
            return base.IsNodeAsArray( nodeNamePath );
        }
    }
}
