using System;
using System.Collections.ObjectModel;
using System.IO;
using NameDateParsing;
using System.Collections.Generic;

namespace GedcomReader
{
	public class Gedcom
	{
		public static class Tokens
		{
            public const string Header = "HEAD";
            public const string Filename = "FILE";

            public const string Individual = "INDI";
			public const string Family = "FAM";
            public const string Repository = "REPO";
			public const string Source = "SOUR";
			public const string Media = "OBJE";

			public const string Husband = "HUSB";
			public const string Wife = "WIFE";
			public const string Child = "CHIL";

			public const string Name = "NAME";
            public const string Prefix = "NPFX";
            public const string Given = "GIVN";
            public const string Nickname = "NICK";
            public const string SurnamePrefix = "SPFX";
            public const string Surname = "SURN";
            public const string Suffix = "NSFX";

            public const string Sex = "SEX";

			// Individual Events / Attributes
			public const string Birth = "BIRT";
			public const string Death = "DEAT";
			public const string Burial = "BURI";

			// Family Events / Attributes
			public const string Annulment = "ANUL";
			public const string Census = "CENS";
			public const string Divorce = "DIV";
			public const string DivorceFiled = "DIVF";
			public const string Engagement = "ENGA";
			public const string Marriage = "MARR";
			public const string MarriageBann = "MARB";
			public const string MarriageContract = "MARC";
			public const string MarriageLicense = "MARL";
			public const string MarriageSettlement = "MARS";

			// LDS Events
			public const string BaptismLDS = "BAPL";
			public const string ConfirmationLDS = "CONL";
			public const string EndowmentLDS = "ENDL";
			public const string SealParentLDS = "SLGC";
			public const string SealSpouseLDS = "SLGS";

			public const string Event = "EVEN";

			public const string Status = "STAT";
			public const string Date = "DATE";
			public const string Place = "PLAC";
			public const string Temple = "TEMP";

			public const string Form = "FORM";

            public const string Authority = "AUTH";
            public const string Title = "TITL";
            public const string Abbreviation = "ABBR";
            public const string Publication = "PUBL";
			public const string Page = "PAGE";
			public const string Quality = "QUAY";

            public const string Address = "ADDR";
            public const string Email = "EMAIL";
            public const string Phone = "PHON";

			public const string Data = "DATA";
			public const string Text = "TEXT";
			public const string Note = "NOTE";

			public const string Concat = "CONC";
			public const string Continue = "CONT";

			public const string RefNo = "REFN";
			public const string Rin = "RIN";
		}

        public string Filename { get; set; }
        public GedcomHeader Header { get; set; }
        public Dictionary<string, GedcomPerson> People { get; set; }
        public Dictionary<string, GedcomFamily> Families { get; set; }
        public Dictionary<string, GedcomFact> Facts { get; set; }
        public Dictionary<string, GedcomPlace> Places { get; set; }
        public Dictionary<string, GedcomRepository> Repositories { get; set; }
        public Dictionary<string, GedcomSource> Sources { get; set; }
        public Dictionary<string, GedcomCitation> Citations { get; set; }
        public Dictionary<string, GedcomMedia> Media { get; set; }
        public Dictionary<string, GedcomNote> Notes { get; set; }

        private ObservableCollection<GedcomLine> GedcomLines { get; set; }

		public Gedcom(string filename)
		{
		    if (!File.Exists(filename)) return;

		    Filename = filename;
			People = new Dictionary<string, GedcomPerson>() ;
			Families = new Dictionary<string, GedcomFamily>();
            Facts = new Dictionary<string, GedcomFact>();
            Places = new Dictionary<string, GedcomPlace>();
            Repositories = new Dictionary<string, GedcomRepository>();
            Sources = new Dictionary<string, GedcomSource>();
            Citations = new Dictionary<string, GedcomCitation>();
            Media = new Dictionary<string, GedcomMedia>();
            Notes = new Dictionary<string, GedcomNote>();

			GedcomLines = new ObservableCollection<GedcomLine>();

			using (var stream = File.OpenText(filename))
			{
				ReadGedcom(stream);
				stream.Close();
			}

            // Fixups to be performed after the gedcome is read
            FixupPersonFacts();             // Add family facts to the person
            FixupCitationSourceTitle();     // Add the Source title to the citation
            FixupNotes();
		}

