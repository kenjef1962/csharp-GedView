using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GedViewWPF.DataAccess;
using GedViewWPF.Messaging;
using Microsoft.Win32;

namespace GedViewWPF.ViewModel
{
	class MainWindowViewModel : ViewModelBase
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
			get { return _fileOpenCommand ?? (_fileOpenCommand = new RelayCommand<string>(ExecuteFileOpen, CanExecuteFileOpen)); }
		}

		private ICommand _fileCloseCommand;
		public ICommand FileCloseCommand
		{
			get { return _fileCloseCommand ?? (_fileCloseCommand = new RelayCommand(ExecuteFileClose, CanExecuteFileClose)); }
		}

		private ICommand _fileExitCommand;
		public ICommand FileExitCommand
		{
			get { return _fileExitCommand ?? (_fileExitCommand = new RelayCommand(ExecuteFileExit, CanExecuteFileExit)); }
		}

		private ICommand _viewNavigateCommand;
		public ICommand ViewNavigateCommand
		{
			get { return _viewNavigateCommand ?? (_viewNavigateCommand = new RelayCommand<string>(ExecuteViewNavigate, CanExecuteViewNavigate)); }
		}

		// Public Properties
        private object _currentVM;
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

		// Private Properties
		private readonly DataManager _dataMgr;

		private PlanViewModel _planVM;
		private PeopleViewModel _peopleVM;

		public MainWindowViewModel()
		{
			// Setup handlers messaging event handlers
            Messenger.Default.Register<MessageArgs>(this, Messages.DoFileNew, DoFileNew);
			Messenger.Default.Register<MessageArgs>(this, Messages.DoFileOpen, DoFileOpen);
			Messenger.Default.Register<MessageArgs>(this, Messages.DoFileClose, DoFileClose);
			Messenger.Default.Register<MessageArgs>(this, Messages.DoFileExit, DoFileExit);

			Messenger.Default.Register<MessageArgs>(this, "DoViewNavigate", DoViewNavigate);

			_dataMgr = new DataManager();

            InitializeViews();
        }

		private void InitializeViews()
		{
			_currentVM = null;
			_planVM = null;
			_peopleVM = null;

            ExecuteViewNavigate("Plan");
        }

		#region File New Command / Message Handling
		private void DoFileNew(MessageArgs args)
		{
			ExecuteFileNew();
		}

		private bool CanExecuteFileNew()
		{
			return true;
		}

		private void ExecuteFileNew()
		{
			if (!CanExecuteFileNew()) return;

			MessageBox.Show("Functionality not implemented");
		}
		#endregion

		#region File Open Command / Message Handling
		private void DoFileOpen(MessageArgs args)
		{
			var filename = args.Data as string;
			ExecuteFileOpen(filename);
		}

		private bool CanExecuteFileOpen(string filename)
		{
			return true;
		}

		private void ExecuteFileOpen(string filename)
		{
			if (!CanExecuteFileOpen(filename)) return;

			// If the filename is blank or the file does not exist, prompt the user for the file
			if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
			{
				var dlg = new OpenFileDialog
				{
					CheckFileExists = true,
					CheckPathExists = true,
					InitialDirectory = "C:\\Temp",
					FileName = "*.ged"
				};

				if ((bool)dlg.ShowDialog())
				{
					filename = dlg.FileName;
				}
				else
				{
					return;
				}
			}

			// If the filename is blank or the file does not exist, let the user know
			if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
			{
				var message = string.Format(Properties.Resources.FilenameBlank, filename);
				MessageBox.Show(message, 
								Properties.Resources.AppTitle, 
								MessageBoxButton.OK, 
								MessageBoxImage.Warning);
				return;
			}

			// Open the file and navigate to the plan area
			_dataMgr.OpenGedcom(filename);

			// Update last file MRU
			if (!Properties.Settings.Default.LastOpenFileMRU.Contains(filename))
			{
				Properties.Settings.Default.LastOpenFileMRU = filename + "|" + Properties.Settings.Default.LastOpenFileMRU;
				Properties.Settings.Default.Save();
			}
			
			InitializeViews();
		}
		#endregion

		#region File Close Command / Message Handling
		private void DoFileClose(MessageArgs args)
		{
			ExecuteFileClose();
		}

		public bool CanExecuteFileClose()
		{
			return ((_dataMgr != null) && _dataMgr.IsGedcomOpen);
		}

		private void ExecuteFileClose()
		{
			if (!CanExecuteFileClose()) return;

			_dataMgr.CloseGedcom();
			InitializeViews();
		}
		#endregion

		#region File Exit Command / Message Handling
		private void DoFileExit(MessageArgs args)
		{
			ExecuteFileExit();
		}

		private bool CanExecuteFileExit()
		{
			return true;
		}

		private void ExecuteFileExit()
		{
			if (!CanExecuteFileExit()) return;

			var dr = MessageBox.Show(Properties.Resources.AppExitConfirm,
									 Properties.Resources.AppTitle,
									 MessageBoxButton.YesNo,
									 MessageBoxImage.Question);

			if (dr == MessageBoxResult.Yes)
			{
				_dataMgr.CloseGedcom();
				Properties.Settings.Default.Save();
				Application.Current.Shutdown();
			}
		}
		#endregion

		#region View Navigate Command / Message Handling
		public void DoViewNavigate(MessageArgs args)
		{
			var view = args.Data as string;

			ExecuteViewNavigate(view);
		}

		public bool CanExecuteViewNavigate(string view)
		{
			return (view.Equals("Plan") || 
				   ((_dataMgr != null) && _dataMgr.IsGedcomOpen));
		}

		public void ExecuteViewNavigate(string view)
		{
			if (!CanExecuteViewNavigate(view)) return;

			switch (view)
			{
				case "Plan":
					if (_planVM == null)
						_planVM = new PlanViewModel(_dataMgr);

                    _planVM.RefreshMRU();
					CurrentVM = _planVM;
					break;

				case "People":
					if (_peopleVM == null)
						_peopleVM = new PeopleViewModel(_dataMgr);

					CurrentVM = _peopleVM;
					break;

				default:
					break;
			}
		}
		#endregion
	}
}
