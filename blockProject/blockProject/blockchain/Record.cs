using Newtonsoft.Json;

namespace blockProject.blockchain;

public class Record
{
    //public Record() { } // Konstruktor bezparametrowy potrzebny przy deserializacji
    [JsonConstructor]

    public Record() { }
    
    public Record(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; set; } = "";

    public string Value { get; set; }= "";

    public override string ToString()
    {
        return $"{Key}:{Value}";
    }
}