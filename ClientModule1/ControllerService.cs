using HLCS01.Shared.Communication;
using HLCS01.Shared.Models;
using Prism.Events;
using SDK;
using System;

namespace ClientModule1
{
    public class ControllerService : MonitorBase, IControlService
    {
        public override int TabIndex { get; set; }
        public string RemoteDate { get; set; }


        public ControllerService(IDataProvider _dataProvider, IEventAggregator _eventAggregator) : base(_dataProvider, _eventAggregator)
        {
            var updateParameter = new UpdateParameter(CommMessage.GET_DATA, GetType().GetProperty(nameof(RemoteDate)))
                .SetUpdateInterval(1000)
                .SetParameterValueGetter(()=>"date")
                .SetExecutionOnMessageReceived((v, d) => Console.WriteLine(v));

            _dataProvider.RegisterParameter(updateParameter);
        }

    }
}
