// See https://aka.ms/new-console-template for more information


string file = args[0];
string output = args.Length > 1 ? args[1] : "kv.txt";

Dictionary<string, string> kvs = new Dictionary<string, string>();

{
    var lines = await File.ReadAllLinesAsync( file );
    foreach (var line in lines)
    {
        if (string.IsNullOrEmpty( line ))
            continue;
        if (line.IndexOf( ':' ) == -1)
            continue;
        string[] words = line.Split( ':' );
        if (words.Length < 2)
            continue;

        string key = words[0].Trim( ' ' );
        string val = words[1].Trim( ' ', ';' );
        if (kvs.ContainsKey( key ))
        {
            val = PriorType( kvs[key], val );
        }

        kvs[words[0].Trim( ' ' )] = val;
    }

}

bool is_int_type( string type ) => type == "int" || type == "uint";
bool is_float_type( string type ) => type == "float" || type == "double";
bool is_long_type( string type ) => type == "long" || type == "ulong";
bool is_number_type( string type ) => is_int_type( type ) || is_float_type( type ) || is_long_type( type );
bool is_string_type( string type ) => type == "string";
bool is_scalar_type( string type ) => is_number_type( type ) || is_string_type( type );
bool is_vector_type( string type ) => type.StartsWith( '[' ) && type.EndsWith( "]" );

string PriorType( string old, string @new )
{
    if (is_int_type( old ) && is_long_type( @new ))
        return @new;
    if (is_int_type( old ) && is_float_type( @new ))
        return @new;
    if (is_number_type( old ) && is_string_type( @new ))
        return @new;
    if (is_scalar_type( old ) && is_vector_type( @new ))
        return @new;

    return old;
}

{
    List<string> lines = new List<string>();


    foreach (var kv in kvs)
    {
        lines.Add( $"{kv.Key} : {kv.Value};" );
    }

    await File.WriteAllLinesAsync( "kv.txt", lines );
}
