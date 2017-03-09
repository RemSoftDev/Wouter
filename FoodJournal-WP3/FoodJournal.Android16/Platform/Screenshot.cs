using System;
using Android.Graphics;
using Android.Views;
using Java.IO;

namespace FoodJournal.Android15
{
	public class Screenshot
	{
	
		public static void MakeShot(View view, string filename)
		{
            FoodJournal.Logging.SessionLog.Debug("Saving " + filename);
			// image naming and path  to include sd card  appending name you choose for file
			String mPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).ToString() + "/" + filename;   

			try {
                // create bitmap screen capture
                Bitmap bitmap;
                View v1 = view.RootView;
                v1.DrawingCacheEnabled = true;
                bitmap = Bitmap.CreateBitmap(v1.GetDrawingCache(false));
                v1.DrawingCacheEnabled = false;
			
				if (! System.IO.Directory.Exists(new System.IO.FileInfo(mPath).Directory.FullName))
					System.IO.Directory.CreateDirectory(new System.IO.FileInfo(mPath).Directory.FullName);
				
                var imageFile = new System.IO.FileStream(mPath, System.IO.FileMode.Create);

				//fout = new FileOutputStream(imageFile);
				bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, imageFile);
				imageFile.Flush();
				imageFile.Close();

			} catch (FileNotFoundException e) {
				// TODO Auto-generated catch block
				e.PrintStackTrace();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.PrintStackTrace();
			} catch(Exception e)
            {
                FoodJournal.Logging.LittleWatson.ReportException(e);
            }
		}

	}
}

