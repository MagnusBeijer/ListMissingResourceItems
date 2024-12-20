# ListMissingResourceItems
Fetches all untranslated string from related resx files, calls the google lite translation api and then puts the result in an Excel file.  

`source-resx-file` path to the main resx file to use as source.  
`target-excel-file` path to the Excel file to save the result to.  
`take-from-key` is optional and indicates which key to start reading from. All items before this key will be skipped.  
`items-to-read` is optional and indicates the nr. of items to read from the end. ("take-from-key" is evaluated before this one.)  
`translator` is optional and indicates which translator to use for translations. Can be either "GoogleTranslateLite" (default and free) or GoogleMlTranslator (requires an api key put in a "GoogleAuthKey.txt")  

Example:  
`--source-resx-file C:\MyApp\\Resources\TextsIde.resx --target-excel-file C:\temp\out.xlsx --items-to-read 11`

# WriteMissingResourceItems
Imports the Excel file created by ListMissingResourceItems back to the resx files.  

`source-excel` path to the Excel file to use as source.  
`target-excel-file` path to the main resx file to save the result to.  

Example:  
`--target-resx-file C:\MyApp\\Resources\TextsIde.resx --source-excel-file C:\temp\out.xlsx`
