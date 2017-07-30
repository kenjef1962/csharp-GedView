using System.Windows.Controls;
using GedViewWPF.ViewModel;
using GedViewWPF.Model;
using GalaSoft.MvvmLight.Messaging;
using System;
using GedViewWPF.Messaging;

namespace GedViewWPF.Controls
{
	/// <summary>
	/// Interaction logic for PeopleView.xaml
	/// </summary>
	public partial class PedigreeButtonControl : UserControl
	{
		public PedigreeButtonControl()
		{
			InitializeComponent();
        }

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			Messenger.Default.Send(new MessageArgs(this, DataContext), "SelectedPersonChanged");
		}
	}
}
