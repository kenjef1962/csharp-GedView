using GedViewWPF.DataAccess;
using GedViewWPF.Utilities;

namespace GedViewWPF.ViewModel
{
	class PlanClosedViewModel : ViewModelBase
	{
		private DataManager _dataMgr;

		public PlanClosedViewModel(DataManager dataMgr)
		{
			_dataMgr = dataMgr;
		}
	}
}
