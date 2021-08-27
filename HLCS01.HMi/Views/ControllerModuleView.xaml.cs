using HLCS01.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HLCS01.HMi.Views
{
    /// <summary>
    /// Interaction logic for ControllerModuleView.xaml
    /// </summary>
    public partial class ControllerModuleView : UserControl, INotifyPropertyChanged
    {
        #region -- PROPERTIES --
        
        #region -- PUBLIC --
        public string ControllerModuleName { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public List<ControllerModuleView> ModuleProperties { get; set; } = new List<ControllerModuleView>();
        public List<ControllerModuleView> ModuleMethods { get; set; } = new List<ControllerModuleView>();
        #endregion

        #region -- PRIVATE --
        private ControllerModule controllerModule { get; set; }
        #endregion

        #endregion

        #region -- CONSTRUCTOR --
        public ControllerModuleView(ControllerModule _module)
        {
            InitializeComponent();
            controllerModule = _module;
            DataContext = this;
            ControllerModuleName = _module.ControllerModuleName;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ControllerModuleName)));

            Init();
        }
        public ControllerModuleView(string _moduleName)
        {
            InitializeComponent();
            DataContext = this;
            ControllerModuleName = _moduleName;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ControllerModuleName)));
        }
        #endregion

        #region -- FUNCTIONS --
        #region -- PRIVATE --
        private void Init()
        {
            var properties = controllerModule.GetType().GetProperties();
            var methods = controllerModule.GetType().GetMethods();

            var _props = properties.Where(
                p => p.GetCustomAttributes(true).Where(a => a.GetType() == typeof(ExposedControllerPropertyAttribute)).Any());

            var _methods = methods.Where(
                p => p.GetCustomAttributes(true).Where(a => a.GetType() == typeof(ExposedControllerMethodAttribute)).Any());


            foreach (var prop in _props)
            {
                var attr = prop.GetCustomAttributes(true).FirstOrDefault() as ExposedControllerPropertyAttribute;
                ModuleProperties.Add(new ControllerModuleView(attr.description));
            }
            foreach (var met in _methods)
            {
                var attr = met.GetCustomAttributes(true).FirstOrDefault() as ExposedControllerMethodAttribute;

                ModuleMethods.Add(new ControllerModuleView(attr._description));
            }
        }
        #endregion
        #endregion
    }
}
