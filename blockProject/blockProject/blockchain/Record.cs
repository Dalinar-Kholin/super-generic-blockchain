using System.Text.Json.Serialization;

namespace blockProject.blockchain;

public class Record
{
    public Record(string key, string value)
    {
        id = Guid.NewGuid();
        Key = key;
        Value = value;
    }

    public Guid id;
    public string Key { get; set; }
    
    public string Value { get; set; }

    public override string ToString()
    {
        return $"{Key}:{Value}, guid = {id}";
    }
}