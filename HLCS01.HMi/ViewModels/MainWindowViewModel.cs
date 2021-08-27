using HLCS01.HMi.Views;
using HLCS01.SDK;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HLCS01.HMi.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region -- PROPERTIES --
        #region -- PUBLIC --
        #region -- BINDED --
        public DelegateCommand DeleteUserProcessModuleCommand { get; set; }
        public DelegateCommand CreateUserProcessModuleCommand { get; set; }
        public DelegateCommand CompileCommand { get; set; }
        public DelegateCommand RunCommand { get; set; }
        public UserModuleView SelectedUserProcessModule
        {
            get => _selectedUserProcessModule;
            set
            {
                _selectedUserProcessModule = value;
                
                if(value!=null)
                {
                    SourceCode = _selectedUserProcessModule._UserProcessWrapper.ExecuteSourceCode;
                    RaisePropertyChanged(nameof(SourceCode));
                }
            }
        }
        public ObservableCollection<UserModuleView> UserProcessModules { get; set; } = new ObservableCollection<UserModuleView>();
        public List<ControllerModule> ControllerModules { get; set; } = new List<ControllerModule>();
        public ObservableCollection<ControllerModuleView> ControllerModuleViews { get; set; } = new ObservableCollection<ControllerModuleView>();
        public ObservableCollection<ControllerModuleView> SelectedModuleProperties { get; set; } = new ObservableCollection<ControllerModuleView>();
        public ObservableCollection<ControllerModuleView> SelectedModuleMethods { get; set; } = new ObservableCollection<ControllerModuleView>();
        public ControllerModuleView SelectedModule
        {
            get => _selectedModule;
            set
            {
                _selectedModule = value;
                SelectedModuleProperties = _selectedModule.ModuleProperties.ToObservable();
                SelectedModuleMethods = _selectedModule.ModuleMethods.ToObservable();
                RaisePropertyChanged(nameof(SelectedModuleProperties));
                RaisePropertyChanged(nameof(SelectedModuleMethods));
            }
        }

        public string SourceCode { get; set; }
        public string CodeErrors { get; set; }
        #endregion
        #endregion
        #region -- PRIVATE --
        private UserModuleView _selectedUserProcessModule { get; set; }
        private IEventAggregator _eventAggregator { get; set; }
        private ControllerModuleView _selectedModule { get; set; }

        private UserProcessWrapper _userProcessWrapper { get; set; }
        #endregion
        #endregion

        public MainWindowViewModel(IEventAggregator eventAggregator)
        {
            DeleteUserProcessModuleCommand = new DelegateCommand(DeleteUserProcessModuleAction);
            CompileCommand = new DelegateCommand(CompileAction);
            CreateUserProcessModuleCommand = new DelegateCommand(CreateUserProcessModuleAction);
            RunCommand = new DelegateCommand(RunAction);
            _eventAggregator = eventAggregator;
            Console.SetOut(new ControlWriter(
                (s) =>
                {
                    CodeErrors += s;
                    RaisePropertyChanged(nameof(CodeErrors));
                }));
            Console.WriteLine("hello");
            _eventAggregator.GetEvent<OnControllerModuleLoaded>().Subscribe(
                (_m) =>
                {
                    ControllerModules.Add(_m);
                    ControllerModuleViews.Add(new ControllerModuleView(_m));
                });

            new Thread(
                () =>
                {
                    Thread.Sleep(1000);
                    _userProcessWrapper = new UserProcessWrapper() { UserProcessName="UserProcess1"};
                    _userProcessWrapper.SetEventAggregator(eventAggregator);
                    //_userProcessWrapper.ImportedModuleNames = new List<string>() { "Controller1", "Controller2" };
                    SourceCode = _userProcessWrapper.ExecuteSourceCode;
                    RaisePropertyChanged(nameof(SourceCode));
                    _userProcessWrapper.LoadImportedModules();
                }).Start();

            _eventAggregator.GetEvent<OnUserProcessModuleSave>().Subscribe(
                () =>
                {
                    if (SelectedUserProcessModule != null)
                        SelectedUserProcessModule._UserProcessWrapper.ExecuteSourceCode = SourceCode;
                });

            LoadSavedUserProcessModules();
        }

        public class ControlWriter : TextWriter
        {
            Action<string> Fu;
            public ControlWriter(Action<string> func)
            {
                Fu = func;
            }

            public override void Write(char value)
            {
                Fu(value.ToString());
                //textbox.Text += value;
            }

            public override void Write(string value)
            {
                Fu(value);
                //textbox.Text += value;
            }

            public override Encoding Encoding
            {
                get { return Encoding.ASCII; }
            }
        }


        private void LoadSavedUserProcessModules()
        {
            var files = Directory.GetFiles(UserProcessWrapper.UserProcessModulePath).ToList();
            var upms = files.Select(f => new FileInfo(f)).Where(_f => _f.Extension.Contains("upm")).ToList();

            upms.ForEach(
                u =>
                {
                    var data = File.ReadAllBytes(u.FullName);
                    var reconstructed = MessagePack.MessagePackSerializer.Deserialize<UserProcessWrapper>(data);

                    var newUserProcessModuleView = new UserModuleView(_eventAggregator, reconstructed);
                    UserProcessModules.Add(newUserProcessModuleView);

                });
        }
        private void RunAction()
        {
            try
            {
                RaisePropertyChanged(nameof(CodeErrors));
                bool result = _userProcessWrapper.ExecuteUserCode();
            }
            catch (Exception ex)
            {
                CodeErrors = ex.Message+"\n"+ex.InnerException?.Message;
                RaisePropertyChanged(nameof(CodeErrors));
            }
        }
        private void DeleteUserProcessModuleAction()
        {
            File.Delete(UserProcessWrapper.UserProcessModulePath + $@"\{SelectedUserProcessModule.UserProcessName}.upm");
            UserProcessModules.Remove(SelectedUserProcessModule);
        }

        private void CreateUserProcessModuleAction()
        {
            var newUserProcessWrapper = new UserProcessWrapper(_eventAggregator) { UserProcessName = "new user process" };
            var newUserProcessModuleView = new UserModuleView(_eventAggregator, newUserProcessWrapper);
            UserProcessModules.Add(newUserProcessModuleView);
        }
        private void CompileAction()
        {
            try
            {
                CodeErrors = "Compiled\n";
                RaisePropertyChanged(nameof(CodeErrors));
                _userProcessWrapper.ExecuteSourceCode = SourceCode;
                _userProcessWrapper.CompileExecutionCode();
            }
            catch (Exception ex)
            {
                CodeErrors = ex.Message;
                RaisePropertyChanged(nameof(CodeErrors));
            }
        }
    }


    public static class Extensions
    {
        public static ObservableCollection<T> ToObservable<T>(this List<T> _myList)
        {
            return new ObservableCollection<T>(_myList);
        }
    }
}
