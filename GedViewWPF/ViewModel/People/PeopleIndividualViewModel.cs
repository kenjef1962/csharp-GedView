using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GedViewWPF.DataAccess;
using GedViewWPF.Messaging;
using GedViewWPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace GedViewWPF.ViewModel
{
	class PeopleIndividualViewModel : ViewModelBase
	{
        // Public Properties
        private Person _selectedPerson;
        public Person SelectedPerson
        {
            get { return _selectedPerson; }
            set
            {
                _selectedPerson = value;
                OnPropertyChanged("SelectedPerson");

                UpdatePersonDetails();
            }
        }

        private List<Fact> _selectedPersonFacts;
        public List<Fact> SelectedPersonFacts
        {
            get { return _selectedPersonFacts; }
            set
            {
                _selectedPersonFacts = value;
                OnPropertyChanged("SelectedPersonFacts");
            }
        }

        private Person _selectedPersonFather;
        public Person SelectedPersonFather
        {
            get { return _selectedPersonFather; }
            set
            {
                _selectedPersonFather = value;
                OnPropertyChanged("SelectedPersonFather");
            }
        }

        private Person _selectedPersonMother;
        public Person SelectedPersonMother
        {
            get { return _selectedPersonMother; }
            set
            {
                _selectedPersonMother = value;
                OnPropertyChanged("SelectedPersonMother");
            }
        }

        private Fact _selectedFact;
        public Fact SelectedFact
        {
            get { return _selectedFact; }
            set
            {
                _selectedFact = value;
                OnPropertyChanged("SelectedFact");

                UpdateFactDetails(SelectedFact);
            }
        }

        private List<Citation> _selectedFactCitations;
        public List<Citation> SelectedFactCitations
        {
            get { return _selectedFactCitations; }
            set
            {
                _selectedFactCitations = value;
                OnPropertyChanged("SelectedFactCitations");
            }
        }

        private List<Media> _selectedFactMedia;
        public List<Media> SelectedFactMedia
        {
            get { return _selectedFactMedia; }
            set
            {
                _selectedFactMedia = value;
                OnPropertyChanged("SelectedFactMedia");
            }
        }

        private List<string> _selectedFactNotes;
        public List<string> SelectedFactNotes
        {
            get { return _selectedFactNotes; }
            set
            {
                _selectedFactNotes = value;
                OnPropertyChanged("SelectedFactNotes");
            }
        }

        private List<Clue> _selectedPersonClues;
        public List<Clue> SelectedPersonClues
        {
            get { return _selectedPersonClues; }
            set
            {
                _selectedPersonClues = value;
                OnPropertyChanged("SelectedPersonClues");
            }
        }

        // Private Properties
        private readonly DataManager _dataMgr;

        // Commands
        private ICommand _searchCommand;
        public ICommand SearchCommand
        {
            get { return _searchCommand ?? (_searchCommand = new RelayCommand<string>(DoSearch, CanSearch)); }
        }

        public PeopleIndividualViewModel(DataManager dataMgr, Person person)
		{
            _dataMgr = dataMgr;
			_selectedPerson = person;

			UpdatePersonDetails();

            Messenger.Default.Register<MessageArgs>(this, "SelectedPersonChanged", OnSelectedPersonChanged);
        }

        private void OnSelectedPersonChanged(MessageArgs args)
        {
			if (args.Sender == this) return;

            SelectedPerson = args.Data as Person;
        }

        private void UpdatePersonDetails()
        {
            if (SelectedPerson == null)
            {
                SelectedPersonFather = null;
                SelectedPersonMother = null;
                SelectedPersonFacts = null;
                SelectedFact = null;
                SelectedPersonClues = null;

                return;
            }

            var family = _dataMgr.GetFamilyByChild(SelectedPerson.ID);
            SelectedPersonFather = (family != null) ? family.Husband : null;
            SelectedPersonMother = (family != null) ? family.Wife : null;

            var personFacts = _dataMgr.GetPersonFacts(SelectedPerson.ID);
            SelectedPersonFacts = new List<Fact>(personFacts);
            SelectedFact = (0 < SelectedPersonFacts.Count) ? SelectedPersonFacts[0] : null;

            GetPersonClues();
        }

        private void GetPersonClues()
        {
            int? birthYear = null;
            if ((SelectedPerson.Birth != null) && (SelectedPerson.Birth.Date != null))
            {
                birthYear = SelectedPerson.Birth.Date.Year;
            }

            int? deathYear = null;
            if ((SelectedPerson.Death != null) && (SelectedPerson.Death.Date != null))
            {
                deathYear = SelectedPerson.Death.Date.Year;
            }

            int? age = null;
            if ((birthYear != null) && (deathYear != null))
            {
                age = (int)deathYear - (int)birthYear;
            }

            var list = new List<Clue>();

            // SSDI
            if ((deathYear != null) && (1951 <= deathYear))
            {
                var clue = new Clue();
                clue.Category = "BMD";
                clue.Age = string.Format("age: {0}", (age != null) ? age.ToString() : "??");
                clue.Title = "Social Security Death Index";
                clue.Description = "Search the BMD collection at Ancestry";
                clue.SearchString = "Search BMD";
                clue.Url = GetUrlSSDI(SelectedPerson);

                list.Add(clue);
            }

            // Grave
            if (deathYear != null)
            {
                var clue = new Clue();
                clue.Category = "BMD";
                clue.Age = string.Format("age: {0}", (age != null) ? age.ToString() : "??");
                clue.Title = "Find A Grave";
                clue.Description = "Search for records a Find A Grave";
                clue.SearchString = "Search Find A Grave";
                clue.Url = GetUrlGrave(SelectedPerson);

                list.Add(clue);
            }

            // US Federal Census
            for (int censusYear = 1850; censusYear <= 1940; censusYear += 10)
            {
                var age2 = -1;

                if (birthYear != null)
                {
                    age2 = censusYear - (int)birthYear;
                }
                else if (deathYear != null)
                {
                    age2 = censusYear - (int)deathYear - 75;
                }

                if ((0 < age2) && (age2 < 100))   // 1- 99
                {
                    if ((deathYear == null) || (censusYear <= deathYear))
                    {
                        var clue = new Clue();
                        clue.Category = "Census";
                        clue.Age = string.Format("age: {0}", age2);
                        clue.Title = string.Format("{0} US Federal Census", censusYear);
                        clue.Description = "Search the census collection at Ancestry";
                        clue.SearchString = "Search Census Collection";
                        clue.Url = GetUrlCensus(SelectedPerson, censusYear);

                        list.Add(clue);
                    }
                }
            }
            // Military
            if (SelectedPerson.Sex == "M")
            {
                var values = new List<Tuple<int, string>>();
                values.Add(new Tuple<int, string>(1865, "CivilWar"));
                values.Add(new Tuple<int, string>(1918, "World War I"));
                values.Add(new Tuple<int, string>(1945, "World War II"));
                values.Add(new Tuple<int, string>(1953, "Korean Conflict"));
                values.Add(new Tuple<int, string>(1975, "Vietnam War"));

                foreach (var value in values)
                {
                    var age2 = -1;

                    if (birthYear != null)
                    {
                        age2 = value.Item1 - (int)birthYear;
                    }

                    if ((18 <= age2) && (age2 < 50))   // 18 - 50
                    {
                        if ((deathYear == null) || (value.Item1 <= deathYear))
                        {
                            var clue = new Clue();
                            clue.Category = "Military";
                            clue.Age = string.Format("age: {0}", age2);
                            clue.Title = value.Item2;
                            clue.Description = "Search the military collection at Ancestry";
                            clue.Url = GetUrlMilitary(SelectedPerson);
                            clue.SearchString = "Search Millitary Collection";

                            list.Add(clue);
                        }
                    }
                }
            }

            SelectedPersonClues = new List<Clue>(list);
        }

        private void UpdateFactDetails(Fact fact)
        {
            if (SelectedFact == null)
            {
                SelectedFactCitations = null;
                SelectedFactMedia = null;
                SelectedFactNotes = null;

                return;
            }

            var factCitations = _dataMgr.GetFactCitations(SelectedFact.ID);
            SelectedFactCitations = new List<Citation>(factCitations);

            var factMedia = _dataMgr.GetFactMedia(SelectedFact.ID);
            SelectedFactMedia = new List<Media>(factMedia);

            var factNotes = _dataMgr.GetFactNotes(SelectedFact.ID);
            SelectedFactNotes = new List<string>(factNotes);
        }

        private static void GetSearchStrings(Person person, out string birthYear, out string birthPlace, out string marriageYear, out string marriagePlace, out string deathYear, out string deathPlace)
        {
            birthYear = ((person.Birth != null) && (person.Birth.Date != null) && (person.Birth.Date.Year != null)) ? person.Birth.Date.Year.ToString() : string.Empty;
            birthPlace = ((person.Birth != null) && (person.Birth.Place != null)) ? person.Birth.Place.ToString() : string.Empty;
            marriageYear = ((person.Marriage != null) && (person.Marriage.Date != null) && (person.Marriage.Date.Year != null)) ? person.Marriage.Date.Year.ToString() : string.Empty;
            marriagePlace = ((person.Marriage != null) && (person.Marriage.Place != null)) ? person.Marriage.Place.ToString() : string.Empty;
            deathYear = ((person.Death != null) && (person.Death.Date != null) && (person.Death.Date.Year != null)) ? person.Death.Date.Year.ToString() : string.Empty;
            deathPlace = ((person.Death != null) && (person.Death.Place != null)) ? person.Death.Place.ToString() : string.Empty;

            // Eliminate commas
            birthPlace = birthPlace.Replace(",", " ");
            marriagePlace = marriagePlace.Replace(",", " ");
            deathPlace = deathPlace.Replace(",", " ");

            // Eliminate double spaces
            birthPlace = birthPlace.Replace("  ", " ");
            marriagePlace = marriagePlace.Replace("  ", " ");
            deathPlace = deathPlace.Replace("  ", " ");
        }

        private string GetUrlSSDI(Person person)
        {
            string birthYear, birthPlace;
            string marriageYear, marriagePlace;
            string deathYear, deathPlace;
            GetSearchStrings(person, out birthYear, out birthPlace, out marriageYear, out marriagePlace, out deathYear, out deathPlace);

            var url = "http://search.ancestry.com/cgi-bin/sse.dll?";
            url += "gl=34&rank=1&new=1&so=3&MSAV=0&msT=1&gss=ms_f-34";
            url += "&gsfn=" + person.Given.Replace(" ", "+");
            url += "&gsln=" + person.Surname;
            url += "&msbdy=" + birthYear;
            url += "&msbpn__ftp=" + birthPlace;
            url += "&msddy=" + deathYear;
            url += "&msdpn__ftp=" + deathPlace;
            url += "&uidh=ilz&=b&=d&=r&=g&=y&=0";

            return url;
        }

        private string GetUrlGrave(Person person)
        {
            var givenNames = person.Given.Split(new[] { ' ' });
            var givenName = string.Empty;
            var maiden = (person.Sex == "F") ? "1" : "0";
            var iState = "0";

            if (1 <= givenNames.Length)
            {
                givenName = givenNames[0];
            }

            string birthYear, birthPlace;
            string marriageYear, marriagePlace;
            string deathYear, deathPlace;
            GetSearchStrings(person, out birthYear, out birthPlace, out marriageYear, out marriagePlace, out deathYear, out deathPlace);

            var url = "http://www.findagrave.com/cgi-bin/fg.cgi?";
            url += "page=gsr";
            url += "&GSfn=" + givenName;
            url += "&GSmn=";
            url += "&GSln=" + person.Surname;
            url += "&GSiman=" + maiden;
            url += "&GSbyrel=in";
            url += "&GSby=";
            url += "&GSdyrel=in";
            url += "&GSdy=" + deathYear;
            url += "&GScntry=4";
            url += "&GSst=" + iState;
            url += "&GSgrid=";
            url += "&df=all";
            url += "&GSob=n";

            return url;
        }

        private string GetUrlCensus(Person person, int censusYear)
        {
            string birthYear, birthPlace;
            string marriageYear, marriagePlace;
            string deathYear, deathPlace;
            GetSearchStrings(person, out birthYear, out birthPlace, out marriageYear, out marriagePlace, out deathYear, out deathPlace);

            var url = "http://search.ancestry.com/cgi-bin/sse.dll?";
            url += "gl=CEN_" + censusYear.ToString();
            url += "&rank=1&new=1&MSAV=0&msT=1";
            url += "&gsfn=" + person.Given.Replace(" ", "+");
            url += "&gsln=" + person.Surname;
            url += "&msbdy=" + birthYear;
            url += "&msbpn__ftp=" + birthPlace;
            url += "&msddy=" + deathYear;
            url += "&msdpn__ftp=" + deathPlace;
            url += "&uidh=ilz&=b%2cd%2cr%2cg%2cy%2c0&so=2";

            return url;
        }

        private string GetUrlMilitary(Person person)
        {
            string birthYear, birthPlace;
            string marriageYear, marriagePlace;
            string deathYear, deathPlace;
            GetSearchStrings(person, out birthYear, out birthPlace, out marriageYear, out marriagePlace, out deathYear, out deathPlace);

            var url = "http://search.ancestry.com/cgi-bin/sse.dll?";
            url += "gl=39&rank=1&new=1&MSAV=0&msT=1&gss=angs-g";
            url += "&gsfn=" + person.Given.Replace(" ", "+");
            url += "&gsln=" + person.Surname;
            url += "&msbdy=" + birthYear;
            url += "&msbpn__ftp=" + birthPlace;
            url += "&msddy=" + deathYear;
            url += "&msdpn__ftp=" + deathPlace;
            url += "&uidh=ilz&=b%2cd%2cr%2cg%2cy%2c0&so=2";

            return url;
        }

        private string GetUrlAncestry(Person person)
        {
            string birthYear, birthPlace;
            string marriageYear, marriagePlace;
            string deathYear, deathPlace;
            GetSearchStrings(person, out birthYear, out birthPlace, out marriageYear, out marriagePlace, out deathYear, out deathPlace);

            var url = "http://search.ancestry.com/cgi-bin/sse.dll?";
            url += "gl=ROOT_CATEGORY";
            url += "&rank=1";
            url += "&new=1";
            url += "&so=3";
            url += "&MSAV=0";
            url += "&msT=1";
            url += "&gss=ms_f-2_s";
            url += "&gsfn=" + person.Given.Replace(" ", "+");
            url += "&gsln=" + person.Surname.Replace(" ", "+");
            url += "&msbdy=" + birthYear;
            url += "&msbpn__ftp=" + birthPlace;
            url += "&msgdy=" + marriageYear;
            url += "&msgpn__ftp=" + marriagePlace;
            url += "&msddy=" + deathYear;
            url += "&msdpn__ftp=" + deathPlace;
            url += "&uidh=ilz";

            return url;
        }

        private string GetUrlArchives(Person person)
        {
            string birthYear, birthPlace;
            string marriageYear, marriagePlace;
            string deathYear, deathPlace;
            GetSearchStrings(person, out birthYear, out birthPlace, out marriageYear, out marriagePlace, out deathYear, out deathPlace);

            var url = "http://www.archives.com/GA.aspx?";
            url += "FirstName=" + person.Given.Replace(" ", "+");
            url += "&LastName=" + person.Surname.Replace(" ", "+");
            url += "&Location=US";
            url += "&BirthYear=" + birthYear;
            url += "&BirthYearSpan=";
            url += "&DeathYear=" + deathYear;
            url += "&DeathYearSpan=";
            url += "&_act=registerAS_org";

            return url;
        }

        private string GetUrlFamilySearch(Person person)
        {
            string birthYear, birthPlace;
            string marriageYear, marriagePlace;
            string deathYear, deathPlace;
            GetSearchStrings(person, out birthYear, out birthPlace, out marriageYear, out marriagePlace, out deathYear, out deathPlace);

            var url = "http://www.familysearch.org/search/record/results#count=20&query=";
            url += "+givenname:\"" + person.Given + "\"~";
            url += "+surname:\"" + person.Surname + "\"~";

            if (!string.IsNullOrEmpty(birthPlace))
                url += "+birth_place:\"" + birthPlace + "\"~";
            
            if (!string.IsNullOrEmpty(birthYear))
                url += "+birth_year:" + birthYear + "-" + birthYear + "~";

            if (!string.IsNullOrEmpty(marriagePlace))
                url += "+marriage_place:\"" + marriagePlace + "\"~";

            if (!string.IsNullOrEmpty(marriageYear))
                url += "+marriage_year:" + marriageYear + "-" + marriageYear + "~";

            if (!string.IsNullOrEmpty(deathPlace))
                url += "+death_place:\"" + deathPlace + "\"~";

            if (!string.IsNullOrEmpty(deathYear))
                url += "+death_year:" + deathYear + "-" + deathYear + "~";


            return url;
        }

        private string GetUrlFold3(Person person)
        {
            string birthYear, birthPlace;
            string marriageYear, marriagePlace;
            string deathYear, deathPlace;
            GetSearchStrings(person, out birthYear, out birthPlace, out marriageYear, out marriagePlace, out deathYear, out deathPlace);

            var url = "http://go.fold3.com/query.php?";
            url += "query=" + person.ToString().Replace(" ", "+");

            return url;
        }

        private string GetUrlNewsPapers(Person person)
        {
            string birthYear, birthPlace;
            string marriageYear, marriagePlace;
            string deathYear, deathPlace;
            GetSearchStrings(person, out birthYear, out birthPlace, out marriageYear, out marriagePlace, out deathYear, out deathPlace);

            var url = "http://www.newspapers.com/search/#query=";
            url += person.ToString().Replace(" ", "+");

            return url;
        }

        #region Command Handlers
        public bool CanSearch(string site)
        {
            return (SelectedPerson != null);
        }
        public void DoSearch(string site)
        {
            if (!CanSearch(site)) return;

            var url = string.Empty;

            switch (site)
            {
                case "Ancestry":
                    url = GetUrlAncestry(SelectedPerson);
                    break;

                case "Archives":
                    url = GetUrlArchives(SelectedPerson);
                    break;

                case "FamilySearch":
                    url = GetUrlFamilySearch(SelectedPerson);
                    break;

                case "Fold3":
                    url = GetUrlFold3(SelectedPerson);
                    break;

                case "NewsPapers":
                    url = GetUrlNewsPapers(SelectedPerson);
                    break;
            }

            if (!string.IsNullOrEmpty(url))
            {
                Process.Start(url);
            }
        }
        #endregion
    }
}