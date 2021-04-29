using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace PhotosManager.Models
{
    // Auteur : Nicolas Chourot
    // Fevrier 2020
    public class JsonSerializer<T>
    {
        static private readonly Mutex mutex = new Mutex();
        private DateTime lastUpdate;
        private List<T> dataList;
        private string FilePath;
        public JsonSerializer()
        {
            lastUpdate = DateTime.Now;
            dataList = new List<T>();
            try
            {
                var idExist = AttributeNameExist("Id");
                if (!idExist)
                    throw new Exception("The class JsonSerializer cannot work with type that does not contain an attribute named Id of type int.");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private object GetAttributeValue(T data, string attributeName)
        {
            return data.GetType().GetProperty(attributeName).GetValue(data, null);
        }
        private void SetAttributeValue(T data, string attributeName, object value)
        {
            data.GetType().GetProperty(attributeName).SetValue(data, value, null);
        }
        private bool AttributeNameExist(string attributeName)
        {
            return (Activator.CreateInstance(typeof(T)).GetType().GetProperty(attributeName) != null);
        }
        private int Id(T data)
        {
            return (int)GetAttributeValue(data, "Id");
        }
        public void Init(string filePath)
        {
            mutex.WaitOne();
            try
            {
                FilePath = filePath;
                if (string.IsNullOrEmpty(FilePath))
                {
                    throw new Exception("FilePath not set exception");
                }
                if (!File.Exists(FilePath))
                {
                    using (StreamWriter sw = File.CreateText(FilePath))
                    {
                        sw.Close();
                    };
                }
                ReadFile();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        private void ReadFile()
        {
            if (dataList != null)
            {
                dataList.Clear();
            }
            try
            {
                using (StreamReader sr = new StreamReader(FilePath))
                {
                    dataList = JsonConvert.DeserializeObject<List<T>>(sr.ReadToEnd());
                }
                if (dataList == null)
                {
                    dataList = new List<T>();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void UpdateFile()
        {
            if (dataList != null)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(FilePath))
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(dataList));
                    }
                    ReadFile();
                    lastUpdate = DateTime.Now;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        private int NextId()
        {
            int maxId = 0;
            if (dataList == null)
                return 1;
            foreach (var data in dataList)
            {
                int Id = this.Id(data);
                if (Id > maxId)
                    maxId = Id;
            }
            return maxId + 1;
        }

        public T Get(int Id)
        {
            foreach (var data in dataList)
            {
                int dataId = this.Id(data);
                if (dataId == Id)
                    return data;
            }
            return default(T);
        }
        public List<T> ToList()
        {
            return dataList;
        }
        public bool NeedUpdate(DateTime date)
        {
            return DateTime.Compare(date, lastUpdate) < 0;
        }
        public DateTime LastUpdate()
        {
            return lastUpdate;
        }
        public virtual int Add(T data)
        {
            int newId = 0;
            mutex.WaitOne();
            try
            {
                newId = NextId();
                SetAttributeValue(data, "Id", newId);
                dataList.Add(data);
                UpdateFile();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return newId;
        }
        public virtual bool Update(T data)
        {
            bool success = false;
            mutex.WaitOne();
            try
            {
                T dataToUpdate = Get(Id(data));
                if (dataToUpdate != null)
                {
                    int index = dataList.IndexOf(dataToUpdate);
                    dataList[index] = data;
                    UpdateFile();
                    success = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return success;
        }
        public virtual bool Delete(int Id)
        {
            bool success = false;
            mutex.WaitOne();
            try
            {
                T dataToDelete = Get(Id);

                if (dataToDelete != null)
                {
                    int index = dataList.IndexOf(dataToDelete);
                    dataList.RemoveAt(index);
                    UpdateFile();
                    success = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return success;
        }
    }
}