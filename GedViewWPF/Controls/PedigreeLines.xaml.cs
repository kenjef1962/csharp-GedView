using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GedViewWPF.Controls
{
	/// <summary>
	/// Interaction logic for PedigreeLines.xaml
	/// </summary>
	public partial class PedigreeLines : UserControl
	{
		public bool IsTop { get; set; }
		public double Offset { get; set; }

		private static Pen PedigreeLinePen { get; set; }

		public PedigreeLines()
		{
			InitializeComponent();

			if (PedigreeLinePen == null)
			{
				PedigreeLinePen = new Pen(Brushes.Gray, 1);
			}
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			var pt1 = new Point(ActualWidth * .75, ActualHeight * Offset);
			var pt2 = new Point(ActualWidth, ActualHeight * Offset);
			var pt3 = new Point(ActualWidth * .75, IsTop ? ActualHeight : 0);

			dc.DrawLine(PedigreeLinePen, pt1, pt2);	// Horzontal
			dc.DrawLine(PedigreeLinePen, pt1, pt3);	// Vertical
		}
	}
}
