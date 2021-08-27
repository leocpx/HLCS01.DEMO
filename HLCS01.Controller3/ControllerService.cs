using HLCS01.SDK;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Controller3
{
    public class ControllerService : ControllerModule
    {
        public ControllerService(IEventAggregator eventAggregator) : base(eventAggregator, "TempController")
        {
        }

        [ExposedControllerMethod("void TurnPID(bool state)")]
        public void TurnPID(bool state)
        {
            Console.WriteLine("message from the module");
        }

        [ExposedControllerMethod(" int GetInt(int number)")]
        public int GetInt(int number)
        {
            return number * 2;
        }
    }
}
