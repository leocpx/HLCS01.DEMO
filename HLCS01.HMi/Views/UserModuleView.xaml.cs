using HLCS01.SDK;
using Prism.Events;
using SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    /// Interaction logic for UserModuleView.xaml
    /// </summary>
    public partial class UserModuleView : UserControl, INotifyPropertyChanged
    {
        #region -- PROPERTIES --
        #region -- PUBLIC --
        public ICommand SaveCommand => new DefaultCommand(SaveAction, () => true);
        public string UserProcessName { get; set; } 
        public event PropertyChangedEventHandler PropertyChanged;
        public UserProcessWrapper _UserProcessWrapper { get; set; }
        #endregion
        #region -- PRIVATE --
        private IEventAggregator eventAggregator;
        #endregion
        #endregion
        public UserModuleView(IEventAggregator eventAggregator, UserProcessWrapper userProcessWrapper)
        {
            this.eventAggregator = eventAggregator;
            InitializeComponent();
            DataContext = this;
            _UserProcessWrapper = userProcessWrapper;
            UserProcessName = _UserProcessWrapper.UserProcessName;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UserProcessName)));
        }

        #region -- METHODS --
        #region -- PUBLIC --
        public UserModuleView Clone()
        {
            var result = new UserModuleView(eventAggregator,_UserProcessWrapper);
            return result;
        }
        #endregion
        #region -- PRIVATE --
        public void SaveAction()
        {
            if (!Directory.Exists(UserProcessWrapper.UserProcessModulePath))
                Directory.CreateDirectory(UserProcessWrapper.UserProcessModulePath);

            eventAggregator.GetEvent<OnUserProcessModuleSave>().Publish(_UserProcessWrapper);
            _UserProcessWrapper.UserProcessName = UserProcessName;
            var data = MessagePack.MessagePackSerializer.Serialize<UserProcessWrapper>(_UserProcessWrapper);
            File.WriteAllBytes(UserProcessWrapper.UserProcessModulePath + $@"\{UserProcessName}.upm", data);

            if (File.Exists(UserProcessWrapper.UserProcessModulePath + $@"\new user process.upm"))
                File.Delete(UserProcessWrapper.UserProcessModulePath + $@"\new user process.upm");
        }
        #endregion
        #endregion

    }
}
