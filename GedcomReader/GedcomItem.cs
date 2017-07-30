using System.Collections.ObjectModel;

namespace GedcomReader
{
    public class GedcomItem
    {
		public string ID { get; set; }
		public string Status { get; set; }
		public ObservableCollection<GedcomCitation> Citations { get; set; }
		public ObservableCollection<GedcomMedia> Media { get; set; }
		public ObservableCollection<GedcomNote> Notes { get; set; }

		public GedcomItem()
		{
			Citations = new ObservableCollection<GedcomCitation>();
			Media = new ObservableCollection<GedcomMedia>();
            Notes = new ObservableCollection<GedcomNote>();
		}

		public void AddCitation(GedcomCitation citation)
		{
			Citations.Add(citation);
		}

		public void AddMedia(GedcomMedia media)
		{
			Media.Add(media);
		}

        public void AddNote(GedcomNote note)
		{
			Notes.Add(note);
		}
	}
}
