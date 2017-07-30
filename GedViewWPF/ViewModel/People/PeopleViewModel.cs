using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GedViewWPF.DataAccess;
using GedViewWPF.Messaging;
using GedViewWPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace GedViewWPF.ViewModel
{
	class PeopleViewModel : ViewModelBase
	{
        // Public Commands
        private ICommand _viewNavigateCommand;
        public ICommand ViewNavigateCommand
        {
            get { return _viewNavigateCommand ?? (_viewNavigateCommand = new RelayCommand<string>(DoViewNavigate, CanDoViewNavigate)); }
        }

        // Public Properties
        private List<object> _personList;
		public List<object> PersonList
        {
            get { return _personList; }
            set
            {
                _personList = value;
                OnPropertyChanged("PersonList");
            }
        }

        private object _selectedPerson;
        public object SelectedPerson
        {
            get { return _selectedPerson; }
            set
            {
                if (value is GroupHeader)
                    _selectedPerson = (value as GroupHeader).HeaderPerson;
                else
                    _selectedPerson = value as Person;

				OnPropertyChanged("SelectedPerson");

                Messenger.Default.Send(new MessageArgs(this, _selectedPerson), "SelectedPersonChanged");
            }
        }

		private int _groupByIndex;
		public int GroupByIndex
		{
			get { return _groupByIndex; }
			set
			{
				_groupByIndex = value;
				OnPropertyChanged("GroupByIndex");

				InitializePeopleList();
			}
		}

		private string _filterByText;
		public string FilterByText
		{
			get { return _filterByText; }
			set
			{
				_filterByText = value;
				OnPropertyChanged("FilterByText");

				InitializePeopleList();
			}
		}

        private object _currentVM;
        public object CurrentVM
        {
            get { return _currentVM; }
            set
            {
                if (_currentVM == value) return;

                _currentVM = value;
                base.OnPropertyChanged("CurrentVM");
            }
        }

		private bool _isPeopleListVisible = true;
		public bool IsPeopleListVisible
        {
			get { return _isPeopleListVisible; }
            set
            {
				if (_isPeopleListVisible == value) return;

				_isPeopleListVisible = value;
				base.OnPropertyChanged("IsPeopleListVisible");
            }
        }

        // Private Properties
        private readonly DataManager _dataMgr;

        private PeoplePedigreeViewModel _pedigreeVM;
        private PeopleFamilyViewModel _familyVM;
        private PeopleIndividualViewModel _individualVM;

		public PeopleViewModel(DataManager dataMgr)
		{
            _dataMgr = dataMgr;

            InitializePeopleList();
            ResetViews();

		    Messenger.Default.Register<MessageArgs>(this, "SelectedPersonChanged", OnSelectedPersonChanged);
		}

        private void OnSelectedPersonChanged(MessageArgs args)
        {
			if (args.Sender == this) return;

            SelectedPerson = args.Data as Person;
		}

        private void InitializePeopleList()
        {
			var sortedList = new List<object>(_dataMgr.PersonList);

			if (!string.IsNullOrEmpty(_filterByText))
			{
				FilterPersonList(sortedList);
			}

            if (GroupByIndex == 1)
            {
                GroupPersonListBySurname(sortedList);
            }
            else if (GroupByIndex == 2)
            {
                GroupPersonListByBirthYear(sortedList);
            }

			PersonList = new List<object>(sortedList);

			if (SelectedPerson == null)
			{
				SelectedPerson = _dataMgr.RootPerson;
			}
        }

		private void FilterPersonList(List<object> sortedList)
		{
			for (int i = sortedList.Count - 1; i >= 0; i--)
			{
				var personName = sortedList[i].ToString();

				if (!personName.ToLower().Contains(_filterByText.ToLower()))
				{
					sortedList.RemoveAt(i);
				}
			}
		}

		private void GroupPersonListBySurname(List<object> sortedList)
        {
            var lastSurname = string.Empty;
            for (var idx = 0; idx < sortedList.Count; idx++)
            {
                if (sortedList[idx] is Person)
                {
                    var person = sortedList[idx] as Person;
                    if (person.Surname != lastSurname)
                    {
                        var groupHeader = new GroupHeader();
                        groupHeader.Header = person.Surname;
                        groupHeader.HeaderPerson = person;
                        lastSurname = person.Surname;

                        sortedList.Insert(idx, groupHeader);
                    }
                }
            }
        }

		private void GroupPersonListByBirthYear(List<object> sortedList)
        {
            sortedList.Sort((item1, item2) =>
            {
                var person1 = item1 as Person;
                var person2 = item2 as Person;

                var date1 = (person1 != null) ? person1.Birth : null;
                var date2 = (person2 != null) ? person2.Birth : null;

                if ((date1 != null) && (date2 != null))
                {
                    if (((date1.Date != null) && (date1.Date.DateDT != null)) &&
                        ((date2.Date != null) && (date2.Date.DateDT != null)))
                    {
                        var dt1 = (DateTime)(date1.Date.DateDT);
                        var dt2 = (DateTime)(date2.Date.DateDT);
                        var result = dt1.CompareTo(dt2);

                        if (result != 0)
                            return result;
                    }
                    else if (((date1.Date != null) && (date1.Date.DateDT != null)) &&
                             ((date2.Date == null) || (date2.Date.DateDT == null)))
                    {
                        return -1;
                    }
                    else if (((date1.Date == null) || (date1.Date.DateDT == null)) &&
                             ((date2.Date != null) && (date2.Date.DateDT != null)))
                    {
                        return 1;
                    }
                }
                else if ((date1 != null) && (date2 == null))
                    return -1;
                else if ((date1 == null) && (date2 != null))
                    return 1;

                if ((person1 != null) && (person2 != null))
                    return person1.CompareTo(person2);
                else if ((person1 != null) && (person2 == null))
                    return -1;
                else if ((person1 == null) && (person2 != null))
                    return 1;

                return 0;
            });

            var lastYear = -1;
            for (var idx = 0; idx < sortedList.Count; idx++)
            {
                if (sortedList[idx] is Person)
                {
                    var person = sortedList[idx] as Person;
                    if ((person.Birth != null) && (person.Birth.Date != null) && (person.Birth.Date.DateDT != null))
                    {
                        var dt = (DateTime)(person.Birth.Date.DateDT);
                        var year = (dt.Year / 10) * 10;

                        if (lastYear != year)
                        {
                            var groupHeader = new GroupHeader();
                            groupHeader.Header = string.Format("{0} - {1}", year, year + 9);
                            groupHeader.HeaderPerson = person;
                            lastYear = year;

                            sortedList.Insert(idx, groupHeader);
                        }
                    }
                    else if (lastYear < 9999)
                    {
                        var groupHeader = new GroupHeader();
                        groupHeader.Header = "[unknown]";
                        groupHeader.HeaderPerson = person;
                        lastYear = 9999;

                        sortedList.Insert(idx, groupHeader);
                    }
                }
            }
        }

        private void ResetViews()
        {
            _currentVM = null;
            _pedigreeVM = null;
            _familyVM = null;
            _individualVM = null;

            DoViewNavigate("Pedigree");
        }

        #region Command Handlers
        public bool CanDoViewNavigate(string view)
		{
            return true;
		}
		public void DoViewNavigate(string view)
		{
			if (!CanDoViewNavigate(view)) return;

			switch (view)
			{
                case "Pedigree":
                    if (_pedigreeVM == null)
                        _pedigreeVM = new PeoplePedigreeViewModel(_dataMgr, SelectedPerson as Person);

                    CurrentVM = _pedigreeVM;
                    break;

                case "Family":
                    if (_familyVM == null)
                        _familyVM = new PeopleFamilyViewModel(_dataMgr, SelectedPerson as Person);

                    CurrentVM = _familyVM;
                    break;

                case "Individual":
					if (_individualVM == null)
                        _individualVM = new PeopleIndividualViewModel(_dataMgr, SelectedPerson as Person);

                    CurrentVM = _individualVM;
					break;

                default:
					break;
			}
		}
		#endregion
    }
}
