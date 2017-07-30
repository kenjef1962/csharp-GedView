using System.Collections.ObjectModel;
using GedViewWPF.DataAccess;
using GedViewWPF.Model;
using GedViewWPF.Utilities;

namespace GedViewWPF.ViewModel
{
	class GedcomViewModel : ViewModelBase
	{
		private GedLine _selectedItem;
		private int _selectedIndex;

		public string FileName
		{
			get
			{
				DataManager dataMgr = Utils.GetDataManager();
				return dataMgr.GetGedcomFilename();
			}
		}

		public ObservableCollection<GedLine> GedLines
		{
			get
			{
				DataManager dataMgr = Utils.GetDataManager();
				return dataMgr.GetGedcomGedLines();
			}
		}

		public GedLine SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				_selectedItem = value;
				base.OnPropertyChanged("SelectedItem");
			}
		}
		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set { _selectedIndex = value; OnPropertyChanged("SelectedIndex"); }
		}

		public GedcomViewModel()
		{
		}
	}
}
