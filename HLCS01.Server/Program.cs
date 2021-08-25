using HLCS01.ServiceCore;
using HLCS01.Shared.Communication;
using HLCS01.Shared.Serializer;
using Prism.Events;
using System;
using Unity;

namespace HLCS01.Server
{
    class Program
    {
        private static UnityContainer _unityContainer;
        private static EventAggregator _eventAggregator;
        private static IServerService _serverService;
        private static IService _serviceDispatcher;

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing unity...");
            _unityContainer = new UnityContainer();
            _eventAggregator = new EventAggregator();

            _unityContainer.RegisterSingleton<IEventAggregator, EventAggregator>();
            _unityContainer.RegisterSingleton<ICommSocket, DummyMqttServer>();
            _unityContainer.RegisterSingleton<ICommSerializer, MPackCommSerializer<MPackTransportMessage>>();
            _unityContainer.RegisterSingleton<IUnivSerializer, MPackCommSerializer<MPackTransportMessage>>();
            _unityContainer.RegisterSingleton<IServerService, ServerService>();
            _unityContainer.RegisterSingleton<IService, R0013Service>();

            _serviceDispatcher = _unityContainer.Resolve<IService>();
            _serverService = _unityContainer.Resolve<IServerService>();
            Console.WriteLine("Starting services...");
            _serviceDispatcher.Start();
            _serviceDispatcher.OnServiceMessage += _serviceDispatcher_OnServiceMessage;

            Console.WriteLine("Press return to exit...");
            Console.ReadLine();
        }

        private static void _serviceDispatcher_OnServiceMessage(IService sender, string serviceMessage)
        {
            Console.WriteLine(serviceMessage);
        }
    }
}
