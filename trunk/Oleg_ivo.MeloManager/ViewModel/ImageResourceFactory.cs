using System;
using System.Collections;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ImageResourceFactory
    {
        public static BitmapImage GetImage(Type type)
        {
            //return GetResourceIcon(type);
            if (images.ContainsKey(type)) return images[type] as BitmapImage;

            BitmapImage image =
                new BitmapImage(new Uri(@"/Oleg_ivo.MeloManager.MediaObjects;component/Resources/" + GetResourceName(type) + ".ico",
                                        UriKind.Relative));
            images.Add(type, image);
/*
            var icon = GetResourceIcon(type);
            if (icon != null)
            {
                var bitmap = icon.ToBitmap();
                image = LoadBitmap(bitmap);
                images.Add(type, image);
            }

*/
            return image;
        }

        private static BitmapSource LoadBitmap(Bitmap source)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero,
                                                                                Int32Rect.Empty,
                                                                                BitmapSizeOptions.FromEmptyOptions());
        }

        private static Hashtable images = new Hashtable();

        private static Icon GetResourceIcon(Type type)
        {
            if (type == typeof (Category)) 
                return MediaIconsResources.folder;
            if (type == typeof (Playlist))
                return MediaIconsResources.form_green;
            if (type == typeof (MediaFile))
                return MediaIconsResources.headphones;

            return null;
        }

        private static string GetResourceName(Type type)
        {
            if (type == typeof (Category)) 
                return "folder";
            if (type == typeof (Playlist))
                //return"form_blue";
                //return"form_green";
                //return"form_red";
                return"form_yellow";
            if (type == typeof (MediaFile))
                return "headphones";

            return null;
        }
    }
}