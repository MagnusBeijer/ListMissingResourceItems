# ListMissingResourceItems
Fetches all untranslated string from related resx files, calls the google lite translation api and then puts the result in an Excel file.

To run:  
`--source-resx-file C:\\R\\iXDeveloper\\Resources\\ResourcesIde\\Texts\\TextsIde.resx --target-excel-file C:\\temp\\out.xlsx`

# WriteMissingResourceItems
Imports the Excel file created by ListMissingResourceItems back to the resx files.  

To run:  
`--target-resx-file C:\\R\\iXDeveloper\\Resources\\ResourcesIde\\Texts\\TextsIde.resx --source-excel-file C:\\temp\\out.xlsx`