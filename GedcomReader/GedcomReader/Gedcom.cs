using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedcomReader
{
    public class Gedcom
    {
        public string Filename { get; set; }
        public GedcomHeader Header { get; set; }
        public Dictionary<string, GedcomIndividual> Individuals { get; set; }
        public Dictionary<string, GedcomFamily> Families { get; set; }
        public Dictionary<string, GedcomFact> Facts { get; set; }
        public Dictionary<string, GedcomRepository> Repositories { get; set; }
        public Dictionary<string, GedcomSource> Sources { get; set; }
        public Dictionary<string, GedcomMedia> Media { get; set; }
        public Dictionary<string, GedcomNote> Notes { get; set; }

		private static List<GedcomLogItem> _logItems;
		private static Stopwatch _logTimer;

        public Gedcom(string path, bool showLog = false)
        {
			// Initialize logging variables
            _logItems = new List<GedcomLogItem>();
            _logTimer = new Stopwatch();
            _logTimer.Start();

			// Initialize Data properties
            Filename = path;
            Header = null;
            Individuals = new Dictionary<string, GedcomIndividual>();
            Families = new Dictionary<string, GedcomFamily>();
            Facts = new Dictionary<string, GedcomFact>();
            Repositories = new Dictionary<string, GedcomRepository>();
            Sources = new Dictionary<string, GedcomSource>();
            Media = new Dictionary<string, GedcomMedia>();
            Notes = new Dictionary<string, GedcomNote>();

			// Reset IDs / counters
            GedcomCitation.CurrentID = 1;
            GedcomFact.CurrentID = 1;
            GedcomLine.CurrentLineNo = 1;

			// Read the Gedcom file
            ReadGedcom(path, showLog);
        }

		public bool IsValid()
		{
			if (string.IsNullOrEmpty(Filename))
				return false;

			if (Header == null)
				return false;

			if ((Individuals.Count == 0) &&
				(Families.Count == 0) &&
				(Facts.Count == 0) &&
				(Repositories.Count == 0) &&
				(Sources.Count == 0) &&
				(Media.Count == 0) &&
				(Notes.Count == 0))
			{
				return false;
			}

			return true;
		}
        internal void ReadGedcom(string path, bool showLog = false)
        {
			// Check to make sure the Gedcom file exists
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return;
			}

            // Open and read the Gedcom file
            using (var stream = File.OpenText(path))
            {
				// While there are lines to read, continue to read
                var line = GedcomLine.Read(stream);
                while ((line != null) && (-1 < line.Level))
			    {
                    switch (line.Token)
                    {
                        case "HEAD":	// <HEADER>
                            Header = GedcomHeader.Read(stream, line, out line);
                            break;

                        case "INDI":	// <INDIVIDUAL_RECORD>
                            var individual = GedcomIndividual.Read(stream, line, out line);
                            if (individual != null)
                            {
                                Individuals.Add(individual.ID, individual);
                            }
                            break;

                        case "FAM":		// <FAMILY_RECORD>
                            var family = GedcomFamily.Read(stream, line, out line);
                            if (family != null)
                            {
                                Families.Add(family.ID, family);
                            }
                            break;

                        case "REPO":	// <REPOSITORY_RECORD>
                            var repository = GedcomRepository.Read(stream, line, out line);
                            if (repository != null)
                            {
                                Repositories.Add(repository.ID, repository);
                            }
                            break;

                        case "SOUR":	// <SOURCE_RECORD>
                            var source = GedcomSource.Read(stream, line, out line);
                            if (source != null)
                            {
                                Sources.Add(source.ID, source);
                            }
                            break;

                        case "OBJE":	// <MULTIMEDIA_RECORD>
                            var media = GedcomMedia.Read(stream, line, out line);
                            if (media != null)
                            {
                                Media.Add(media.ID, media);
                            }
                            break;

                        case "NOTE":	// <NOTE_RECORD>
                            var note = GedcomNote.Read(stream, line, out line);
                            if (note != null)
                            {
                                Notes.Add(note.ID, note);
                            }
                            break;

                        case "TRLR":	// <TRAILER>
                            line = null;
                            break;

                        default:		// Unknown / unsupported tags
                            line = ReadToken.ReadUnsupportedToken(stream, line);
                            break;
                    }
                }
            }

            foreach (var individual in Individuals.Values)
            {
                foreach (var fact in individual.Facts)
                {
                    fact.PrimaryID = individual.ID;
                    Facts.Add(fact.ID, fact);
                }
            }

            foreach (var family in Families.Values)
            {
                foreach (var fact in family.Facts)
                {
                    fact.PrimaryID = family.HusbandID;
                    fact.SecondaryID = family.HusbandID;
                    Facts.Add(fact.ID, fact);

                    if (fact.PrimaryID != null)
                    {
                        var gedcomPerson = Individuals[fact.PrimaryID];
                        gedcomPerson.Facts.Add(fact);
                    }
                    if (fact.SecondaryID != null)
                    {
                        var gedcomPerson = Individuals[fact.SecondaryID];
                        gedcomPerson.Facts.Add(fact);
                    }
                }
            }

            _logTimer.Stop();

            if (showLog)
            {
                ViewGedcomLog();
            }
        }

        public void ViewGedcomLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("----------------------------------------------------------------------------------------------------");
            sb.AppendLine("Gedcom Reader - Log File");
            sb.AppendLine("----------------------------------------------------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine(string.Format("Filename:  \t{0}", Filename));
            sb.AppendLine(string.Format("Read Time: \t{0}", _logTimer.Elapsed.ToString()));
            sb.AppendLine();

            sb.AppendLine("----------------------------------------------------------------------------------------------------");
            sb.AppendLine("Gedcom items:");
            sb.AppendLine();
            sb.AppendLine(string.Format("Individuals:  \t{0}", Individuals.Count.ToString()));
            sb.AppendLine(string.Format("Families:     \t{0}", Families.Count.ToString()));
            sb.AppendLine(string.Format("Repositories: \t{0}", Repositories.Count.ToString()));
            sb.AppendLine(string.Format("Sources:      \t{0}", Sources.Count.ToString()));
            sb.AppendLine(string.Format("Media:        \t{0}", Media.Count.ToString()));
            sb.AppendLine(string.Format("Notes:        \t{0}", Notes.Count.ToString()));
            sb.AppendLine();

            sb.AppendLine("----------------------------------------------------------------------------------------------------");

            if (_logItems != null)
            {
                sb.AppendLine(string.Format("Log items: ({0})", _logItems.Count));
                sb.AppendLine();

                int count = 1;
                foreach (var logItem in _logItems)
                {
                    sb.AppendLine(string.Format("{0}. {1} (line {2})", count++, logItem.Message, logItem.LineNo));
                    foreach (var line in logItem.Lines)
                    {
                        sb.AppendLine(string.Format("   {0}\t{1}\t{2}", line.Level, line.Token, line.Data));
                    }
                    sb.AppendLine();
                }
            }

            var logFilename = Filename.Replace(".ged", ".txt");
            File.WriteAllText(logFilename, sb.ToString());

            Process.Start(logFilename);
        }

        internal static void WriteLogItem(GedcomLogItem logItem)
        {
			if (_logItems != null)
			{
				_logItems.Add(logItem);
			}
        }

		internal static void WriteLogItem(string message, GedcomLine line)
		{
			Gedcom.WriteLogItem(new GedcomLogItem(message, line));
		}
	}
}
