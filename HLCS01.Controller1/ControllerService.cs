using HLCS01.SDK;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Controller1
{
    public class ControllerService : ControllerModule
    {
        #region -- PROPERTIES --
        #region -- PUBLIC --
        #region -- EXPOSED --
        [ExposedControllerProperty("string DateData")]
        public string DateData => DateTime.Now.ToString("fffffff");

        [ExposedControllerProperty("double PV")]
        public double PV { get; set; }


        [ExposedControllerProperty("double SP")]
        public double SP { get; set; }
        #endregion
        #endregion
        #endregion

        #region -- CONSTUCTOR --
        public ControllerService(IEventAggregator eventAggregator) : base(eventAggregator, "Controller1")
        {
            Console.WriteLine();
        }

        #endregion

        #region -- FUNCTIONS --
        #region -- PUBLIC --
        #region -- EXPOSED --
        [ExposedControllerMethod("bool StartPIDControl()")]
        public bool StartPIDControl()
        {
            return true;
        }

        [ExposedControllerMethod("double ReadTempSensor()")]
        public double ReadTempSensor()
        {
            return 67.8;
        }
        #endregion
        #endregion
        #endregion

    }
}
