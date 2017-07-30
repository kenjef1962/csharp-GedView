using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GedViewWPF.DataAccess;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using GedViewWPF.Messaging;

namespace GedViewWPF.ViewModel
{
	class PlanViewModel : ViewModelBase
	{
		// Public Commands
        private ICommand _fileNewCommand;
        public ICommand FileNewCommand
        {
            get { return _fileNewCommand ?? (_fileNewCommand = new RelayCommand(ExecuteFileNew, CanExecuteFileNew)); }
        }

        private ICommand _fileOpenCommand;
        public ICommand FileOpenCommand
        {
            get { return _fileOpenCommand ?? (_fileOpenCommand = new RelayCommand(ExecuteFileOpen, CanExecuteFileOpen)); }
        }

        private ICommand _gedcomOpenCommand;
        public ICommand GedcomOpenCommand
        {
            get { return _gedcomOpenCommand ?? (_gedcomOpenCommand = new RelayCommand(ExecuteGedcomOpen, CanExecuteGedcomOpen)); }
        }

        // Public Properties
		public object CurrentVM
		{
			get { return _currentVM; }
			set
			{
				if (_currentVM == value) return;

				_currentVM = value;
				base.OnPropertyChanged("CurrentVM");
			}
		}

        private List<string> _gedcomFileList;
        public List<string> GedcomFileList
        {
            get { return _gedcomFileList;}
            set
            {
                _gedcomFileList = value;
                OnPropertyChanged("GedcomFileMRU");
            }
        }

        private string _gedcomFilename;
        public string GedcomFilename
        {
            get { return _gedcomFilename; }
            set
            {
                _gedcomFilename = value;
				OnPropertyChanged("GedcomFilename");
            }
        }

		// Private Properties
		private readonly DataManager _dataMgr;

		private object _currentVM;

		public PlanViewModel(DataManager dataMgr)
		{
			_dataMgr = dataMgr;
			if ((_dataMgr != null) && _dataMgr.IsGedcomOpen)
			{
				_currentVM = new PlanOpenedViewModel(_dataMgr);
			}
			else
			{
				_currentVM = new PlanClosedViewModel(_dataMgr);
			}

            RefreshMRU();
		}

        public void RefreshMRU()
        {
            var fileMRU = Properties.Settings.Default.LastOpenFileMRU.Split(new[] { '|' });
            var filenames = new List<string>();

            foreach (var filename in fileMRU)
            {
                if (File.Exists(filename))
                {
                    filenames.Add(filename);
                }
            }

            GedcomFileList = new List<string>(filenames);
            GedcomFilename = Properties.Settings.Default.LastOpenFilename;
        }

		#region Command Handlers
		private bool CanExecuteFileNew()
		{
			return true;
		}
		private void ExecuteFileNew()
        {
			if (!CanExecuteFileNew()) return;

            Messenger.Default.Send(new MessageArgs(this), Messages.DoFileNew);
        }

		private bool CanExecuteFileOpen()
		{
			return true;
		}
		private void ExecuteFileOpen()
		{
			if (!CanExecuteFileOpen()) return;

			Messenger.Default.Send(new MessageArgs(this), Messages.DoFileOpen);
		}

        private bool CanExecuteGedcomOpen()
        {
            return (0 < GedcomFileList.Count);
        }
		private void ExecuteGedcomOpen()
        {
			if (!CanExecuteGedcomOpen()) return;

			Messenger.Default.Send(new MessageArgs(this, GedcomFilename), Messages.DoFileOpen);
        }
        #endregion
	}
}
