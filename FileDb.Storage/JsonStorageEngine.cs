using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileDb.Storage
{
    public class JsonStorageEngine
    {
        private string StoragePath { get; }
        public JsonStorageEngine(string storagePath)
        {
            StoragePath = storagePath;
            CreateFolderIfNotExists(StoragePath);
        }

        public void AppendToFile<T>(string fileName, T record)
        {
            var waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, Path.GetFileNameWithoutExtension(fileName));
            waitHandle.WaitOne();

            var jsonArray = ReadFileWithoutCS<T>(fileName).ToList();
            var path = Path.Combine(StoragePath, fileName);
            jsonArray.Add(record);
            var newJson = JsonConvert.SerializeObject(jsonArray);
            File.WriteAllText(path, newJson);

            waitHandle.Set();
        }

        public IEnumerable<T> ReadFile<T>(string fileName)
        {
            var waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, Path.GetFileNameWithoutExtension(fileName));
            waitHandle.WaitOne();

            var jsonArray = ReadFileWithoutCS<T>(fileName);

            waitHandle.Set();
            return jsonArray;
        }

        private IEnumerable<T> ReadFileWithoutCS<T>(string fileName)
        {
            IEnumerable<T> jsonArray;
            var path = Path.Combine(StoragePath, fileName);
            if (File.Exists(path))
            {
                string existingJson = File.ReadAllText(path);
                jsonArray = JsonConvert.DeserializeObject<IEnumerable<T>>(existingJson);
            }
            else
            {
                jsonArray = new List<T>();
            }
            return jsonArray;
        }

        private void CreateFolderIfNotExists(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
