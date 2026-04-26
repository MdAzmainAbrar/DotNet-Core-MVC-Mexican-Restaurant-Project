using System.Text.Json;
namespace MaxicanResturant.Models
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value) // This is an extension method for the ISession interface that allows you to set a value in the session using a specified key. The method takes three parameters: the session object, the key as a string, and the value of type T that you want to store in the session. The method uses JSON serialization to convert the value into a string format before storing it in the session, allowing you to easily store complex objects in the session state. By calling this method, you can save data in the user's session that can be accessed across multiple requests during their interaction with the application.
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key) // This is an extension method for the ISession interface that allows you to retrieve a value from the session using a specified key. The method takes two parameters: the session object and the key as a string. It attempts to get the value associated with the key from the session as a string, and if it is not null or empty, it deserializes the JSON string back into an object of type T and returns it. If the key does not exist in the session or if the value is null or empty, it returns the default value of type T. This method provides a convenient way to retrieve complex objects from the session state by using JSON serialization and deserialization.
        {
            var json = session.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }
            else
            {
                return JsonSerializer.Deserialize<T>(json);
            }
        }
    }
}

