using HLCS01.HMi.Views;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HLCS01.HMi
{
    public class BootStrapper : PrismBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            Container.Resolve<IRegionManager>().RegisterViewWithRegion("ShellRegion", typeof(MainWindowView));
            return new Shell();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            var path = Directory.GetCurrentDirectory() + @"\Modules";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return new DirectoryModuleCatalog() { ModulePath = path };
        }
    }
}
