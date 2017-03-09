using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Values;
using FoodJournal.Extensions;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using Windows.Foundation.Collections;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using FoodJournal.ViewModels;
using FoodJournal.Messages;

namespace FoodJournal.Logging
{

    public class PictureCache
    {

        public List<string> all = new List<string>();
        private const string PicturesFolder = "pictures";
        private const int PictureSize = 240;
        private const int PictureQuality = 90;

        private List<PicturesVM> vms = new List<PicturesVM>(); // TODO: subscibe to messagebroker instead?

        public void AddVM(PicturesVM vm) { vms.Add(vm); }

        private PictureCache()
        {

            try
            {
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isStore.DirectoryExists(PicturesFolder))
                        isStore.CreateDirectory(PicturesFolder);

                    List<string> ToDelete = new List<string>();
                    ToDelete.AddRange(isStore.GetFileNames(PicturesFolder + "\\item0-*"));
                    foreach (var filename in ToDelete)
                    {
                        SessionLog.RecordTraceValue("Deleting zombie picture", filename);
                        isStore.DeleteFile(PicturesFolder + "\\" + filename);
                    }

                    all.AddRange(isStore.GetFileNames(PicturesFolder + "\\*"));
                }
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

        }

        private static PictureCache _current;
        public static PictureCache Current { get { if (_current == null) _current = new PictureCache(); return _current; } }

        private IEnumerable<string> GetPatternMatches(string pattern)
        {
            foreach (var file in all)
                if (file.StartsWith(pattern))
                    yield return file;
        }

        private string NextFilenameIn(string pattern)
        {
            int i = 1;
            string output;
            do
            {
                output = string.Format("{0}-{1}.jpg", pattern, i);
                i++;
            } while (all.Contains(output));
            return output;
        }
        public string NextFilename(DateTime date, Period period) { return NextFilenameIn(string.Format("{0}.{1}", date.ToStorageStringDate(), (int)period)); }
        public string NextFilename(string Text) { return NextFilenameIn(string.Format("item{0}", Text)); }

        public List<string> GetFoodItemPictures(string Text) { return new List<string>(GetPatternMatches(string.Format("item{0}", Text))); }
        public List<string> GetPeriodPictures(DateTime date, Period period) { return new List<string>(GetPatternMatches(string.Format("{0}.{1}", date.ToStorageStringDate(), (int)period))); }

        public void Save(MemoryStream stream, string filename, string foodId, bool rotate)
        {

            BitmapImage b = new BitmapImage();
            b.CreateOptions = BitmapCreateOptions.None;
            b.SetSource(stream);

            //calculate bounding box
            int w = b.PixelWidth;
            int h = b.PixelHeight;
            WriteableBitmap wb;

            if (w == h)
            {

                wb = new WriteableBitmap(b);

            }
            else
            {

                // crop to square

                int s = (h < w) ? h : w;

                Image temporaryImage = new Image { Source = b, Width = w, Height = h };

                wb = new WriteableBitmap(s, s);

                Transform t;
                var tt = new TranslateTransform();
                tt.X = (s - w) / 2.0;
                tt.Y = (s - h) / 2.0;

                if (rotate)
                    t = new CompositeTransform() { TranslateX = tt.X, TranslateY = tt.Y, CenterX = 0.5 * w, CenterY = 0.5 * h, Rotation = 90 };
                else
                    t = tt;

                wb.Render(temporaryImage, t);
                wb.Invalidate();

            }

            long len = 0;
            using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var outstream = isStore.CreateFile(PicturesFolder + "\\" + filename))
                {
                    wb.SaveJpeg(outstream, PictureSize, PictureSize, 0, PictureQuality);
                    len = outstream.Length / 1024;
                    all.Add(filename);

                }
            }

            SessionLog.RecordTraceValue("New picture",  filename , len.ToString() + " kb");

            var bw = new System.ComponentModel.BackgroundWorker();
            bw.DoWork += (s, a) =>
            {
                try
                {
                    MessageQueue.Push(new PictureMessage(wb, foodId, filename));
                    SessionLog.RecordTrace("Picture msg saved.");
                }
                catch (Exception ex) { LittleWatson.ReportException(ex); }
            };
            bw.RunWorkerAsync();


            foreach (var vm in vms)
                vm.Refresh();

        }

        public BitmapImage LoadPicture(string filename)
        {

            using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
            {

                using (var stream = isStore.OpenFile(PicturesFolder + "\\" + filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var b = new BitmapImage();
                    b.SetSource(stream);
                    return b;
                }
            }

        }

        public void Delete(string filename)
        {
            try
            {
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    isStore.DeleteFile(PicturesFolder + "\\" + filename);
                }
                all.RemoveAll(x => x.Equals(filename));

                SessionLog.RecordTraceValue("Deleted picture" , filename);
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

            foreach (var vm in vms)
                vm.Refresh();
        }

        public void UpdateNewItemWithID(string text)
        {
            try
            {
                var NewFilename = string.Format("item{0}-", text);
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    foreach (var filename in isStore.GetFileNames(PicturesFolder + "\\item0-*"))
                        isStore.MoveFile(PicturesFolder + "\\" + filename, PicturesFolder + "\\" + filename.Replace("item0-", NewFilename));

                    all.Clear();
                    all.AddRange(isStore.GetFileNames(PicturesFolder + "\\*"));
                }
            }
            catch (Exception ex) { LittleWatson.ReportException(ex); }

            foreach (var vm in vms)
                vm.Refresh();
        }

    }

}

#endif