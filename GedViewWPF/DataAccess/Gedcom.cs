using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using GedViewWPF.Model;

namespace GedViewWPF.DataAccess
{
	public class Gedcom
	{
		private static class Tokens
		{
			public const string Individual = "INDI";
			public const string Family = "FAM";
            public const string Repository = "REPO";
			public const string Source = "SOUR";
			public const string Media = "OBJE";

			public const string Husband = "HUSB";
			public const string Wife = "WIFE";
			public const string Child = "CHIL";

			public const string Name = "NAME";
			public const string Surname = "SURN";
			public const string Given = "GIVN";

            public const string Sex = "SEX";

            public const string Birth = "BIRT";
            public const string Death = "DEAT";
            public const string Burial = "BURI";

            public const string BaptismLDS = "BAPL";
            public const string ConfirmationLDS = "CONL";
            public const string EndowmentLDS = "ENDL";
            public const string SealParentLDS = "SLGC";

            public const string Marriage = "MARR";

            public const string SealSpouseLDS = "SLGS";

			public const string Date = "DATE";
			public const string Place = "PLAC";

            public const string Authority = "AUTH";
            public const string Title = "TITL";
            public const string Abbreviation = "ABBR";
            public const string Publication = "PUBL";
            public const string Page = "PAGE";

            public const string Address = "ADDR";
            public const string Email = "EMAIL";
            public const string Phone = "PHON";

			public const string File = "FILE";
			public const string Data = "DATA";
			public const string Text = "TEXT";
            public const string Note = "NOTE";

            public const string Concat = "CONC";
            public const string Continue = "CONT";
        }

        bool _gedEOF;

        int _gedLevel;
        string _gedToken;
        string _gedData;


		public string FileName { get; private set; }

		public ObservableCollection<Person> People { get; private set; }
		public ObservableCollection<Family> Families { get; private set; }
		public ObservableCollection<Repository> Repositories { get; private set; }
		public ObservableCollection<Source> Sources { get; private set; }
		public ObservableCollection<Citation> Citations { get; private set; }
		public ObservableCollection<Media> Media { get; private set; }
		public ObservableCollection<GedLine> GedLines { get; private set; }

		public Gedcom(string fileName)
		{
			FileName = fileName;
			ReadGedcom();
		}

		private void ReadGedcom()
		{
			People = new ObservableCollection<Person>();
            Families = new ObservableCollection<Family>();
            Repositories = new ObservableCollection<Repository>();
			Sources = new ObservableCollection<Source>();
			Citations = new ObservableCollection<Citation>();
			Media = new ObservableCollection<Media>();

			using (StreamReader stream = File.OpenText(FileName))
			{
				GetNextLine(stream);
				while (!_gedEOF && !_gedLevel.Equals(-1))
				{
                    switch (_gedToken)
                    {
                        case Tokens.Individual:
                            var person = ReadPerson(stream, _gedLevel);
                            People.Add(person);
                            break;

                        case Tokens.Family:
                            var family = ReadFamily(stream, _gedLevel);
                            Families.Add(family);
                            break;

                        case Tokens.Repository:
                            var repository = ReadRepository(stream, _gedLevel);
                            Repositories.Add(repository);
                            break;

                        case Tokens.Source:
                            var source = ReadSource(stream, _gedLevel);
                            Sources.Add(source);
                            break;

						case Tokens.Media:
                    		var media = ReadMedia(stream, _gedLevel);
							Media.Add(media);
                    		break;

                        default:
                            ReadUnknownToken(stream, _gedLevel);
                            break;
                    }
				}

				stream.Close();
			}
		}

