using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace DB
{
    public class Image_Actions
    {
        /** 
        * Récupérer l'image au format byte
        * 
        * @param path    : le chemin de l'image
        * 
        * @return l'image au format byte
        * 
        */
        public static byte[] getImageByte(String path)
        {
            Bitmap bmp1 = new Bitmap(path);
            MemoryStream ms = new MemoryStream();
            bmp1.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        /** 
       * Récupérer l'image au format byte
       * 
       * @param img    : l'image
       * 
       * @return l'image au format byte
       * 
       */
        public static byte[] getImageByte(Image img)
        {
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();

        }

        /** 
       * Récupérer l'image à partir du format byte
       * 
       * @param img    : L'image au format byte
       * 
       * @return l'image
       * 
       */
        public static Image getImage(byte[] img)
        {
            MemoryStream ms = new MemoryStream(img);
            return Image.FromStream(ms);
        }
    }
}
