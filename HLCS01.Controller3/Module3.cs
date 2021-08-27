using HLCS01.SDK;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Controller3
{
    public class Module3 : IModule
    {
        private IControllerModule _controllerModule { get; set; }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _controllerModule = containerProvider.Resolve<IControllerModule>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IControllerModule, ControllerService>();
        }
    }
}