        private Person ReadPerson(StreamReader stream, int gedLevelIn)
        {
            // Create and initialize the new person
            var person = new Person
            {
                ID = _gedData,
                Fullname = string.Empty,
                FamilyName = string.Empty,
                GivenName = string.Empty,
                Sex = "U",
                Facts = new ObservableCollection<Fact>(),
            };

            // Fill in the person data
            GetNextLine(stream);
            while (!_gedEOF && (gedLevelIn < _gedLevel))
            {
                switch (_gedToken)
                {
                    case Tokens.Name:
                        person.Fullname = _gedData;
                		ParsePersonName(person);
						ReadPersonName(stream, _gedLevel, person);
                        break;

                    case Tokens.Sex:
                        person.Sex = _gedData;
                        GetNextLine(stream);
                        break;

                    case Tokens.Birth:
					case Tokens.Death:
					case Tokens.Burial:
					case Tokens.BaptismLDS:
                    case Tokens.ConfirmationLDS:
                    case Tokens.EndowmentLDS:
                    case Tokens.SealParentLDS:
                        var fact = ReadFact(stream, _gedLevel);
                        person.Facts.Add(fact);
                        break;

                    default:
                        ReadUnknownToken(stream, _gedLevel);
                        break;
                }
            }

            return person;
        }

		private void ParsePersonName(Person person)
		{
			if (string.IsNullOrEmpty(person.Fullname)) return;

			var name = person.Fullname;
			if (name.Contains("/"))
			{
				var idx1 = name.IndexOf('/');
				var idx2 = name.LastIndexOf('/');

				if (idx1 < idx2)
				{
					person.GivenName = name.Substring(0, idx1).Trim();
					person.FamilyName = name.Substring(idx1 + 1, idx2 - idx1 - 1).Trim();
					person.Suffix = name.Substring(idx2).Trim();
				}
			}
		}

        private void ReadPersonName(StreamReader stream, int gedLevelIn, Person person)
        {
            GetNextLine(stream);
            while (!_gedEOF && (gedLevelIn < _gedLevel))
            {
                switch (_gedToken)
                {
					case Tokens.Surname:
						if (string.IsNullOrEmpty(person.Fullname))
						{
							person.FamilyName = _gedData;
						}
						GetNextLine(stream);
                		break;

					case Tokens.Given:
						if (string.IsNullOrEmpty(person.Fullname))
						{
							person.GivenName = _gedData;
						}
						GetNextLine(stream);
						break;

                    default:
                        ReadUnknownToken(stream, _gedLevel);
                        break;
                }
            }
        }

        private Family ReadFamily(StreamReader stream, int gedLevelIn)
        {
            // Create and initialize the new family
            var family = new Family
            {
                ID = _gedData,
                FatherID = string.Empty,
                MotherID = string.Empty,
                ChildIDs = new ObservableCollection<string>(),
                Facts = new ObservableCollection<Fact>()
            };

            // Fill in the family data
            GetNextLine(stream);
            while (!_gedEOF && (gedLevelIn < _gedLevel))
            {
                switch (_gedToken)
                {
                    case Tokens.Husband:
						family.FatherID = _gedData;
                        GetNextLine(stream);
                        break;

                    case Tokens.Wife:
						family.MotherID = _gedData;
                        GetNextLine(stream);
                        break;

                    case Tokens.Child:
                        family.ChildIDs.Add(_gedData);
                        GetNextLine(stream);
                        break;

					case Tokens.Marriage:
					case Tokens.SealSpouseLDS:
						var fact = ReadFact(stream, _gedLevel);
                        family.Facts.Add(fact);
                        break;

                    case Tokens.Note:
                        family.Note = ReadText(stream, _gedLevel);
                        break;

                    default:
                        ReadUnknownToken(stream, _gedLevel);
                        break;
                }
            }

            return family;
        }

        private Repository ReadRepository(StreamReader stream, int gedLevelIn)
        {
            // Create and initialize the new repository
            var repository = new Repository
            {
                ID = _gedData,
                Name = string.Empty,
                Address = string.Empty,
                Email = string.Empty,
                Phone = string.Empty,
                Note = string.Empty,
            };

            // Fill in the repository data
            GetNextLine(stream);
            while (!_gedEOF && (gedLevelIn < _gedLevel))
            {
                switch (_gedToken)
                {
                    case Tokens.Name:
                        repository.Name = _gedData;
                        GetNextLine(stream);
                        break;

                    case Tokens.Address:
                        repository.Address = _gedData;
                        GetNextLine(stream);
                        break;

                    case Tokens.Email:
                        repository.Email = _gedData;
                        GetNextLine(stream);
                        break;

                    case Tokens.Phone:
                        repository.Phone = _gedData;
                        GetNextLine(stream);
                        break;

                    case Tokens.Note:
                        repository.Note = ReadText(stream, _gedLevel);
                        break;

                    default:
                        ReadUnknownToken(stream, _gedLevel);
                        break;
                }
            }

            return repository;
        }

