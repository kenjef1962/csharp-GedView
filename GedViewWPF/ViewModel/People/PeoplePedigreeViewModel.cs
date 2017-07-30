using GalaSoft.MvvmLight.Messaging;
using GedViewWPF.DataAccess;
using GedViewWPF.Model;
using GedViewWPF.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using GedViewWPF.Messaging;

namespace GedViewWPF.ViewModel
{
	class PeoplePedigreeViewModel : ViewModelBase
	{
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

        private List<Person> _pedigree;
        public List<Person> Pedigree
        {
            get { return _pedigree; }
            set
            {
                _pedigree = value;
                OnPropertyChanged("Pedigree");
            }
        }

        // Private Properties
        private readonly DataManager _dataMgr;

        public PeoplePedigreeViewModel(DataManager dataMgr, Person person)
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
            var list = new List<Person>();
            for (int i = 0; i < 32; i++)
            {
                list.Add(null);
            }

            if (SelectedPerson != null)
            {
                list[0] = SelectedPerson;

                var family = _dataMgr.GetFamilyByParent(SelectedPerson.ID);
                if (family != null)
                {
                    if ((family.Husband != null) && (family.Husband.ID == SelectedPerson.ID))
                        list[1] = family.Wife;
                    else if ((family.Wife != null) && (family.Wife.ID == SelectedPerson.ID))
                        list[1] = family.Husband;
                }

                AddPeopleToPedigree(list, SelectedPerson, 2);
            }

            Pedigree = new List<Person>(list);
        }

        private void AddPeopleToPedigree(List<Person> list, Person person, int index)
        {
            if ((person != null) && (index < 31))
            {
                var family = _dataMgr.GetFamilyByChild(person.ID);
                if (family != null)
                {
                    list[index] = family.Husband;
                    list[index + 1] = family.Wife;

                    AddPeopleToPedigree(list, family.Husband, index * 2);
                    AddPeopleToPedigree(list, family.Wife, (index + 1) * 2);
                }
            }
        }
	}
}
