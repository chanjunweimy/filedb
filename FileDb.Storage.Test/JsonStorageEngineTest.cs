using FileDb.Storage.Test.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileDb.Storage.Test
{
    [TestClass]
    public class JsonStorageEngineTest
    {
        private HelperManager _helper;
        private string _folderPath;
        private JsonStorageEngine _jsonStorageEngine;

        [TestInitialize]
        public void TestInitialize()
        {
            _helper = HelperManager.Instance;
            _folderPath = Path.Combine(Directory.GetCurrentDirectory(), "JsonStorageEngineTest");
            _helper.CreateFolder(_folderPath);
            _jsonStorageEngine = new JsonStorageEngine(_folderPath);
        }

        [TestMethod]
        public void JsonStorageEngineTest_NormalTest()
        {
            var fileName = "file1.txt";
            var filePath = Path.Combine(_folderPath, fileName);
            Assert.IsFalse(File.Exists(filePath));

            var firstRecord = new DummyStorageObject { Id = 1, Name = "name" };
            _jsonStorageEngine.AppendToFile(fileName, firstRecord);

            Assert.IsTrue(File.Exists(filePath));

            var text = File.ReadAllText(filePath);
            Assert.AreEqual(string.Format("[{{\"Id\":{0},\"Name\":\"{1}\"}}]", firstRecord.Id, firstRecord.Name), text);

            var records = _jsonStorageEngine.ReadFile<DummyStorageObject>(fileName);
            Assert.AreEqual(1, records.Count());

            var actualRecord = records.ElementAt(0);
            Assert.AreEqual(firstRecord.Id, actualRecord.Id);
            Assert.AreEqual(firstRecord.Name, actualRecord.Name);
        }

        [TestMethod]
        public void JsonStorageEngineTest_SaveParallelTest()
        {
            var fileName = "fileWriteParallel.txt";
            var filePath = Path.Combine(_folderPath, fileName);
            Assert.IsFalse(File.Exists(filePath));

            var records = new List<DummyStorageObject>
            {
                new DummyStorageObject { Id = 1, Name = "name" },
                new DummyStorageObject { Id = 2, Name = "name2" }
            };

            records.AsParallel().ForAll(record => _jsonStorageEngine.AppendToFile(fileName, record));
            

            Assert.IsTrue(File.Exists(filePath));

            var text = File.ReadAllText(filePath);
            Assert.AreEqual(string.Format("[{{\"Id\":{0},\"Name\":\"{1}\"}},{{\"Id\":{2},\"Name\":\"{3}\"}}]",
                records[0].Id, records[0].Name, records[1].Id, records[1].Name), text);
        }

        [TestMethod]
        public void JsonStorageEngineTest_ReadParallelTest()
        {
            var fileName = "fileReadParallel.txt";
            var filePath = Path.Combine(_folderPath, fileName);
            Assert.IsFalse(File.Exists(filePath));

            var firstRecord = new DummyStorageObject { Id = 1, Name = "name" };
            _jsonStorageEngine.AppendToFile(fileName, firstRecord);

            Assert.IsTrue(File.Exists(filePath));

            var text = File.ReadAllText(filePath);
            Assert.AreEqual(string.Format("[{{\"Id\":{0},\"Name\":\"{1}\"}}]", firstRecord.Id, firstRecord.Name), text);

            Enumerable.Range(0, 5).AsParallel().ForAll(i =>
            {
                var records = _jsonStorageEngine.ReadFile<DummyStorageObject>(fileName);
                Assert.AreEqual(1, records.Count());

                var actualRecord = records.ElementAt(0);
                Assert.AreEqual(firstRecord.Id, actualRecord.Id);
                Assert.AreEqual(firstRecord.Name, actualRecord.Name);
            });            
        }

        [TestMethod]
        public void JsonStorageEngineTest_ReadWriteParallelTest()
        {
            var fileName = "fileReadWriteParallel.txt";
            var filePath = Path.Combine(_folderPath, fileName);
            Assert.IsFalse(File.Exists(filePath));

            var originalRecords = new List<DummyStorageObject>
            {
                new DummyStorageObject { Id = 1, Name = "name" },
                new DummyStorageObject { Id = 2, Name = "name2" }
            };

            Enumerable.Range(0, 5).AsParallel().ForAll(i =>
            {
                if (i < originalRecords.Count) {
                    _jsonStorageEngine.AppendToFile(fileName, originalRecords[i]);
                }
                else
                {
                    _jsonStorageEngine.ReadFile<DummyStorageObject>(fileName);
                }
            });

            Assert.IsTrue(File.Exists(filePath));

            var text = File.ReadAllText(filePath);
            Assert.AreEqual(string.Format("[{{\"Id\":{0},\"Name\":\"{1}\"}},{{\"Id\":{2},\"Name\":\"{3}\"}}]",
                originalRecords[0].Id, originalRecords[0].Name, originalRecords[1].Id, originalRecords[1].Name), text);
        }
    }

    public class DummyStorageObject
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