        private Source ReadSource(StreamReader stream, int gedLevelIn)
        {
            // Create and initialize the new source
            var source = new Source
            {
                ID = _gedData,
                Authority = string.Empty,
                Title = string.Empty,
                Publication = string.Empty,
                RepositoryID = string.Empty,
            };

            // Fill in the source data
            GetNextLine(stream);
            while (!_gedEOF && (gedLevelIn < _gedLevel))
            {
                switch (_gedToken)
                {
                    case Tokens.Authority:
                        source.Authority = ReadText(stream, _gedLevel);
                        break;

                    case Tokens.Title:
                        source.Title = ReadText(stream, _gedLevel);
                        break;

                    case Tokens.Abbreviation:
                        source.Abbreviation = ReadText(stream, _gedLevel);
                        break;

                    case Tokens.Publication:
                        source.Publication = ReadText(stream, _gedLevel);
                        break;

                    case Tokens.Text:
                        source.Text = ReadText(stream, _gedLevel);
                        break;

                    case Tokens.Repository:
                        source.RepositoryID = _gedData;
                        GetNextLine(stream);
                        break;

                    case Tokens.Note:
                        source.Note = ReadText(stream, _gedLevel);
                        break;

                    default:
                        ReadUnknownToken(stream, _gedLevel);
                        break;
                }
            }

            return source;
        }

        private int _factID = 1;

        private Fact ReadFact(StreamReader stream, int gedLevelIn)
        {
            var fact = new Fact
            {
                ID = string.Format("@E{0}@", _factID++),
                Type = _gedToken,
                Date = null,
                Place = string.Empty,
                Description = _gedData,
				Note = string.Empty,
				SourceID = string.Empty,
			};

            GetNextLine(stream);
            while (!_gedEOF && (gedLevelIn < _gedLevel))
            {
                switch (_gedToken)
                {
                    case Tokens.Date:
                		DateTime dt;
                        fact.Date = DateTime.TryParse(_gedData, out dt) ? dt : (DateTime?)null;
                        GetNextLine(stream);
                        break;

                    case Tokens.Place:
                        fact.Place = _gedData;
                        GetNextLine(stream);
                        break;

					case Tokens.Source:
                		Citation citation = ReadCitation(stream, _gedLevel);
                		Citation citation2 = FindDuplicateCitatoin(citation);

						if (citation2 != null)
						{
							fact.SourceID = citation2.SourceID;
							citation2.FactIDs.Add(fact.ID);
						}
						else
						{
							fact.SourceID = citation.SourceID;
							citation.FactIDs.Add(fact.ID);
							Citations.Add(citation);
						}

						break;

					case Tokens.Note:
						fact.Note = ReadText(stream, _gedLevel);
						break;

                    default:
                        ReadUnknownToken(stream, _gedLevel);
                        break;
                }
            }

            return fact;
        }

		private Citation FindDuplicateCitatoin(Citation citationIn)
		{
			foreach (var citation in Citations)
			{
				if ((citation.SourceID == citationIn.SourceID) &&
					(citation.Page == citationIn.Page) &&
					(citation.Text == citationIn.Text))
				{
					return citation;
				}
			}

			return null;
		}

		private int _citationID = 1;

        private Citation ReadCitation(StreamReader stream, int gedLevelIn)
        {
        	var citation = new Citation
			{
        		ID = string.Format("@C{0}@", _citationID++),
        		SourceID = _gedData,
        		Page = string.Empty,
				Text = string.Empty,
				FactIDs = new ObservableCollection<string>()
            };

            GetNextLine(stream);
            while (!_gedEOF && (gedLevelIn < _gedLevel))
            {
                switch (_gedToken)
                {
                    case Tokens.Page:
                		citation.Page = _gedData;
                        GetNextLine(stream);
                        break;

					case Tokens.Data:
						citation.Text = ReadText(stream, _gedLevel);
						break;

                    default:
                        ReadUnknownToken(stream, _gedLevel);
                        break;
                }
            }

			return citation;
        }

