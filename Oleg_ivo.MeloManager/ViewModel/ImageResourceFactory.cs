using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Oleg_ivo.Base.Extensions;
using Oleg_ivo.MeloManager.MediaObjects;

namespace Oleg_ivo.MeloManager.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ImageResourceFactory
    {
        public static BitmapImage GetImage(Type type)
        {
            return images.GetOrAdd(type,
                type1 =>
                    new BitmapImage(
                        new Uri(String.Format(@"/Oleg_ivo.MeloManager;component/Resources/{0}.ico", resourcesDic.GetValueOrDefault(type1)),
                            UriKind.Relative)));
        }

        /*private static BitmapSource LoadBitmap(Bitmap source)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero,
                                                                                Int32Rect.Empty,
                                                                                BitmapSizeOptions.FromEmptyOptions());
        }*/

        private static readonly ConcurrentDictionary<Type, BitmapImage> images = new ConcurrentDictionary<Type, BitmapImage>();

        /*private static Icon GetResourceIcon(Type type)
        {
            if (type == typeof (Category)) 
                return MediaIconsResources.folder;
            if (type == typeof (Playlist))
                return MediaIconsResources.form_green;
            if (type == typeof (MediaFile))
                return MediaIconsResources.headphones;

            return null;
        }*/

        private static readonly Dictionary<Type, string> resourcesDic = new Dictionary<Type, string>()
        {
            {typeof(Category), "folder"},
            {typeof(Playlist), "form_yellow"},//"form_blue";//"form_green";//"form_red";
            {typeof(MediaFile), "headphones"},
        };
    }
}