        private void FixupPersonFacts()
        {
            foreach (var person in People.Values)
            {
                foreach (var family in Families.Values)
                {
                    if ((family.HusbandID == person.ID) || (family.WifeID == person.ID))
                    {
                        foreach (var fact in family.Facts)
                        {
                            person.Facts.Add(fact);
                        }

                        break;
                    }
                }
            }
        }

        private void FixupCitationSourceTitle()
        {
            foreach (var citation in Citations.Values)
            {
                foreach (var source in Sources.Values)
                {
                    if (citation.SourceID == source.ID)
                    {
                        citation.SourceTitle = source.Title;
                        break;
                    }
                }
            }
        }

        private void FixupNotes()
        {
            var items = Facts.Values;
            foreach (var fact in items)
            {
                var notePairs = new List<Tuple<GedcomNote, GedcomNote>>();
                foreach (var note in fact.Notes)
                {
                    if (Notes.ContainsKey(note.ID))
                    {
                        notePairs.Add(new Tuple<GedcomNote, GedcomNote>(note, Notes[note.ID]));
                    }
                }

                foreach (var notePair in notePairs)
                {
                    fact.Notes.Remove(notePair.Item1);
                    fact.Notes.Add(notePair.Item2);
                }
            }

            foreach (var fact in Citations.Values)
            {
                var notePairs = new List<Tuple<GedcomNote, GedcomNote>>();
                foreach (var note in fact.Notes)
                {
                    if (Notes.ContainsKey(note.ID))
                    {
                        notePairs.Add(new Tuple<GedcomNote, GedcomNote>(note, Notes[note.ID]));
                    }
                }

                foreach (var notePair in notePairs)
                {
                    fact.Notes.Remove(notePair.Item1);
                    fact.Notes.Add(notePair.Item2);
                }
            }
        }

		private void ReadGedcom(StreamReader stream)
		{
            var line = GetNextLine(stream);
            while ((line != null) && (-1 < line.Level))
			{
                switch (line.Token)
                {
                    case Tokens.Header:
						ReadHeader(stream, line, out line);
                        break;

                    case Tokens.Individual:
						ReadPerson(stream, line, out line);
                        break;

                    case Tokens.Family:
						ReadFamily(stream, line, out line);
                        break;

					case Tokens.Repository:
                        ReadRepository(stream, line, out line);
                        break;

                    case Tokens.Source:
                        ReadSource(stream, line, out line);
                        break;

					case Tokens.Media:
						ReadMedia(stream, line, out line);
						break;

                    case Tokens.Note:
                        var note = ReadNote(stream, line, out line);
                        Notes.Add(note.ID, note);
                        break;

                    default:
                        ReadUnknownToken(stream, line, out line);
                        break;
                }
			}
		}

        private GedcomHeader ReadHeader(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            // Create and initialize the new person
            var header = new GedcomHeader();

            line = GetNextLine(stream);
            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case Tokens.Filename:
                        header.Filename = line.Data;
                        line = GetNextLine(stream);
                        break;

					case Tokens.Note:
                		var note = ReadNote(stream, line, out line);
						header.AddNote(note);
                		break;

                    default:
                        ReadUnknownToken(stream, line, out line);
                        break;
                }
            }

            Header = header;

