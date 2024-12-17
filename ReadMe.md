# ListMissingResourceItems
Fetches all untranslated string from related resx files, calls the google lite translation api and then puts the result in an Excel file.  
Parameter `items-to-read` is optional and indicates the nr of items to read from the end of the resx file.  

To run:  
`--source-resx-file C:\\MyApp\\Resources\\TextsIde.resx --target-excel-file C:\\temp\\out.xlsx --items-to-read 11`

# WriteMissingResourceItems
Imports the Excel file created by ListMissingResourceItems back to the resx files.  

To run:  
`--target-resx-file C:\\MyApp\\Resources\\TextsIde.resx --source-excel-file C:\\temp\\out.xlsx`
