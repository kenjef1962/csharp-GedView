using GedcomReader;
using GedViewWPF.Model;
using GedViewWPF.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using System.Timers;

namespace GedViewWPF.DataAccess
{
	public partial class DataManager
	{
	    private Gedcom _gedcom;

		internal bool IsGedcomOpen
		{
            get { return (_gedcom != null); }
		}

        internal string GedcomFilename
        {
            get { return IsGedcomOpen ? Path.GetFileName(_gedcom.Filename) : string.Empty; }
        }

		public void OpenGedcom(string fileName)
		{
			CloseGedcom();

			_gedcom = new Gedcom(fileName);
		}

		public void CloseGedcom()
		{
            _gedcom = null;
            _personList = null;
            _factList = null;
            _familyList = null;
            _sourceList = null;
        }

		#region Statistics
		public int PersonCount { get { return IsGedcomOpen ? _gedcom.Individuals.Count : 0; } }
		public int FamilyCount { get { return IsGedcomOpen ? _gedcom.Families.Count : 0; } }
		public int FactCount { get { return IsGedcomOpen ? _gedcom.Facts.Count : 0; } }
		public int RepositoryCount { get { return IsGedcomOpen ? _gedcom.Repositories.Count : 0; } }
		public int SourceCount { get { return IsGedcomOpen ? _gedcom.Sources.Count : 0; } }
		public int MediaCount { get { return IsGedcomOpen ? _gedcom.Media.Count : 0; } }
		public int NoteCount { get { return IsGedcomOpen ? _gedcom.Notes.Count : 0; } }

		public long FileSize
		{
			get
			{
				var fi = new FileInfo(_gedcom.Filename);
				return IsGedcomOpen ? fi.Length : 0;
			}
		}
		public string FileLastOpened
		{
			get
			{
				var fi = new FileInfo(_gedcom.Filename);
				return IsGedcomOpen ? fi.LastWriteTime.ToString() : null;
			}
		}

		public int SurnameCount
		{
			get
			{
				var surnames = new List<string>();
				if (IsGedcomOpen)
				{
					foreach (var individual in _gedcom.Individuals.Values)
					{
						var name = individual.Names[0];
						if (!surnames.Contains(name.Surname.ToLower()))
						{
							surnames.Add(name.Surname.ToLower());
						}
					}
				}
				return surnames.Count;
			}
		}
		public int MaleCount
		{
			get
			{
				var count = 0;
				if (IsGedcomOpen)
				{
					foreach (var individual in _gedcom.Individuals.Values)
					{
						if (!string.IsNullOrEmpty(individual.Sex) && 
							(individual.Sex.StartsWith("M") || individual.Sex.StartsWith("m")))
						{
							count++;
						}
					}
				}
				return count;
			}
		}
		public int FemaleCount
		{
			get
			{
				var count = 0;
				if (IsGedcomOpen)
				{
					foreach (var individual in _gedcom.Individuals.Values)
					{
						if (!string.IsNullOrEmpty(individual.Sex) && 
							(individual.Sex.StartsWith("F") || individual.Sex.StartsWith("f")))
						{
							count++;
						}
					}
				} 
				return count;
			}
		}
		#endregion

		#region Person Access Properties / Methods
		private List<Person> _personList;
        internal List<Person> PersonList
        {
            get
            {
                if (_personList != null)
                    return _personList;

                _personList = new List<Person>();

                foreach (var gedcomPerson in _gedcom.Individuals.Values)
                {
                    var person = Mapper.GedcomPersonToPerson(_gedcom, gedcomPerson);

                    // BMD

                    _personList.Add(person);
                }
                
                // Sort Person List
                _personList.Sort((p1, p2) => p1.CompareTo(p2));

                return _personList;
            }
        }

		internal Person RootPerson
		{
			get
			{
				foreach (var gedcomPerson in _gedcom.Individuals.Values)
				{
					var person = Mapper.GedcomPersonToPerson(_gedcom, gedcomPerson);
					return person;
				}

				return null;
			}
		}

        internal Person GetPersonByID(string id)
        {
            return PersonList.FirstOrDefault(p => p.ID == id);
        }
        #endregion

        #region Fact Access Properties / Methods
        private List<Fact> _factList;
        internal List<Fact> FactList
        {
            get
            {
                if (_factList != null)
                    return _factList;

                _factList = new List<Fact>();

                foreach (var gedcomPerson in _gedcom.Individuals.Values)
                {
                    foreach (var gedcomFact in gedcomPerson.Facts)
                    {
                        var fact = Mapper.GedcomFactToFact(_gedcom, gedcomFact);
                        fact.PersonIDs.Add(gedcomPerson.ID);

                        _factList.Add(fact);
                    }
                }

                foreach (var gedcomFamily in _gedcom.Families.Values)
                {
                    foreach (var gedcomFact in gedcomFamily.Facts)
                    {
                        var fact = Mapper.GedcomFactToFact(_gedcom, gedcomFact);
                        fact.FamilyIDs.Add(gedcomFamily.ID);

                        _factList.Add(fact);
                    }
                }

                return _factList;
            }
        }

