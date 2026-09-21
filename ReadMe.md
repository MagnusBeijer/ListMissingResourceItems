# ListMissingResourceItems

Compares the keys/values in a resx file with the same resx file in a remote branch and **translates** all new/changed values, then saves the result to an Excel file or resx file(s).

> If no remote branch is specified, **all** values in the source resx file are translated.

## Options

**`--source-resx-file`** *(required)*  
Path to the main resx file to use as source.

**`--remote-branch-name`**  
Name of the remote branch to compare the resx file with. If omitted, all items are translated.

**`--target-excel-file`**  
Path to the Excel file to save the result to.

**`--target-resx-file`**  
Path to the main resx file to save the result to. Translations end up in the correct related language file. If no target is specified (neither Excel nor resx), `--source-resx-file` is used as target.

**`--translator`**  
Which translator to use. Default: `GoogleTranslateLite`.
- **`GoogleTranslateLite`** - default and free.
- **`GoogleMlTranslator`** - requires an API key placed in a `GoogleAuthKey.txt` file in the application directory.

**`--open-excel`**  
Open the Excel file after it is created. Default: `false`.

## Examples

Translate the diff between the current branch and `master`, saving the result to the source resx file:

```powershell
ListMissingResourceItems.exe --translator GoogleMlTranslator --source-resx-file C:\MyRepo\Texts.resx --remote-branch-name master
```

Translate all items:

```powershell
ListMissingResourceItems.exe --source-resx-file C:\MyRepo\Texts.resx
```

Translate the diff between the current branch and `master`, saving the result to an Excel file and opening it:

```powershell
ListMissingResourceItems.exe --source-resx-file C:\MyRepo\Texts.resx --remote-branch-name master --target-excel-file C:\temp\out.xlsx --open-excel true
```

# WriteMissingResourceItems

Imports the Excel file created by ListMissingResourceItems back into the resx files.

## Options

**`--source-excel-file`** *(required)*  
Path to the Excel file to use as source.

**`--target-resx-file`** *(required)*  
Path to the main resx file to save the result to.

## Example

```powershell
WriteMissingResourceItems.exe --target-resx-file C:\R\MyRepo\Resources\TextsIde.resx --source-excel-file C:\temp\out.xlsx
```
