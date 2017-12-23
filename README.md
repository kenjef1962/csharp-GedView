# csharp-GedView

GedView is a Windows application used to display the basic information found in a GEDCOM file.  The application read a GEDCOM file and uses multiple views to display information found in the file.  The following are the was to view the GEDCOM data.

- Plan: The basic file info including the number of people, families, etc.
- People: The people view has multiple was to view the data.
  - List view: A list of all the individuals in the file.  Selecting a person in the list will reset all of the other people related views.
  - Pedigree view: A simple pedigree view for the selected person.  Selecting a person in the pedigree view will reset all of the other people related views.
  - Family view: A simple view of the family (person, spouse, parent, and children).  
  - Individual view: A view of the data related to the selected individual.  This includes the person's associated facts, source citations, and potential online search hints.

The project includes 3 separate sub-projects.
- Main application
- GEDCOM file reader project
- Unit test project

The main application was written using C# and WPF/XML using the MVVM architecture.  A number of reusable views (i.e. the simple person view that contains a person gender picture, name, and birth / death lifespan) have been utilized.  It also utilizes a simple publish/subscribe method to communicate cross view events (i.e. person selected).

The GEDCOM file reader was written to provide read support for files conforming to the GEDCOM 5.5 specification (specification file included in the project).  This is NOT a full read implementation of the specification, nor has performance and error handling been fully realized.

The unit test project is specific to the GEDCOM file reader an is less concerned about entire GEDCOM file processing than ensuring that section readers (i.e. SOUR, NOTE, INDI) are reading and processing data correctly.

The project was written using C# and WPF/XML using Visual Studio Pro 2015.
