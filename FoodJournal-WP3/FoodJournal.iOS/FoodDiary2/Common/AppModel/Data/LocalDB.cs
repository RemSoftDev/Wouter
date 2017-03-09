using FoodJournal.AppModel;
using FoodJournal.Logging;
using FoodJournal.WinPhone.Common.AppModel.Data.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoodJournal.AppModel.Data
{


    /// <summary>
    /// Local DB is 4th generation local storage for Food Journal app:
    /// Essentially a NoSQL storage container.
    /// 
    /// LocalDB is designed to be as simple as possible, to support cross-platform implementation
    /// Additionally, it stores entire documents, which are XML representation of a complete object graph
    /// These documents are optimized for extensibility, and synchronization, to enable partial and comprehensive multi-client synchronization, and forward versioning support
    /// 
    /// To interact with the storage, you need to identify
    /// a. Storage container name (IE identify a specific month)
    /// b. Document type (IE Day, to interact with all the data collected for a day)
    /// c. Document Id (IE your current date formatted as a filename compatible string)
    /// 
    /// To get the best performance, it is recommended to make as few calls to this class as possible; storing all required data for 1 scenario in a whole document.
    /// 
    /// </summary>
    public static class LocalDB
    {

        // FILE FORMAT
        // Because Windows Phone 8 doesn't support the ZipArchive class, and our files are small anyway, we'll be doing direct file access.
        // We'll have an index at the top of the file, so individual documents can be loaded the fastest. For storing, we'll read the whole file in memory, and write the entire file contents at once
        // In the future, we could optimize this by reserving file pages of 1000 bytes or so. Documents can then be 1 or more pages, if the number of pages increases, the document is placed at the end of the file.
        // 
        // INDEXCOUNT
        // INDEX[]: DOCUMENTNAME, START, CHECKSUM
        // DOCUMENT[]

        private class documentrow
        {
            public string name;
            public int start;
            public int checksum;
            public int checksumstart;
            public string contents;
        }

        // Thread safety sync
        private static readonly AutoResetEvent ThreadSync = new AutoResetEvent(true);


        public static object ReadDataContract(string Container, string DocumentType, string DocumentID, Type DataContractType)
        {
            try
            {
                string Xml = Read(Container, DocumentType, DocumentID);
                if (Xml == null) return null;
                return DataContractSerialization.Deserialize(Xml, DataContractType);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

            return null;
        }

        public static void WriteDataContract(string Container, string DocumentType, string DocumentID, Object DataContract)
        {
            try
            {
                string Xml = DataContractSerialization.Serialize(DataContract);
                WriteAsync(Container, DocumentType, DocumentID, Xml);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
        }

        // Threadsafe reading of entire document contents
        public static string Read(string Container, string DocumentType, string DocumentID)
        {

            try
            {
                ThreadSync.WaitOne(); // Wait till we have a sync signal, and clear the signal

				SessionLog.StartPerformance("Read");
				return ReadDocument(Container, DocumentType, DocumentID);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }
            finally
            {
				SessionLog.EndPerformance("Read");
                ThreadSync.Set(); // Set the signal for the next thread to go if it comes to it 
            }

            return null;

        }

        public static void WriteAsync(string Container, string DocumentType, string DocumentID, string Contents)
        {

            //BackgroundTask.Start(0, () =>
            //{

//				FoodJournal.Model.Data.FoodJournalBackup.Log("Write",Container,DocumentType + "." + DocumentID);

                try
                {
                    ThreadSync.WaitOne(); // Wait till we have a sync signal, and clear the signal

					SessionLog.StartPerformance("Write");
                    WriteDocument(Container, DocumentType, DocumentID, Contents);
				}
                catch (Exception ex) { LittleWatson.ReportException(ex); }
                finally
                {
					SessionLog.EndPerformance("Write");
                    ThreadSync.Set(); // Set the signal for the next thread to go if it comes to it 
                }

				SyncQueue.Post(Container, DocumentType, DocumentID, Contents);

            //});

        }

        #region GetContainerFilename
        private static string GetContainerFilename(string Container)
        {

            string filename = "data" + Container + ".dat";
#if NETFX_CORE
                var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, filename);
#else

#if SILVERLIGHT
            // Windows Phone expects a local path, not absolute
            //var path = "ms-appdata:///" + filename;
            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "/" + filename;
            // string path;// = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().;
            // path = filename;
#else

#if __ANDROID__
				// Just use whatever directory SpecialFolder.Personal returns
				//string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); ;
				string libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); ;
#else
                // we need to put in /Library/ on iOS5.1 to meet Apple's iCloud terms
                // (they don't want non-user-generated data in Documents)
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
                string libraryPath = Path.Combine(documentsPath, "../Library/"); // Library folder
#endif

                var path = Path.Combine(libraryPath, filename);

			//Console.WriteLine(String.Format("{0}, {1}", path, System.IO.File.Exists(path.Replace("com","app"))));
#endif
#endif
            return path;
        }
        #endregion

        // Returns null when Document Not Found, or some exception occurred
        private static string ReadDocument(string Container, string DocumentType, string DocumentID)
        {

            var filename = GetContainerFilename(Container);
            var documentname = DocumentType + "." + DocumentID;

#if DEBUG
			System.Console.WriteLine("Reading {0} {1}", filename, documentname);
#endif

            if (!System.IO.File.Exists(filename))
            {
#if DEBUG
				System.Console.WriteLine("not found");
#endif
                return null;
            }

            using (FileStream instream = new FileStream(filename, FileMode.Open))
            using (var r = new BinaryReader(instream))
            {

                int count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                {

                    string doc = r.ReadString();
                    int start = r.ReadInt32();
                    int checksum = r.ReadInt32();

                    if (doc == documentname)
                    {
                        instream.Seek(start, SeekOrigin.Begin);
#if DEBUG
                        var result = r.ReadString(); 
						System.Console.WriteLine("Length: {0}", result.Length);
                        return result; // <- EXITS HERE
#else
                        return r.ReadString(); // <- EXITS HERE
#endif
                    }

                }
            }

            return null;
        }

        private static void WriteDocument(string Container, string DocumentType, string DocumentID, string Contents)
        {

            var filename = GetContainerFilename(Container);
            var documentname = DocumentType + "." + DocumentID;

#if DEBUG
			System.Console.WriteLine("Writing {0} {1}", filename, documentname);
			if (DocumentType == "Day" || DocumentType == "Items" || DocumentType == "Recent")
				System.Console.WriteLine(Contents);
			//System.Diagnostics.Debug.WriteLine(Contents);
#endif

            List<documentrow> documents = new List<documentrow>();
            // (last) written documents are always placed at the end of the file. this improves update time, and makes file management as easy as possible:
            // if the the document already exists, but is in the middle, removing by moving all the bytes after it up


            // first, read the current file
            if (System.IO.File.Exists(filename))
            {

                using (FileStream instream = new FileStream(filename, FileMode.Open))
                using (var r = new BinaryReader(instream))
                {

                    int count = r.ReadInt32();
                    int hitid = count;

                    // read index
                    for (int i = 0; i < count; i++)
                    {
                        documents.Add(new documentrow() {
                            name = r.ReadString(), 
                            start = r.ReadInt32(), 
                            checksumstart = (int)instream.Position,
                            checksum = r.ReadInt32()});

                        if (documents[i].name == documentname) hitid = i;
                    }

                    // read contents
                    for (int i=0; i<count; i++)
                        documents[i].contents = r.ReadString();

                    // if the document is already at the end, it's easy, we just have to write the contents and the checksum
                    if (hitid == count-1)
                    {

                        using (var w = new BinaryWriter(instream))
                        {
                            w.Seek(documents[hitid].checksumstart, SeekOrigin.Begin);
                            w.Write(Contents.GetHashCode());

                            w.Seek(documents[hitid].start, SeekOrigin.Begin);
                            w.Write(Contents);
                            return; // <-- DONE, EXIT
                        }
                    }

                    // if somewhere in the middle; remove it from there, add it at the end
                    if (hitid < count)
                        documents.RemoveAt(hitid);

                }

            }

            documents.Add(new documentrow() { name = documentname, contents = Contents, checksum = Contents.GetHashCode() });

            // Rebuild the whole out file.
            using (FileStream outstream = new FileStream(filename, FileMode.OpenOrCreate))
            using (BinaryWriter w = new BinaryWriter(outstream))
            {

                w.Write(documents.Count);

                documentrow doc;

                // write index
                for (int i=0; i<documents.Count; i++)
                {
                    doc = documents[i];
                    w.Write(doc.name);
                    w.Write((int)0); // gotta fill this later;
                    w.Write((int)0); // filling this later as well
                }

                // write contents
                for (int i=0; i<documents.Count; i++)
                {
                    doc = documents[i];
                    doc.start=(int)outstream.Position;
                    w.Write(doc.contents);
                }

                // write index with correct values
                outstream.Seek(0, SeekOrigin.Begin);
                w.Write(documents.Count);
                for (int i=0; i<documents.Count; i++)
                {
                    doc = documents[i];
                    w.Write(doc.name);
                    w.Write(doc.start); 
                    w.Write(doc.checksum); 
                }

            }

        }

    }
}
