# ListMissingResourceItems
Compares the keys/values in a resx file with the same resx file in a remote branch and **translates** all new/changed values and saves the result to an Excel file or resx file(s).  
If no remote branch is specified, all values in the source resx file are translated.

`source-resx-file` path to the the main resx file to use as source.  
`remote-branch-name` name of the remote branch to compare the resx file with. If omitted, all items are translated. (optional)  
`target-excel-file` path to the Excel file to save the result to. (optional)  
`target-resx-file` path to the main resx file to save the result to. If no target is specified (excel nor resx), source-resx-file will be used as target.  (Translations will end up in correct related file) (optional)  
`translator` indicates which translator to use. (optional)  
* `GoogleTranslateLite` (default and free)
* `GoogleMlTranslator` (requires an api key put in a "GoogleAuthKey.txt" in application dir)  

`open-excel` indicates whether to open the Excel file after it is created, default is false. (optional)

Example (Translate diff between current branch and master, save result to source resx file):  
`ListMissingResourceItems.exe --translator GoogleMlTranslator --source-resx-file C:\MyRepo\Texts.resx --remote-branch-name master`

Example (translate all items):  
`ListMissingResourceItems.exe --source-resx-file C:\MyRepo\Texts.resx`

Example (Translate diff between current branch and master, save result to an Excel file and open it):  
`ListMissingResourceItems.exe --source-resx-file C:\MyRepo\Texts.resx --remote-branch-name master --target-excel-file C:\temp\out.xlsx --open-excel true`

# WriteMissingResourceItems
Imports the Excel file created by ListMissingResourceItems back to the resx files.  

`source-excel-file` path to the Excel file to use as source.  
`target-resx-file` path to the main resx file to save the result to.  

Example:  
`WriteMissingResourceItems.exe --target-resx-file C:\R\MyRepo\Resources\TextsIde.resx --source-excel-file C:\temp\out.xlsx`
