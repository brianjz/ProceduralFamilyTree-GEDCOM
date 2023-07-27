using GeneGenie.Gedcom;
using GeneGenie.Gedcom.Enums;
using GeneGenie.Gedcom.Parser;
using Microsoft.VisualBasic;
using ProceduralFamilyTree;
using ProceduralFamilyTreeGEDCOM;
using System;
using System.Reflection.PortableExecutable;

// read in blank GEDCOM since GeneGenie cannot create a new one from scratch, yet
var sourceFile = "./data/new.ged";
var dbReader = GedcomRecordReader.CreateReader(sourceFile);
var db = dbReader.Database;

// Create random family with specified generations from their children
var first = Family.CreateNewRandomFamily(1915);
first.CreateGenerations(1);

// Primary husband
var husband = Processes.CreatePerson(first.Husband, dbReader);
first.Husband.PersonNumber = husband.XRefID;
Console.WriteLine($"Added record for Husband '{husband.GetName().Name}' with birth date {husband.Birth.Date.Date1}.");

// Primary wife
var wife = Processes.CreatePerson(first.Wife, dbReader);
first.Wife.PersonNumber = wife.XRefID;
Console.WriteLine($"Added record for Wife '{wife.GetName().Name}' with birth date {wife.Birth.Date.Date1}.");

// Primary family
var fam = new GedcomFamilyRecord(db, husband, wife);
var marriageDate = new GedcomDate(db);
marriageDate.ParseDateString(first.MarriageDate.ToString("dd MMM yyyy"));
var marriage = fam.AddNewEvent(GedcomEventType.MARR);
marriage.Date = marriageDate;

var currentHead = first.Husband;

foreach (var descendant in first.Husband.GetNestedChildren())
{
    if (descendant != first.Husband && descendant != first.Wife)
    {
        var child = Processes.CreatePerson(descendant, dbReader);
        descendant.PersonNumber = child.XRefID;
        if (currentHead != descendant.BirthFamily.Husband)
        {
            GedcomIndividualRecord famHusband;
            GedcomIndividualRecord famWife;
            currentHead = descendant.BirthFamily.Husband;
            if (String.IsNullOrEmpty(currentHead.PersonNumber))
            {
                famHusband = Processes.CreatePerson(currentHead, dbReader);
                famWife = (GedcomIndividualRecord)db[currentHead.Family.Wife.PersonNumber];
            }
            else
            {
                famHusband = (GedcomIndividualRecord)db[currentHead.PersonNumber];
                famWife = Processes.CreatePerson(currentHead.Family.Wife, dbReader);
            }
            fam = new GedcomFamilyRecord(db, famHusband, famWife);
        }
        fam.AddChild(child);
        Console.WriteLine($"Added record for '{child.GetName().Name}' with birth date {child.Birth.Date.Date1}.");
    }
}

string outputGEDCOM = "k:\\temp\\rewritten.ged";
GedcomRecordWriter.OutputGedcom(db, outputGEDCOM);
Console.WriteLine($"Output database to rewritten.ged.");

// Read all lines from the file into memory
string[] lines = File.ReadAllLines(outputGEDCOM);

// Filter out the empty lines
string[] nonEmptyLines = Array.FindAll(lines, line => !string.IsNullOrWhiteSpace(line));

// Overwrite the original file with the non-empty lines
File.WriteAllLines(outputGEDCOM, nonEmptyLines);
