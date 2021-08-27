using SDK;
using Unity;

namespace ClientModule1
{
    public class Module1 : IModule
    {
        private IControlService controlService;
        public void OnInitialized(IUnityContainer container)
        {
            controlService = container.Resolve<IControlService>();
        }

        public void RegisterTypes(IUnityContainer container)
        {
            container.RegisterSingleton<IControlService, ControllerService>();
        }
    }
}
