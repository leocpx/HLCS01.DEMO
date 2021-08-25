using HLCS01.ServiceCore;
using HLCS01.ServiceCore.PubSubEvents;
using HLCS01.Shared.Communication;
using HLCS01.Shared.PubSubEvents;
using Prism.Events;
using System;

namespace HLCS01.Server
{
    public class R0013Service : IService
    {
        #region -- PROPERTIES --

        #region -- PUBLIC --
        public string ServiceName { get; set; } = "R0013-HMIservice";
        public event OnServiceMessageEventHandler OnServiceMessage;
        #endregion

        #region -- PRIVATE --
        private IEventAggregator _ea;
        private ICommSocket _commServer;
        private ICommSerializer _serializer;
        private bool _started = false;
        #endregion

        #endregion

        #region -- CONSTRUCTOR --
        public R0013Service(IEventAggregator ea, ICommSocket commServer, ICommSerializer commSerializer)
        {
            _ea = ea;
            _serializer = commSerializer;
            _commServer = commServer;
            _commServer.OnMessageReceived += _commServer_OnMessageReceived;

            _ea.GetEvent<OnForwardMessageRequestEvent>().Subscribe(
                _m =>
                {
                    var serializedMessage = _serializer.Serialize(_m);
                    _commServer.Send(serializedMessage);
                    LogForwardRequest(_m);
                });

            _ea.GetEvent<OnCloseEvent>().Subscribe(
                () =>
                {
                    _started = false;
                    _commServer.Stop();
                });

            _ea.GetEvent<OnServiceLoadedEvent>().Publish(this);
        }

        #endregion

        #region -- FUNCTIONS --
        #region -- PRIVATE --
        private void LogForwardRequest(IMessage message)
        {
            if (_started)
            {
                var log = $"[{DateTime.Now.ToString("HH:mm:ss")}]*" +
                    $"SERVICE:{ServiceName}*" +
                    $"TX*" +
                    $"ID:{message.Id}*" +
                    $"param:{message.ParameterName}*" +
                    $"value:{message.Value}*";

                OnServiceMessage?.Invoke(this, log);
            }
        }
        private void _commServer_OnMessageReceived(byte[] message)
        {
            if (_started)
            {
                var deserializedMessage = _serializer.Deserialize(message);
                _ea.GetEvent<OnMessageReceivedEvent>().Publish(deserializedMessage);

                var log = $"[{DateTime.Now.ToString("HH:mm:ss")}]*" +
                    $"SERVICE:{ServiceName}*" +
                    $"RX*" +
                    $"ID:{deserializedMessage.Id}*" +
                    $"param:{deserializedMessage.ParameterName}*" +
                    $"value:{deserializedMessage.Value}*";

                OnServiceMessage?.Invoke(this, log);
            }
        }
        #endregion
        #region -- PUBLIC --
        public void Start()
        {
            _commServer.Start();
            _started = true;

            var log = $"[{DateTime.Now.ToString("HH:mm:ss")} " +
                $" SERVICE:{ServiceName} started";

            OnServiceMessage?.Invoke(this, log);
        }
        public void Stop()
        {
            _commServer.Stop();
            _started = false;

            var log = $"[{DateTime.Now.ToString("HH:mm:ss")} " + $" SERVICE:{ServiceName} stopped";
            OnServiceMessage?.Invoke(this, log);
        }
        #endregion
        #endregion
    }
}
