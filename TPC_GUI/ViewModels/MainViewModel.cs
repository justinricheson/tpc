using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TPC.Group;
using TPC.Utilities;

namespace TPC_GUI.ViewModels
{
    public class MainViewModel : ViewModel, IDisposable
    {
        #region Vars
        public string Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    NotifyPropertyChanged("Image");
                }
            }
        }
        private string _image;
        public string Icon
        {
            get { return _icon; }
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    NotifyPropertyChanged("Icon");
                }
            }
        }
        private string _icon;
        public ObservableCollection<string> StatusItems { get; private set; }
        public ICommand Join { get; private set; }
        public ICommand Leave { get; private set; }
        public ICommand Clear { get; private set; }
        private Coordinator _coordinator;
        private BackgroundWorker _coordinatorThread;
        #endregion

        #region Constructor
        public MainViewModel()
        {
            Icon = "/TPC_GUI;component/Images/Light_Off.png";
            Image = "/TPC_GUI;component/Images/Light_Off.png";
            StatusItems = new ObservableCollection<string>();
            Join = new RelayCommand(JoinGroup);
            Leave = new RelayCommand(LeaveGroup);
            Clear = new RelayCommand(ClearStatusItems);
            LogBroker.OnLog += LogBroker_OnLog;
            _coordinator = new Coordinator();

            _coordinatorThread = new BackgroundWorker();
            _coordinatorThread.DoWork += _coordinatorThread_DoWork;
            _coordinatorThread.RunWorkerCompleted += _coordinatorThread_RunWorkerCompleted;
        }
        #endregion

        #region Public
        public void Dispose()
        {
            if (_coordinator != null)
                _coordinator.Dispose();
        }
        #endregion

        #region Private
        private void _coordinatorThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
        private void _coordinatorThread_DoWork(object sender, DoWorkEventArgs e)
        {
            switch (e.Argument as string)
            {
                case "JOIN":
                    _coordinator.Join();
                    break;
                case "LEAVE":
                    _coordinator.Leave();
                    break;
            }
        }
        private void LogBroker_OnLog(object sender, LoggerEventArgs e)
        {
            try
            {
                if (Application.Current != null) // Null when window is closing
                {
                    Application.Current.Dispatcher.Invoke(
                    (Action)delegate()
                    {
                        lock (StatusItems)
                            StatusItems.Add(e.Event);

                        if (e.Event.Contains("Join Complete"))
                            Icon = Image = "/TPC_GUI;component/Images/Light_On.png";
                        else if (e.Event.Contains("Leave Complete"))
                            Icon = Image = "/TPC_GUI;component/Images/Light_Off.png";
                    },
                    DispatcherPriority.Render);
                }
            }
            catch
            {
                // Just ignore, this may happen if the task
                // created by the dispatcher is cancelled before
                // it's executed like when the program is closed
            }
        }
        private void JoinGroup()
        {
            if (!_coordinatorThread.IsBusy)
                _coordinatorThread.RunWorkerAsync("JOIN");
        }
        private void LeaveGroup()
        {
            if (!_coordinatorThread.IsBusy)
                _coordinatorThread.RunWorkerAsync("LEAVE");
        }
        private void ClearStatusItems()
        {
            if (StatusItems.Count > 0)
            {
                lock (StatusItems)
                {
                    var idItem = StatusItems[0];
                    StatusItems.Clear();
                    StatusItems.Add(idItem);
                }
            }
        }
        #endregion
    }
}
