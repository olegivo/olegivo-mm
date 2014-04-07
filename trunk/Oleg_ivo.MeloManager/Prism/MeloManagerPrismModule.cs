using Autofac;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Oleg_ivo.MeloManager.View;

namespace Oleg_ivo.MeloManager.Prism
{
    public class MeloManagerPrismModule : IModule
    {
        private readonly IRegionManager regionManager;
        private readonly IComponentContext context;

        public MeloManagerPrismModule(IRegionManager regionManager, IComponentContext context)
        {
            this.regionManager = regionManager;
            this.context = context;
        }

        public void Initialize()
        {
            regionManager.RegisterViewWithRegion("MainRegion", typeof(MainView));
            //regionManager.RegisterViewWithRegion("MediaTreeRegion", () => context.Resolve<MediaTree>());
            //regionManager.RegisterViewWithRegion("MediaListRegion", () => context.Resolve<MediaList>());
        }
    }
}