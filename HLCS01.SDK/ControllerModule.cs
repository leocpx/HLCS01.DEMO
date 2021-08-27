using Prism.Events;
using SDK;

namespace HLCS01.SDK
{
    public abstract class ControllerModule : IControllerModule, IUserCode
    {
        public string ControllerModuleName { get; set; }
        public ControllerModule(IEventAggregator eventAggregator, string ControllerName)
        {
            ControllerModuleName = ControllerName;
            eventAggregator.GetEvent<OnControllerModuleLoaded>().Publish(this);

            eventAggregator.GetEvent<OnRequestModuleInstance>().Subscribe(
                () =>
                {
                    eventAggregator.GetEvent<OnProvideModuleInstance>().Publish(this);
                });
        }

        public T GetExecute<T>(string Method, params object[] _objects)
        {
            return (T)GetType().GetMethod(Method).Invoke(this, _objects);
        }

        public void Execute(string Method, params object[] _objects)
        {
            GetType().GetMethod(Method).Invoke(this, _objects);
        }
    }
}
