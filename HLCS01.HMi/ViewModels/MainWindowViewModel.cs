using GongSolutions.Wpf.DragDrop;
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
    public class MainWindowViewModel : BindableBase, IDropTarget
    {
        #region -- PROPERTIES --

        #region -- PUBLIC --
        #region -- BINDED --
        #region -- ICOMMANDS --
        public DelegateCommand SaveProcessCommand { get; set; } 
        public DelegateCommand DeleteUserProcessModuleCommand { get; set; } 
        public DelegateCommand CreateUserProcessModuleCommand { get; set; } 
        public DelegateCommand AddCommand { get; set; } 
        public DelegateCommand RemoveCommand { get; set; } 
        public DelegateCommand CompileCommand { get; set; } 
        public DelegateCommand RunCommand { get; set; } 
        #endregion
        public bool EditMode { get; set; } 
        public UserModuleView SelectedUserProcessModule
        {
            get => _selectedUserProcessModule;
            set
            {
                if(UserProcessModules.Contains(value))
                {
                    SelectedUserProcessName = value.UserProcessName;
                    RaisePropertyChanged(nameof(SelectedUserProcessName));
                }

                if(UserProcess.Contains(value))
                {
                    SelectedUserProcessName += " > " + value.UserProcessName;
                    RaisePropertyChanged(nameof(SelectedUserProcessName));
                }

                _selectedUserProcessModule = value;

                _selectedUserProcessModule?._UserProcessWrapper?.SetEventAggregator(_eventAggregator);

                if(value!=null && !EditMode)
                {
                    UserProcess.Clear();
                    SourceCode = _selectedUserProcessModule._UserProcessWrapper.ExecuteSourceCode;
                    RaisePropertyChanged(nameof(SourceCode));

                    if(value._UserProcessWrapper.UserProcessWrapperCollection.Count>0)
                    {
                        foreach (var item in value._UserProcessWrapper.UserProcessWrapperCollection)
                        {
                            var loadedUserProcess = new UserModuleView(_eventAggregator, item);
                            UserProcess.Add(loadedUserProcess);
                        }
                    }
                }
            }
        }
        public ObservableCollection<UserModuleView> UserProcess { get; set; } = new ObservableCollection<UserModuleView>();
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

        private string _selectedUserProcessName;

        public string SelectedUserProcessName
        {
            get { return _selectedUserProcessName; }
            set { _selectedUserProcessName = value; }
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

        #region -- CONSTRUCTOR --
        public MainWindowViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            
            InitCommandDelegates();
            InitEventSubscriptions();
            LoadSavedUserProcessModules();
        }

        #endregion

        #region -- METHODS --

        #region -- PUBLIC --

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

        #endregion

        #region -- PRIVATE --
        private void LoadSavedUserProcessModules()
        {
            var files = Directory.GetFiles(UserProcessWrapper.UserProcessModulePath).ToList();
            var upms = files.Select(f => new FileInfo(f)).Where(_f => _f.Extension.Contains("upm")).ToList();
            UserProcessModules.Clear();
            UserProcess.Clear();
            SelectedUserProcessModule = null;
            upms.ForEach(
                u =>
                {
                    var data = File.ReadAllBytes(u.FullName);
                    var reconstructed = MessagePack.MessagePackSerializer.Deserialize<UserProcessWrapper>(data);

                    var newUserProcessModuleView = new UserModuleView(_eventAggregator, reconstructed);
                    UserProcessModules.Add(newUserProcessModuleView);
                });
        }
        private void InitCommandDelegates()
        {
            DeleteUserProcessModuleCommand = new DelegateCommand(DeleteUserProcessModuleAction);
            SaveProcessCommand = new DelegateCommand(SaveProcessAction);
            CompileCommand = new DelegateCommand(CompileAction);
            CreateUserProcessModuleCommand = new DelegateCommand(CreateUserProcessModuleAction);
            AddCommand = new DelegateCommand(AddAction);
            RemoveCommand = new DelegateCommand(RemoveAction);
            RunCommand = new DelegateCommand(RunAction);
        }
        private void InitEventSubscriptions()
        {
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


            _eventAggregator.GetEvent<OnFileDropped>().Subscribe(
                f =>
                {
                    var data = File.ReadAllBytes(f);
                    var reconstructed = MessagePack.MessagePackSerializer.Deserialize<UserProcessWrapper>(data);

                    var newUserProcessModuleView = new UserModuleView(_eventAggregator, reconstructed);
                    UserProcessModules.Add(newUserProcessModuleView);
                });

            new Thread(
                () =>
                {
                    Thread.Sleep(1000);
                    _userProcessWrapper = new UserProcessWrapper() { UserProcessName = "UserProcess1" };
                    _userProcessWrapper.SetEventAggregator(_eventAggregator);
                    //_userProcessWrapper.ImportedModuleNames = new List<string>() { "Controller1", "Controller2" };
                    SourceCode = _userProcessWrapper.ExecuteSourceCode;
                    RaisePropertyChanged(nameof(SourceCode));
                    _userProcessWrapper.LoadImportedModules();
                }).Start();

            _eventAggregator.GetEvent<OnUserProcessModuleSave>().Subscribe(
                (upw) =>
                {

                    for (int i = 0; i < UserProcess.Count; i++)
                    {
                        if(UserProcessModules.Select(upm=>upm.UserProcessName).Any(a=>a==UserProcess[i].UserProcessName))
                        {
                            var name = UserProcess[i].UserProcessName;
                            var updatedUpm = UserProcessModules.Where(upm => upm.UserProcessName == name).First();
                            //UserProcess.Remove(UserProcess[i]);
                            //UserProcess.Insert(i,updatedUpm);

                            UserProcess[i]._UserProcessWrapper = updatedUpm._UserProcessWrapper;
                        }
                    }




                    if (SelectedUserProcessModule != null)
                        SelectedUserProcessModule._UserProcessWrapper.ExecuteSourceCode = SourceCode;

                    upw.UserProcessWrapperCollection.Clear();

                    foreach (var item in UserProcess)
                    {
                        upw.UserProcessWrapperCollection.Add(item._UserProcessWrapper);
                    }

                    var data = MessagePack.MessagePackSerializer.Serialize<UserProcessWrapper>(upw);
                    File.WriteAllBytes(UserProcessWrapper.UserProcessModulePath + $@"\{upw.UserProcessName}.upm", data);
                });
        }
        #region -- COMMAND ACTIONS --
        private void SaveProcessAction()
        {
            if(UserProcess.Count>0)
            {
                var newUserProcessWrapper = new UserProcessWrapper(_eventAggregator) { UserProcessName = "new_user_process" };
                var newUserProcessModuleView = new UserModuleView(_eventAggregator, newUserProcessWrapper);
                UserProcessModules.Add(newUserProcessModuleView);

                foreach (var item in UserProcess)
                {
                    newUserProcessWrapper.UserProcessWrapperCollection.Add(item._UserProcessWrapper);
                }


                if (!Directory.Exists(UserProcessWrapper.UserProcessModulePath))
                    Directory.CreateDirectory(UserProcessWrapper.UserProcessModulePath);

                var data = MessagePack.MessagePackSerializer.Serialize<UserProcessWrapper>(newUserProcessWrapper);
                File.WriteAllBytes(UserProcessWrapper.UserProcessModulePath + $@"\new_user_process.upm", data);
            }

        }
        private void RunAction()
        {
            new Thread(
                () =>
                {
                    try
                    {
                        RaisePropertyChanged(nameof(CodeErrors));
                        _selectedUserProcessModule.ProgressValue=0;
                        _selectedUserProcessModule._UserProcessWrapper.SetEventAggregator(_eventAggregator);
                        _selectedUserProcessModule._UserProcessWrapper.ClearProgress();
                        bool result = _selectedUserProcessModule._UserProcessWrapper.ExecuteUserCode();
                    }
                    catch (Exception ex)
                    {
                        CodeErrors = ex.Message + "\n" + ex.InnerException?.Message;
                        RaisePropertyChanged(nameof(CodeErrors));
                    }
                }).Start();
        }
        private void DeleteUserProcessModuleAction()
        {
            try
            {
                File.Delete(UserProcessWrapper.UserProcessModulePath + $@"\{SelectedUserProcessModule.UserProcessName}.upm");
                UserProcessModules.Remove(SelectedUserProcessModule);
            }
            catch (Exception)
            {
            }
        }

        private void RemoveAction()
        {
            if (SelectedUserProcessModule != null)
                UserProcess.Remove(SelectedUserProcessModule);
        }
        private void AddAction()
        {
            if (SelectedUserProcessModule != null)
            {
                UserProcess.Add(SelectedUserProcessModule.Clone());
            }
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

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.Effects = DragDropEffects.Move;
        }

        public void Drop(IDropInfo dropInfo)
        {
            if(!dropInfo.IsSameDragDropContextAsSource)
                UserProcess.Add((dropInfo.Data as UserModuleView).Clone());
            else
            {
                UserProcess.Remove((UserModuleView)dropInfo.Data);
                UserProcess.Insert(dropInfo.InsertIndex,(UserModuleView)dropInfo.Data);
            }
        }
        #endregion
        #endregion


        #endregion
    }


    public static class Extensions
    {
        public static ObservableCollection<T> ToObservable<T>(this List<T> _myList)
        {
            return new ObservableCollection<T>(_myList);
        }
    }
}
