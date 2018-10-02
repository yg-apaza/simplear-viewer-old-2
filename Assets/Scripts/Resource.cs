using Firebase.Database;

[System.Serializable]
public class Resource
{
    public string id;
    public string description;
    public string content;
    public string type;

    public Resource(string id, string description, string content, string type)
    {
        this.id = id;
        this.description = description;
        this.content = content;
        this.type = type;
    }

    public static Resource FromDataSnapshot(DataSnapshot snapshot)
    {
        string id = (string)snapshot.Child("id").Value;
        string description = (string)snapshot.Child("description").Value;
        string content = (string)snapshot.Child("content").Value;
        string type = (string)snapshot.Child("type").Value;

        return new Resource(id, description, content, type);
    }
}