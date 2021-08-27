using Prism.Events;

namespace HLCS01.SDK
{
    public class OnControllerModuleLoaded : PubSubEvent<ControllerModule> { }

    public class OnRequestModuleInstance : PubSubEvent { }

    public class OnProvideModuleInstance : PubSubEvent<ControllerModule> { }

    public class OnMessageProvided : PubSubEvent<string> { }
    public class OnUserProcessModuleSave : PubSubEvent { }
}
