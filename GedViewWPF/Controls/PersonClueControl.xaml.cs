using System.Windows.Controls;
using GedViewWPF.ViewModel;
using GedViewWPF.Model;
using System.Diagnostics;

namespace GedViewWPF.Controls
{
	/// <summary>
	/// Interaction logic for PeopleView.xaml
	/// </summary>
	public partial class PersonClueControl : UserControl
	{
        public PersonClueControl()
		{
			InitializeComponent();
		}

        private void UrlClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Clue clue = DataContext as Clue;
            Process.Start(clue.Url);
        }
	}
}
