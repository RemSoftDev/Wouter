using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.Threading;
using FoodJournal.Logging;
using FoodJournal.AppModel.Data;
using FoodJournal.WinPhone.Common.AppModel.Data.Serialization;

namespace FoodJournal.AppModel
{

    public static class LocalStorage<T> where T : new()
    {

        public static void Save(string name, T settings)
        {
            try
            {

                FoodJournal.Model.Cache.ISOSync.WaitOne();

//#if DEBUG
//                IsolatedStorageSettings.ApplicationSettings[name] = settings;
//                IsolatedStorageSettings.ApplicationSettings.Save();
//#else
                if (IsolatedStorageSettings.ApplicationSettings.Contains(name))
                    IsolatedStorageSettings.ApplicationSettings.Remove(name);
//#endif

                LocalDB.WriteDataContract("Settings", settings.GetType().Name, "All", settings);

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
            finally { FoodJournal.Model.Cache.ISOSync.Set(); }

        }

        public static T LoadOrNew(string name)
        {
            T result = default(T);
            try
            {

                // Prefer LocalDB
                result = (T)LocalDB.ReadDataContract("Settings", typeof(T).Name, "All", typeof(T));

                if (result == null)
                {
                    // Fall back to IsolatedStorageSettings
                    if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(name, out result))
                        return result;
                    else
                        return new T();
                }

#if DEBUG
                // in debug, compare LocalDB and IsolatedStorageSettings based retrieval  (to validate the LocalDB)
                T fromAS = default(T);
                if (IsolatedStorageSettings.ApplicationSettings.Contains(name))
                    IsolatedStorageSettings.ApplicationSettings.TryGetValue(name, out fromAS);

                if (fromAS != null)
                    if (!DataContractSerialization.IsObjectTreeIdentical(result, fromAS))
                    {
                        System.Diagnostics.Debugger.Break();
                    }
#endif

            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
            return result;
        }

    }

#if LEGACY
    public static class LocalStorage<T> where T : new()
    {

        public static void Save(string name, T settings)
        {
            try
            {
            // http://forums.xamarin.com/discussion/7275/simple-file-write-operation-is-throwing-unauthorizedaccessexception
                FoodJournal.Model.Cache.ISOSync.WaitOne();
                IsolatedStorageSettings.ApplicationSettings[name] = settings;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
            finally { FoodJournal.Model.Cache.ISOSync.Set(); }

        }

        public static T LoadOrNew(string name)
        {
            T result = default(T);
            try
            {
                if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(name, out result))
                    return new T();
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
            return result;
        }

    }
#endif

}