        internal Fact GetFactByID(string id)
        {
            return FactList.FirstOrDefault(f => f.ID == id);
        }

        internal List<Fact> GetPersonFacts(string id)
        {
            var person = GetPersonByID(id);
            if ((person != null) && (person.Facts != null))
                return person.Facts;

            person.Facts = new List<Fact>();

            foreach (var fact in FactList)
            {
                if (fact.PersonIDs.Contains(person.ID))
                {
                    person.Facts.Add(fact);
                }
            }

            // Sort Fact List
            person.Facts.Sort((f1, f2) => f1.CompareTo(f2));

            foreach (var fact in person.Facts)
            {
                fact.Age = Utils.CalculateAge(fact, person.Birth, person.Death);
            }

            return person.Facts;
        }

        internal List<Citation> GetFactCitations(string factID)
        {
            var citations = new List<Citation>();

            var gedcomFact = _gedcom.Facts[factID];
            if (gedcomFact != null)
            {
                foreach (var gedcomCitation in gedcomFact.Citations)
                {
                    var citation = Mapper.GedcomCitationToCitation(_gedcom, gedcomCitation);
                    citations.Add(citation);
                }

                citations.Sort((c1, c2) => c1.CompareTo(c2));

            }

            return citations;
        }

        internal List<Media> GetFactMedia(string factID)
        {
            var mediaList = new List<Media>();

            var gedcomFact = _gedcom.Facts[factID];
            if (gedcomFact != null)
            {
                foreach (var gedcomMedia in gedcomFact.Media)
                {
                    var gedcomMedia2 = gedcomMedia;
                    if (!string.IsNullOrEmpty(gedcomMedia2.ID))
                    {
                        gedcomMedia2 = _gedcom.Media[gedcomMedia2.ID];
                    }

                    var media = Mapper.GedcomMediaToMedia(_gedcom, gedcomMedia2);
                    mediaList.Add(media);
                }

                mediaList.Sort((m1, m2) => m1.CompareTo(m2));
            }

            return mediaList;
        }

        internal List<string> GetFactNotes(string factID)
        {
            var notes = new List<string>();

            var gedcomFact = _gedcom.Facts[factID];
            if (gedcomFact != null)
            {
                foreach (var gedcomNote in gedcomFact.Notes)
                {
                    var gedcomNote2 = gedcomNote;
                    if (!string.IsNullOrEmpty(gedcomNote2.ID))
                    {
                        gedcomNote2 = _gedcom.Notes[gedcomNote2.ID];
                    }

                    notes.Add(gedcomNote2.Text);
                }
            }

            return notes;
        }
        #endregion

        #region Family Access Properties / Methods
        private List<Family> _familyList;
        internal List<Family> FamilyList
        {
            get
            {
                if (_familyList != null)
                    return _familyList;

                _familyList = new List<Family>();

                foreach (var gedcomFamily in _gedcom.Families.Values)
                {
                    var family = Mapper.GedcomFamilyToFamily(_gedcom, gedcomFamily);
                    _familyList.Add(family);
                }

                return _familyList;
            }
        }

        internal Family GetFamilyByID(string id)
        {
            return FamilyList.FirstOrDefault(f => f.ID == id);
        }

        internal Family GetFamilyByChild(string personID)
        {
            foreach (var family in FamilyList)
            {
                var child = family.Children.FirstOrDefault(c => c.ID == personID);

                if (child != null)
                    return family;
            }

            return null;
        }

        internal Family GetFamilyByParent(string personID)
        {
            foreach (var family in FamilyList)
            {
                if (((family.Husband != null) && (family.Husband.ID == personID)) || 
                    ((family.Wife != null) && (family.Wife.ID == personID)))
                {
                    return family;
                }
            }

            return null;
        }
        #endregion

        #region Source Access Properties / Methods
        private List<Source> _sourceList;
        internal List<Source> SourceList
        {
            get
            {
                if (_sourceList != null)
                    return _sourceList;

                _sourceList = new List<Source>();

                foreach (var gedcomSource in _gedcom.Sources.Values)
                {
                    var Source = Mapper.GedcomSourceToSource(_gedcom, gedcomSource);
                    _sourceList.Add(Source);
                }

                // Sort Source List
                _sourceList.Sort((p1, p2) => p1.CompareTo(p2));

                return _sourceList;
            }
        }

        internal Source GetSourceByID(string id)
        {
            return SourceList.FirstOrDefault(s => s.ID == id);
        }
        #endregion
	}
}