			return header;
        }

		private GedcomPerson ReadPerson(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            var person = new GedcomPerson
            {
                ID = lineIn.Data
            };

            line = GetNextLine(stream);
            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case Tokens.Name:
                        var personName = ReadPersonName(stream, line, out line);
                        person.AddName(personName);
                        break;

                    case Tokens.Sex:
                        person.Sex = line.Data;
                        line = GetNextLine(stream);
                        break;

					case Tokens.Birth:
					case Tokens.Death:
					case Tokens.Burial:
						var fact = ReadFact(stream, person.ID, false, false, line, out line);
						person.AddFact(fact);
						break;

					case Tokens.BaptismLDS:
					case Tokens.ConfirmationLDS:
					case Tokens.EndowmentLDS:
					case Tokens.SealParentLDS:
						var factLDS = ReadFact(stream, person.ID, false, true, line, out line);
						person.AddFact(factLDS);
						break;

					case Tokens.RefNo:
						person.RefNo = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Rin:
						person.Rin = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Source:
						var citation = ReadSourceCitation(stream, line, out line);
						person.AddCitation(citation);
						break;

					case Tokens.Media:
						var media = ReadMedia(stream, line, out line);
						person.AddMedia(media);
						break;

					case Tokens.Note:
						var note = ReadNote(stream, line, out line);
						person.AddNote(note);
						break;

                    default:
                        ReadUnknownToken(stream, line, out line);
                        break;
                }
            }

            People.Add(person.ID, person);
            return person;
        }

        private GedcomPersonName ReadPersonName(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
			// Todo - Need better name parser
            var parser = new NameParser();
            parser.ParseName(lineIn.Data);

            var personName = new GedcomPersonName
            {
                Given = parser.Given,
                Surname = parser.Family
            };

            line = GetNextLine(stream);
            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case Tokens.Prefix:
                        personName.Given = line.Data;
                        line = GetNextLine(stream);
                        break;

                    case Tokens.Given:
                        personName.Given = line.Data;
                        line = GetNextLine(stream);
                        break;

                    case Tokens.Nickname:
                        personName.Nickname = line.Data;
                        line = GetNextLine(stream);
                        break;

					case Tokens.SurnamePrefix:
						personName.SurnamePrefix = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Surname:
						personName.Surname = line.Data;
						line = GetNextLine(stream);
						break;

                    case Tokens.Suffix:
                        personName.Suffix = line.Data;
                        line = GetNextLine(stream);
                        break;

					case Tokens.Source:
                		var citation = ReadSourceCitation(stream, line, out line);
                		personName.AddCitation(citation);
						break;

					case Tokens.Note:
                		var note = ReadNote(stream, line, out line);
                		personName.AddNote(note);
						break;

                    default:
                        ReadUnknownToken(stream, line, out line);
                        break;
                }
            }

            return personName;
        }

		private GedcomFamily ReadFamily(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
		{
			var family = new GedcomFamily
						 {
						 	ID = lineIn.Data
						 };

			line = GetNextLine(stream);
			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case Tokens.Husband:
						family.HusbandID = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Wife:
						family.WifeID = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Child:
						family.AddChild(line.Data);
						line = GetNextLine(stream);
						break;

					case Tokens.Marriage:
						var fact = ReadFact(stream, family.ID, true, false, line, out line);
						family.AddFact(fact);
						break;

					case Tokens.SealSpouseLDS:
						var factLDS = ReadFact(stream, family.ID, true, true, line, out line);
						family.AddFact(factLDS);
						break;

					case Tokens.RefNo:
						family.RefNo = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Rin:
						family.Rin = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Source:
						var citation = ReadSourceCitation(stream, line, out line);
						family.AddCitation(citation);
						break;

					case Tokens.Media:
						var media = ReadMedia(stream, line, out line);
						family.AddMedia(media);
						break;

					case Tokens.Note:
						var note = ReadNote(stream, line, out line);
						family.AddNote(note);
						break;

					default:
						ReadUnknownToken(stream, line, out line);
						break;
				}
			}

            Families.Add(family.ID, family);
			return family;
		}

		private int _factID = 1;
		private GedcomFact ReadFact(StreamReader stream, string xref, bool isShared, bool isLDS, GedcomLine lineIn, out GedcomLine line)
		{
			var fact = new GedcomFact
		    {
				ID = string.Format("@F{0}@", _factID++),
		        Type = lineIn.Token,
		        IsShared = isShared,
		        IsLDS = isLDS,
		        Xref = xref
		    };

			line = GetNextLine(stream);
			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case Tokens.Status:
						fact.Status = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Date:
                        fact.Date = ReadDate(line.Data);
						line = GetNextLine(stream);
						break;

					case Tokens.Place:
						var place = ReadPlace(stream, line, out line);
						fact.Place = place;
						break;

					case Tokens.Temple:
						fact.Description = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Event:
						line = GetNextLine(stream);
						break;

					case Tokens.Source:
						var citation = ReadSourceCitation(stream, line, out line);
						fact.AddCitation(citation);
						break;

					case Tokens.Note:
						var note = ReadNote(stream, line, out line);
						fact.AddNote(note);
						break;

					default:
						ReadUnknownToken(stream, line, out line);
						break;
				}
			}

			Facts.Add(fact.ID, fact);
			return fact;
		}

		private GedcomRepository ReadRepository(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
		{
			var repository = new GedcomRepository() 
			{ 
				ID = lineIn.Data 
			};

			line = GetNextLine(stream);
			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case Tokens.Name:
						repository.Name = line.Data;
                        line = GetNextLine(stream);
						break;

					case Tokens.Address:
						repository.Address = line.Data;
                        line = GetNextLine(stream);
						break;

					case Tokens.Email:
						repository.Email = line.Data;
                        line = GetNextLine(stream);
						break;

					case Tokens.Phone:
						repository.Phone = line.Data;
                        line = GetNextLine(stream);
						break;

					case Tokens.Note:
						var note = ReadNote(stream, line, out line);
						repository.AddNote(note);
						break;

					default:
						ReadUnknownToken(stream, line, out line);
						break;
				}
			}

            Repositories.Add(repository.ID, repository);
			return repository;
		}

		private GedcomSource ReadSource(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
		{
			var source = new GedcomSource() 
			{ 
				ID = lineIn.Data 
			};

			line = GetNextLine(stream);
			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case Tokens.Authority:
						var authority = ReadText(stream, line, out line);
						source.Authority = authority;
						break;

					case Tokens.Title:
						var title = ReadText(stream, line, out line);
						source.Title = title;
						break;

					case Tokens.Abbreviation:
						source.Abbreviation = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Publication:
						var publication = ReadText(stream, line, out line);
						source.Publication = publication;
						break;

					case Tokens.Text:
						var text = ReadText(stream, line, out line);
						source.Text = text;
						break;

					case Tokens.Repository:
						source.RepositoryID = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Media:
						var media = ReadMedia(stream, line, out line);
						source.AddMedia(media);
						break;

					case Tokens.Note:
						var note = ReadNote(stream, line, out line);
						source.AddNote(note);
						break;

					default:
						ReadUnknownToken(stream, line, out line);
						break;
				}
			}

            Sources.Add(source.ID, source);
			return source;
		}

		private int _citationID = 1;
		private GedcomCitation ReadSourceCitation(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
		{
			var citation = new GedcomCitation()
			{
			    ID = string.Format("@C{0}@", _citationID++),
				SourceID = lineIn.Data
			};

			line = GetNextLine(stream);
			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case Tokens.Page:
						citation.Page = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Text:
						var text = ReadText(stream, line, out line);
						citation.Text = text;
						break;

					case Tokens.Quality:
						citation.Quality = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Note:
						var note = ReadNote(stream, line, out line);
						citation.AddNote(note);
						break;

					default:
						ReadUnknownToken(stream, line, out line);
						break;
				}
			}

			Citations.Add(citation.ID, citation);
			return citation;
		}

		private GedcomMedia ReadMedia(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
		{
			var media = new GedcomMedia()
			{
			    ID = lineIn.Data
			};

			line = GetNextLine(stream);
			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case Tokens.Title:
						media.Title = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Filename:
						media.Filename = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Form:
						media.Format = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Note:
						var note = ReadNote(stream, line, out line);
						media.AddNote(note);
						break;

					default:
						ReadUnknownToken(stream, line, out line);
						break;
				}
			}

            if (!Media.ContainsKey(media.ID))
                Media.Add(media.ID, media);

			return media;
		}

        private static GedcomDate ReadDate(string dateStr)
        {
            var gedcomDate = new GedcomDate() { DateStr = dateStr, Modifier = 0 };

            // Figure out DateParser

            if (dateStr.Contains("ABT"))
            {
                dateStr = dateStr.Replace("ABT ", "");
                gedcomDate.Modifier = 0;
            }
            if (dateStr.Contains("AFT"))
            {
                dateStr = dateStr.Replace("AFT ", "");
                gedcomDate.Modifier = 1;
            }
            if (dateStr.Contains("BEF"))
            {
                dateStr = dateStr.Replace("BEF ", "");
                gedcomDate.Modifier = -1;
            }
            else if (dateStr.Contains("BET"))
            {
                dateStr = dateStr.Replace("BET ", "");
                dateStr = dateStr.Substring(0, dateStr.IndexOf("AND") - 1);
                gedcomDate.Modifier = 1;
            }

            if (dateStr.Length == 4)
            {
                int year;
                if (int.TryParse(dateStr, out year))
                {
                    gedcomDate.Date = new DateTime(int.Parse(dateStr), 1, 1);
                }
            }
            else
            {
                DateTime dt;
                if (DateTime.TryParse(dateStr, out dt))
                {
                    gedcomDate.Date = dt;
                }
            }

            return gedcomDate;
        }

		private int _placeID = 1;
		private GedcomPlace ReadPlace(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
		{
			var place = new GedcomPlace
			{
			    ID = string.Format("@P{0}@", _placeID++),
			    Place = lineIn.Data
			};

			line = GetNextLine(stream);
			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case Tokens.Form:
						place.Form = line.Data;
						line = GetNextLine(stream);
						break;

					// Todo - Handle Source Citation
					case Tokens.Source:
						ReadUnknownToken(stream, line, out line);
						break;

					case Tokens.Note:
						var note = ReadNote(stream, line, out line);
						place.Notes.Add(note);
						break;

					default:
						ReadUnknownToken(stream, line, out line);
						break;
				}
			}

            Places.Add(place.ID, place);
			return place;
		}

        private GedcomNote ReadNote(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            var note = new GedcomNote()
            {
                ID = lineIn.Data
            };

            var text = string.Empty;

            line = GetNextLine(stream);
            while ((line != null) && (lineIn.Level < line.Level))
            {
                switch (line.Token)
                {
                    case Tokens.Text:
                        text = line.Data;
                        line = GetNextLine(stream);
                        break;

                    case Tokens.Concat:
                        if (!string.IsNullOrEmpty(text) && !text.EndsWith(" "))
                            text += " ";
                        text += line.Data;

                        line = GetNextLine(stream);
                        break;

                    case Tokens.Continue:
                        if (!string.IsNullOrEmpty(text) && !text.EndsWith("\r\n"))
                            text += "\r\n";
                        text += line.Data;

                        line = GetNextLine(stream);
                        break;

                    default:
                        ReadUnknownToken(stream, line, out line);
                        break;
                }
            }

            note.Text = text;
            return note;
        }

		private string ReadText(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
		{
			var text = lineIn.Data;

			line = GetNextLine(stream);
			while ((line != null) && (lineIn.Level < line.Level))
			{
				switch (line.Token)
				{
					case Tokens.Text:
						text = line.Data;
						line = GetNextLine(stream);
						break;

					case Tokens.Concat:
						if (!string.IsNullOrEmpty(text) && !text.EndsWith(" "))
							text += " ";
						text += line.Data;

						line = GetNextLine(stream);
						break;

					case Tokens.Continue:
						if (!string.IsNullOrEmpty(text) && !text.EndsWith("\r\n"))
							text += "\r\n";
						text += line.Data;

						line = GetNextLine(stream);
						break;

					default:
						ReadUnknownToken(stream, line, out line);
						break;
				}
			}

			return text.Trim();
		}

        private void ReadUnknownToken(StreamReader stream, GedcomLine lineIn, out GedcomLine line)
        {
            line = GetNextLine(stream);
            while ((line != null) && (lineIn.Level < line.Level))
            {
                line = GetNextLine(stream);
            }
        }


        private GedcomLine GetNextLine(StreamReader stream)
        {
            var lineIn = stream.ReadLine();
            if (string.IsNullOrEmpty(lineIn)) return null;

            var lineParts = lineIn.Trim().Split(new[] { ' ' }, 3);

            var line = new GedcomLine
                           {
                               Level = (0 < lineParts.Length) ? int.Parse(lineParts[0]) : -1,
                               Token = (1 < lineParts.Length) ? lineParts[1] : string.Empty,
                               Data = (2 < lineParts.Length) ? lineParts[2] : string.Empty
                           };

            if (line.Token.StartsWith("@"))
            {
                line.Token = (2 < lineParts.Length) ? lineParts[2] : string.Empty;
                line.Data = (1 < lineParts.Length) ? lineParts[1] : string.Empty;

                line.IsXref = true;
            }

			GedcomLines.Add(line);
            return line;
        }
    }
}
