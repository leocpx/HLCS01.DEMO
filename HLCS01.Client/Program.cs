//using HLCS01.Shared.Attributes;
//using HLCS01.Shared.Communication;
//using HLCS01.Shared.Serializer;
//using Prism.Events;
//using SDK;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using TestEngine;
////using Unity;

//namespace HLCS01.Client
//{
//    class Program
//    {
//        private static IUnityContainer _unityContainer;
//        private static IEventAggregator _eventAggregator;
//        private static IClientService _clientService;

//        static void Main(string[] args)
//        {
//            //var data2 = File.ReadAllBytes("process.bin");
//            //var test = MessagePack.MessagePackSerializer.Deserialize<Process>(data2);
//            var test = new Process();
//            var result = test.ProcessExecuted?.Invoke();
//            var data=MessagePack.MessagePackSerializer.Serialize<Process>(test);
//            File.WriteAllBytes("process.bin", data);

//            _unityContainer = new UnityContainer();
//            _eventAggregator = new EventAggregator();

//            _unityContainer.RegisterSingleton<IEventAggregator, EventAggregator>();
//            _unityContainer.RegisterSingleton<ICommSocket, DummyMqttClient>();
//            _unityContainer.RegisterSingleton<ICommSerializer, MPackCommSerializer<MPackTransportMessage>>();
//            _unityContainer.RegisterSingleton<IUnivSerializer, MPackCommSerializer<MPackTransportMessage>>();
//            _unityContainer.RegisterSingleton<IDataProvider, DataProvider.DataProvider>();
//            _unityContainer.RegisterSingleton<IClientService, ClientService>();


//            LoadModules();
//            _clientService = _unityContainer.Resolve<IClientService>();
//        }

//        private static void LoadModules()
//        {
//            CheckDirectory();
            
//            var assemblies = GetAssemblies();

//            foreach (var asm in assemblies)
//            {
//                InstantiateAllModulesFromAssembly(asm);
//            }
//        }
//        #region -- LOAD MODULES HELPERS --
//        private static List<object> _libInstances { get; set; } = new List<object>();

//        private static void CheckDirectory()
//        {
//            if (!Directory.Exists(@"Modules\netcoreapp3.1\netcoreapp3.1"))
//                Directory.CreateDirectory(@"Modules\netcoreapp3.1\netcoreapp3.1");
//        }
//        private static IEnumerable<Assembly> GetAssemblies()
//        {
//            var dllFiles = new List<string>(Directory.GetFiles(@"Modules\netcoreapp3.1\netcoreapp3.1\")).Where(f => new FileInfo(f).Extension.Equals(".dll"));
//            var assemblies = dllFiles.Select(d => Assembly.Load(File.ReadAllBytes(d)));

//            return assemblies;
//        }
//        private static void InstantiateAllModulesFromAssembly(Assembly _asm)
//        {
//            foreach (var module in _asm.GetExportedTypes())
//            {
//                if(typeof(IModule).IsAssignableFrom(module))
//                {
//                    var _instance = Activator.CreateInstance(module) as IModule;
//                    _instance.RegisterTypes(_unityContainer);
//                    _instance.OnInitialized(_unityContainer);
//                    _libInstances.Add(_instance);
//                }
//            }
//        } 
//        #endregion
//    }
//}
