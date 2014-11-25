using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using NLog;

namespace FlickerBox.Persistence
{
    public class BasePersister<T>
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly string typePersistedName = typeof(T).Name;
        protected readonly Dictionary<string, T> AllManaged;
        protected readonly object InternalLock = new object();
        private readonly Func<T, string> getId;

        public BasePersister(Func<T,string> getId)
        {
            this.getId = getId;
            AllManaged = Load();
        }

        private Dictionary<string, T> Load()
        {
            log.Info(String.Format( "Loading {0}", typePersistedName));
            var serializer = new XmlSerializer(typeof(List<T>));
            try
            {
                using (var reader = XmlReader.Create(typePersistedName+".xml"))
                {
                    var result = (List<T>)serializer.Deserialize(reader);
                    return result.ToDictionary(o => getId(o), o => o);
                }
            }
            catch (FileNotFoundException)
            {
                log.Warn("File not found ...");
            }
            catch (Exception e)
            {
                log.Error("Error while loading data : " + e);
            }
            return new Dictionary<string, T>();
        }

        public List<T> GetAll()
        {
            lock (InternalLock)
            {
                return AllManaged.Values.ToList();
            }
        }

        protected void Persist(T input)
        {
            lock (InternalLock)
            {
                AllManaged.Add(getId(input),input);
                Save();
            }
        }
        
        private void Save()
        {
            log.Info(String.Format("Saving {0}", typePersistedName));
            var list = AllManaged.Values.ToList();
            var serializer = new XmlSerializer(list.GetType());
            using (var writer = XmlWriter.Create(typePersistedName+".xml"))
            {
                serializer.Serialize(writer, list);
            }
        }

        /// <summary>
        /// Should be used for test only, erase all friends.
        /// </summary>
        public static void ResetAll()
        {
            File.Delete(typeof(T).Name + ".xml");
        }
    }
}
