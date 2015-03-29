using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace ComputationalCluster.ComputationalClient.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            _cClient = new ComputationalClientRunner();
        }

        private ComputationalClientRunner _cClient;
        //================================
        private ICommand _loadFileCommand;
        private ICommand _sendSolveRequestCommand;
        private ICommand _sendSolutionRequestCommand;
        public string _fileContent;
        private string _problemType;
        private int _lastSolveRequestId;
        private int _timeout;
        private int _problemId;
        //================================

        public string ProblemType
        {
            get { return _problemType; }
            set
            {
                _problemType = value;
                RaisePropertyChanged(() => ProblemType);
            }
        }
        public ICommand LoadFileCommand
        {
            get
            {
                if (_loadFileCommand == null)
                {
                    _loadFileCommand = new RelayCommand(LoadFile, () => { return String.IsNullOrWhiteSpace(_fileContent); });
                }
                return _loadFileCommand;
            }
        }
        public ICommand SendSolveRequestCommand
        {
            get
            {
                if (_sendSolveRequestCommand == null)
                {
                    _sendSolveRequestCommand = new RelayCommand(SendSolveRequest, () =>
                    {
                        return !String.IsNullOrWhiteSpace(_fileContent) && !String.IsNullOrWhiteSpace(ProblemType);
                    });
                }
                return _sendSolveRequestCommand;
            }
        }
        public int LastSolveRequestId
        {
            get { return _lastSolveRequestId; }
            set
            {
                _lastSolveRequestId = value;
                RaisePropertyChanged();
            }
        }
        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
                RaisePropertyChanged();
            }
        }
        public int ProblemId
        {
            get
            {
                return _problemId;
            }
            set
            {
                _problemId = value;
                RaisePropertyChanged(() => ProblemId);
            }
        }
        public ICommand SendSolutionRequestCommand
        {
            get
            {
                if(_sendSolutionRequestCommand == null)
                {
                    _sendSolutionRequestCommand = new RelayCommand(SendSolutionRequest, () => { return ProblemId > 0; });
                }
                return _sendSolutionRequestCommand;
            }
        }


        private void LoadFile()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                using (var sr = new StreamReader(ofd.OpenFile()))
                {
                    _fileContent = sr.ReadToEnd();
                }
            }
        }
        private void SendSolveRequest()
        {
            ulong problemId;
            if (Timeout != 0)
            {
                problemId = _cClient.SendSolveRequest(_fileContent, ProblemType, (ulong)Timeout);
            }
            else
            {
                problemId = _cClient.SendSolveRequest(_fileContent, ProblemType);
            }

            MessageBox.Show(String.Format("Wys�ane!\nId przdzielone przez serwer to: {0}", problemId));

            _fileContent = null;
            ProblemType = null;
            Timeout = 0;
        }
        private void SendSolutionRequest()
        {
            var result = _cClient.SendSolutionRequest(ProblemId);
            if( result == null )
            {
                MessageBox.Show("Obliczenia nie zosta�y jeszcze zako�czone!");
                return;
            }
            var sfd = new Microsoft.Win32.SaveFileDialog();
            if( sfd.ShowDialog() == true)
            {
                using(var sw = new StreamWriter(sfd.OpenFile()))
                {
                    sw.Write(result);
                }
            }
        }
    }
}