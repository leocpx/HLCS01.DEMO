using HLCS01.Shared.Communication;
using HLCS01.Shared.Models;
using Prism.Events;
using System;

namespace HLCS01.Client
{
    public class ClientService : MonitorBase, IClientService
    {

        public override int TabIndex { get; set; }

        public ClientService(IDataProvider _dataProvider, IEventAggregator _eventAggregator) : 
            base(_dataProvider, _eventAggregator)
        {
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
