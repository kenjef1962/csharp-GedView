using System.Windows;
using System.Windows.Controls;

namespace GedViewWPF.Extensions
{
	public static class DataGridExtension
	{
		public static void ToClipboard(this DataGrid dataGrid)
		{
            MessageBox.Show("ToClipboard() not implemented", "DataGridExtensions", MessageBoxButton.OK, MessageBoxImage.Information);
        }
		public static void ToCSV(this DataGrid dataGrid)
		{
			MessageBox.Show("ToCSV() not implemented", "DataGridExtensions", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		public static void ToExcel(this DataGrid dataGrid)
		{
			MessageBox.Show("ToExcel() not implemented", "DataGridExtensions", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
