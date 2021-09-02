
using HLCS01.SDK;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Controller4
{
    public class ControllerService : ControllerModule
    {
        public ControllerService(IEventAggregator eventAggregator) : base(eventAggregator, "Controller4")
        {
        }

        #region -- EXPOSED METHODS --
        [ExposedControllerMethod("double ReadSensor(int sensorNumber)")]
        public double ReadSensor(int sensorNumber)
        {
            return 24.4;
        }
        #endregion
    }
}
