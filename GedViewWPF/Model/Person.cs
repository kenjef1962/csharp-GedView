using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedViewWPF.Model
{
    public class Person : IComparable<Person>
    {
        public string ID { get; set; }
        public string Fullname { get; set; }
        public string Surname { get; set; }
        public string Given { get; set; }
        public string Sex { get; set; }
        public Fact Birth { get; set; }
        public Fact Marriage { get; set; }
        public Fact Death { get; set; }
        public string Lifespan { get; set; }

        public int CitationCount { get; set; }
        public int MediaCount { get; set; }
        public int NoteCount { get; set; }

        public List<Fact> Facts { get; set; }

        public override string ToString()
        {
            return Surname + ", " + Given;
        }

        public int CompareTo(Person person)
        {
            var result = 0;

            // Compare Surnames
            if ((Surname != null) && (person.Surname != null))
                result = Surname.CompareTo(person.Surname);
            else if ((Surname != null) && (person.Surname == null))
                return - 1;
            else if ((Surname == null) && (person.Surname != null))
                return 1;

            // Compare Given
            if (result == 0)
            {
                if ((Given != null) && (person.Given != null))
                    result = Given.CompareTo(person.Given);
                else if ((Given != null) && (person.Given == null))
                    return -1;
                else if ((Given == null) && (person.Given != null))
                    return 1;
            }

            // Compare Birth
            if (result == 0)
            {
                if ((Birth != null) && (person.Birth != null))
                    result = Birth.CompareTo(person.Birth);
                else if ((Birth != null) && (person.Birth == null))
                    return -1;
                else if ((Birth == null) && (person.Birth != null))
                    return 1;
            }

            // Compare Death
            if (result == 0)
            {
                if ((Death != null) && (person.Death != null))
                    result = Death.CompareTo(person.Death);
                else if ((Death != null) && (person.Death == null))
                    return -1;
                else if ((Death == null) && (person.Death != null))
                    return 1;
            }

            return result;
        }
    }
}