        private string ReadText(StreamReader stream, int gedLevelIn)
        {
            var text = new StringBuilder(_gedData);

            // Fill in the source data
            GetNextLine(stream);
            while (!_gedEOF && (gedLevelIn < _gedLevel))
            {
                switch (_gedToken)
                {
					case Tokens.Text:
                		text.Clear();
						text.Append(_gedData);

						GetNextLine(stream);
						break;

					case Tokens.Concat:
                        if (0 < text.Length)
                            text.Append(" ");
                        text.Append(_gedData);

                        GetNextLine(stream);
                        break;

                    case Tokens.Continue:
                        if (0 < text.Length)
                            text.Append("\r\n");
                        text.Append(_gedData);

                        GetNextLine(stream);
                        break;

                    default:
                        ReadUnknownToken(stream, _gedLevel);
                        break;
                }
            }

            return text.ToString();
        }

		private Media ReadMedia(StreamReader stream, int gedLevelIn)
		{
			var media = new Media
			{
				ID = _gedData,
				FileName = string.Empty,
				Date = string.Empty,
			};

			GetNextLine(stream);
			while (!_gedEOF && (gedLevelIn < _gedLevel))
			{
				switch (_gedToken)
				{
					case Tokens.File:
						media.FileName = _gedData;
						GetNextLine(stream);
						break;

					case Tokens.Date:
						media.Date = ReadText(stream, _gedLevel);
						break;

					default:
						ReadUnknownToken(stream, _gedLevel);
						break;
				}
			}

			return media;
		}

        private void ReadUnknownToken(StreamReader stream, int gedLevelIn)
        {
            GetNextLine(stream);
            while (!_gedEOF && (gedLevelIn < _gedLevel))
            {
                GetNextLine(stream);
            }
        }

		public void ReadGedLines()
		{
			GedLines = new ObservableCollection<GedLine>();

			using (var stream = File.OpenText(FileName))
			{
				GetNextLine(stream);

				while (!_gedEOF)
				{
					var gedLine = AddNewGedLineItem();
					GedLines.Add(gedLine);

					ReadGedLineTokens(stream, 0, gedLine);
				}

				stream.Close();
			}
		}

		private GedLine AddNewGedLineItem()
		{
			var gedLine = new GedLine
			{
				Level = _gedLevel,
				Token = _gedToken,
				Data = _gedData,
				GedLines = new ObservableCollection<GedLine>()
			};

			return gedLine;
		}

		private void ReadGedLineTokens(StreamReader stream, int levelIn, GedLine gedItemIn)
		{
			GetNextLine(stream);
			while (!_gedEOF && (levelIn < _gedLevel))
			{
				var gedItem = AddNewGedLineItem();
				gedItemIn.GedLines.Add(gedItem);

				ReadGedLineTokens(stream, _gedLevel, gedItem);
			}
		}
        private void GetNextLine(StreamReader stream)
		{
			_gedLevel = -1;
			_gedToken = string.Empty;
			_gedData = string.Empty;
			
			string line = stream.ReadLine();

			if (!string.IsNullOrEmpty(line))
			{
				string [] lineParts = line.Trim().Split(new [] { ' ' }, 3);

                _gedLevel = (0 < lineParts.Length) ? int.Parse(lineParts[0]) : -1;
                _gedToken = (1 < lineParts.Length) ? lineParts[1] : string.Empty;
                _gedData = (2 < lineParts.Length) ? lineParts[2] : string.Empty;

                if (_gedToken.StartsWith("@"))
                {
                    var temp = _gedToken;
                    _gedToken = _gedData;
                    _gedData = temp;
                }

				_gedEOF = false;
			}
			else
			{
				_gedEOF = true;
			}
		}
	}
}
