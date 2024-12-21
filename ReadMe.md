# ListMissingResourceItems
Compares the keys/values in a resx file with the same resx file in a remote branch and translates all new/changed values and saves the result to an Excel file for review.

`repository-path` path to repository to use.  
`relative-resx-file` relative path (from the repository) to the main resx file to use as source.  
`remote-branch-name` name of the remote branch to compare the resx file with.  
`target-excel-file` path to the Excel file to save the result to.  
`translator` is optional and indicates which translator to use for translations. Can be either "GoogleTranslateLite" (default and free) or GoogleMlTranslator (requires an api key put in a "GoogleAuthKey.txt")  

Example:  
`ListMissingResourceItems.exe --repository-path C:\R\MyRepo --relative-resx-file Resources\TextsIde.resx --remote-branch-name master --target-excel-file C:\temp\out.xlsx`

# WriteMissingResourceItems
Imports the Excel file created by ListMissingResourceItems back to the resx files.  

`source-excel` path to the Excel file to use as source.  
`target-resx-file` path to the main resx file to save the result to.  

Example:  
`WriteMissingResourceItems.exe --target-resx-file C:\R\MyRepo\Resources\TextsIde.resx --source-excel-file C:\temp\out.xlsx`
