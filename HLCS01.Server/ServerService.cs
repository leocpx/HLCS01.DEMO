using HLCS01.ServiceCore.Attributes;
using HLCS01.ServiceCore.Bases;
using HLCS01.Shared.Communication;
using Prism.Events;
using System.Diagnostics;

namespace HLCS01.Server
{

    public class ServerService : MessageConsumerBase, IServerService
    {
        private string _version = "8252021.853";
        public ServerService(IEventAggregator eventAggregator, IUnivSerializer univSerializer) : 
            base(eventAggregator, univSerializer)
        { }

        [MessageConsumer(CommMessage.GET_DATA,true)]
        public string SET_CONSOLE_COMMAND(string v, byte[] d)
        {
            string command = v;
            string result = "";
            using (System.Diagnostics.Process proc = new System.Diagnostics.Process())
            {
                proc.StartInfo.FileName = "/bin/bash";
                proc.StartInfo.Arguments = "-c \" " + command + " \"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.Start();

                result += proc.StandardOutput.ReadToEnd();
                result += proc.StandardError.ReadToEnd();

                proc.WaitForExit();
            }
            return result;
        }
    }
}
