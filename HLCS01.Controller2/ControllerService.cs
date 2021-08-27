using HLCS01.SDK;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Controller2
{
    public class ControllerService : ControllerModule
    {
        #region -- PROPERTIES --
        [ExposedControllerProperty("bool Status")]
        public bool Status { get; set; }

        #endregion
        public ControllerService(IEventAggregator eventAggregator) : base(eventAggregator, "Controller2")
        {
            Console.WriteLine();
        }

        #region -- FUNCTIONS --
        [ExposedControllerMethod("void TurnON()")]
        public void TurnON()
        { }

        [ExposedControllerMethod("void TurnOFF()")]
        public void TurnOFF()
        { }

        [ExposedControllerMethod("double ReadTankVolume(int tank)")]
        public double ReadTankVolume(int tank)
        { return 2; }

        [ExposedControllerMethod("double ReadTankTemperature(int tank)")]
        public double ReadTankTemperature(int tank)
        { return 24.5; }
        #endregion
    }
}
