using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.Windows.Media.Imaging;

namespace FoodJournal.Messages
{
    [DataContract]
    public class PictureMessage
    {

        public PictureMessage() { }

        public PictureMessage(WriteableBitmap bmp, string FoodID, string filename)
        {
            this.Contents = ToBase64String(bmp);
            this.FoodID = FoodID;
            this.Filename = filename;
        }

        [DataMember]
        public string Contents;

        [DataMember]
        public string FoodID;

        [DataMember]
        public string Filename;

        private static string ToBase64String(WriteableBitmap bmp)
        {
            string base64String = string.Empty;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                bmp.SaveJpeg(memoryStream, (int)(180), (int)(180), 0, 70);

                memoryStream.Position = 0;
                byte[] byteBuffer = memoryStream.ToArray();
                base64String = Convert.ToBase64String(byteBuffer);
                byteBuffer = null;

            }

            return base64String;
        }
    }
}

#endif