using System.Windows;
using GedViewWPF.DataAccess;
using GedViewWPF.ViewModel;

namespace GedViewWPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static MainWindow Instance;
		private readonly MainWindowViewModel _vm;

		public MainWindow()
		{
			InitializeComponent();

			Instance = this;

			_vm = new MainWindowViewModel();
			this.DataContext = _vm;
		}
	}
}
