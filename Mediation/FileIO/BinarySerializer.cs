using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Mediation.FileIO
{
    public static class BinarySerializer
    {
        public static void SerializeObject<T>(string filename, T obj)
        {
            Stream stream = WaitForFile(filename, FileMode.Create);
            if (stream != null)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, obj);
                stream.Close();
            }
        }

        public static T DeSerializeObject<T>(string filename)
        {
            T objectToBeDeSerialized;
            Stream stream = WaitForFile(filename, FileMode.Open);
            if (stream != null)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                objectToBeDeSerialized = (T)binaryFormatter.Deserialize(stream);
                stream.Close();
                return objectToBeDeSerialized;
            }
            return default(T);
        }

        public static Stream WaitForFile(string fullPath, FileMode mode)
        {
            for (int numTries = 0; numTries < 10; numTries++)
            {
                try
                {
                    Stream stream = File.Open(fullPath, mode);
                    return stream;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                }
            }

            return null;
        }
    }
}
