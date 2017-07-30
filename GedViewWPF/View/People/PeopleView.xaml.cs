using System.Windows;
using System.Windows.Controls;
using GedViewWPF.ViewModel;

namespace GedViewWPF.View
{
	/// <summary>
	/// Interaction logic for PeopleView.xaml
	/// </summary>
	public partial class PeopleView : UserControl
	{
		public PeopleView()
		{
			InitializeComponent();
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			var vm = (PeopleViewModel)DataContext;
			vm.FilterByText = ((TextBox)sender).Text;
		}
	}
}
