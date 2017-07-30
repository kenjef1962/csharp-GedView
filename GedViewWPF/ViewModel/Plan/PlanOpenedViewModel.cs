using GedViewWPF.DataAccess;
using GedViewWPF.Utilities;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;
using GedViewWPF.Messaging;
using System;

namespace GedViewWPF.ViewModel
{
	class PlanOpenedViewModel : ViewModelBase
	{
		// Public Commands
		private ICommand _fileCloseCommand;
		public ICommand FileCloseCommand
		{
			get { return _fileCloseCommand ?? (_fileCloseCommand = new RelayCommand(ExecuteFileClose, CanExecuteFileClose)); }
		}

		// Public Properties
		public string GedcomFilename
		{
			get { return _dataMgr.GedcomFilename; }
		}

		public long FileSize
		{
			get { return _dataMgr.FileSize; }
		}

		public string FileLastOpened
		{
			get { return _dataMgr.FileLastOpened; }
		}

		public int PersonCount
		{
			get { return _dataMgr.PersonCount; }
		}

		public int FamilyCount
		{
			get { return _dataMgr.FamilyCount; }
		}

		public int FactCount
		{
			get { return _dataMgr.FactCount; }
		}

		public int RepositoryCount
		{
			get { return _dataMgr.RepositoryCount; }
		}

		public int SourceCount
		{
			get { return _dataMgr.SourceCount; }
		}

		public int MediaCount
		{
			get { return _dataMgr.MediaCount; }
		}

		public int NoteCount
		{
			get { return _dataMgr.NoteCount; }
		}

		public int SurnameCount
		{
			get { return _dataMgr.SurnameCount; }
		}

		public int MaleCount
		{
			get { return _dataMgr.MaleCount; }
		}

		public int FemaleCount
		{
			get { return _dataMgr.FemaleCount; }
		}

		// Private Properties
		private DataManager _dataMgr;

		public PlanOpenedViewModel(DataManager dataMgr)
		{
			_dataMgr = dataMgr;
		}

		#region Command Handlers
		private bool CanExecuteFileClose()
		{
			return ((_dataMgr != null) && _dataMgr.IsGedcomOpen);
		}
		private void ExecuteFileClose()
		{
			if (!CanExecuteFileClose()) return;

			Messenger.Default.Send(new MessageArgs(this), Messages.DoFileClose);
		}
		#endregion
	}
}
