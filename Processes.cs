using GeneGenie.Gedcom;
using GeneGenie.Gedcom.Enums;
using GeneGenie.Gedcom.Parser;
using ProceduralFamilyTree;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralFamilyTreeGEDCOM
{
    public class Processes
    {
        public static GedcomIndividualRecord CreatePerson(Person person, GedcomRecordReader rr)
        {
            var db = rr.Database;
            var individual = new GedcomIndividualRecord(db);
            string dateFormat = "dd MMM yyyy";

            var name = individual.Names[0];
            name.Given = person.FirstName;
            name.Surname = person.LastName;

            individual.Names.Add(name);

            var birthDate = new GedcomDate(db);
            birthDate.ParseDateString(person.BirthDate.ToString(dateFormat));
            individual.Events.Add(new GedcomIndividualEvent
            {
                Database = db,
                Date = birthDate,
                EventType = GedcomEventType.Birth,
            });
            if (person.DeathDate != DateTime.MinValue)
            {
                var deathDate = new GedcomDate(db);
                deathDate.ParseDateString(person.DeathDate.ToString(dateFormat));
                individual.Events.Add(new GedcomIndividualEvent
                {
                    Database = db,
                    Date = deathDate,
                    EventType = GedcomEventType.DEAT,
                });
            }
            return individual;
        }
    }
}
