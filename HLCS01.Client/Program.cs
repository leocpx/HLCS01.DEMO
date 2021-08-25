using HLCS01.Shared.Attributes;
using HLCS01.Shared.Communication;
using HLCS01.Shared.Models;
using HLCS01.Shared.Serializer;
using Prism.Events;
using System;
using Unity;

namespace HLCS01.Client
{
    class Program
    {
        private static IUnityContainer _unityContainer;
        private static IEventAggregator _eventAggregator;
        private static IClientService _clientService;

        static void Main(string[] args)
        {
            _unityContainer = new UnityContainer();
            _eventAggregator = new EventAggregator();

            _unityContainer.RegisterSingleton<IEventAggregator, EventAggregator>();
            _unityContainer.RegisterSingleton<ICommSocket, DummyMqttClient>();
            _unityContainer.RegisterSingleton<ICommSerializer, MPackCommSerializer<MPackTransportMessage>>();
            _unityContainer.RegisterSingleton<IUnivSerializer, MPackCommSerializer<MPackTransportMessage>>();
            _unityContainer.RegisterSingleton<IDataProvider, DataProvider.DataProvider>();
            _unityContainer.RegisterSingleton<IClientService, ClientService>();

            _clientService = _unityContainer.Resolve<IClientService>();
        }
    }

    public class ClientService : MonitorBase, IClientService
    {
        //[DefaultMonitor(CommMessage.GET_DATA, 1000)]
        public string ServerVersion
        {
            get { return ""; }
            set
            {
                Console.WriteLine(value);
            }
        }

        public override int TabIndex { get; set; }

        public ClientService(IDataProvider _dataProvider, IEventAggregator _eventAggregator) : 
            base(_dataProvider, _eventAggregator)
        {
            //var prop = GetType().GetProperty("ServerVersion");
            //var _prop = _dataProvider.GetUpdateParameter(prop);
            //_prop.SetParameterValueGetter(() => "ifconfig");


            while(true)
            {
                Console.Write(":>");
                var cmd = Console.ReadLine();

                var updateParameter = new UpdateParameter(CommMessage.GET_DATA)
                    .SetParameterValueGetter(() => cmd)
                    .SetExecutionOnMessageReceived((v, d) => Console.WriteLine(v));
                _dataProvider.WriteRemoteParameter(updateParameter, 1000);
            }
        }

    }
}
