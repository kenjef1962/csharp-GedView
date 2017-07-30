using System.Windows.Controls;
using GedViewWPF.ViewModel;

namespace GedViewWPF.View
{
	/// <summary>
	/// Interaction logic for PeopleView.xaml
	/// </summary>
	public partial class GedcomView : UserControl
	{
		private GedcomViewModel _vm;

		public GedcomView()
		{
			InitializeComponent();

			_vm = new GedcomViewModel();
			this.DataContext = _vm;
		}
	}
}
