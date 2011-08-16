using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oleg_ivo.MeloManager.MediaObjects
{
    class Class1
    {
        void test()
        {
            MediaContainer container = new MediaContainer();
            container.ChildMediaContainers.Select(mc => mc.ChildMediaContainer);
            Category category = new Category();
            Playlist playlist = new Playlist();
            MediaFile mediaFile = new MediaFile();
        }
    }
}
