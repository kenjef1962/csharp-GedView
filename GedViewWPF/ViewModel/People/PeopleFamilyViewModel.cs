using GalaSoft.MvvmLight.Messaging;
using GedViewWPF.DataAccess;
using GedViewWPF.Model;
using GedViewWPF.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System;
using GedViewWPF.Messaging;

namespace GedViewWPF.ViewModel
{
    class PeopleFamilyViewModel : ViewModelBase
    {
        private Person _husband;
        public Person Husband
        {
            get { return _husband; }
            set
            {
                _husband = value;
                OnPropertyChanged("Husband");
            }
        }

        private Person _husbandFather;
        public Person HusbandFather
        {
            get { return _husbandFather; }
            set
            {
                _husbandFather = value;
                OnPropertyChanged("HusbandFather");
            }
        }

        private Person _husbandMother;
        public Person HusbandMother
        {
            get { return _husbandMother; }
            set
            {
                _husbandMother = value;
                OnPropertyChanged("HusbandMother");
            }
        }

        private Person _wife;
        public Person Wife
        {
            get { return _wife; }
            set
            {
                _wife = value;
                OnPropertyChanged("Wife");
            }
        }

        private Person _wifeFather;
        public Person WifeFather
        {
            get { return _wifeFather; }
            set
            {
                _wifeFather = value;
                OnPropertyChanged("WifeFather");
            }
        }

        private Person _wifeMother;
        public Person WifeMother
        {
            get { return _wifeMother; }
            set
            {
                _wifeMother = value;
                OnPropertyChanged("WifeMother");
            }
        }

        private List<Person> _children;
        public List<Person> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                OnPropertyChanged("Children");
            }
        }

        // Private Properties
        private readonly DataManager _dataMgr;

        public PeopleFamilyViewModel(DataManager dataMgr, Person person)
        {
            _dataMgr = dataMgr;

			UpdatePersonDetails(person);

            Messenger.Default.Register<MessageArgs>(this, "SelectedPersonChanged", OnSelectedPersonChanged);
        }

        private void OnSelectedPersonChanged(MessageArgs args)
        {
			if (args.Sender == this) return;

			UpdatePersonDetails(args.Data as Person);
        }

        private void UpdatePersonDetails(Person person)
        {
            Husband = null;
            HusbandFather = null;
            HusbandMother = null;

            Wife = null;
            WifeFather = null;
            WifeMother = null;

            Children = null;

            if (person != null)
            {
                var family = _dataMgr.GetFamilyByParent(person.ID);
                if (family != null)
                {
                    Husband = family.Husband;
                    Wife = family.Wife;
                    Children = new List<Person>(family.Children);
                }
                else
                {
                    if (person.Sex == "F")
                        Wife = person;
                    else
                        Husband = person;
                }

                if (Husband != null)
                {
                    var parents = _dataMgr.GetFamilyByChild(Husband.ID);
                    if (parents != null)
                    {
                        HusbandFather = parents.Husband;
                        HusbandMother = parents.Wife;
                    }
                }

                if (Wife != null)
                {
                    var parents = _dataMgr.GetFamilyByChild(Wife.ID);
                    if (parents != null)
                    {
                        WifeFather = parents.Husband;
                        WifeMother = parents.Wife;
                    }
                }
            }
        }
    }
}