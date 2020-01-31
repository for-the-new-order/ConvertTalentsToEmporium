# ConvertTalentsToEmporium
Convert JSON/YAML talents used by the website (Jekyll) to the character sheet ("Genesys Emporium") format.

## How to use
Run the program passing 2 or more file path as arguments. The last file is the destination while the others are the sources (combined together). 

Example:

```
ConvertTalentsToEmporium.exe c:\talents1.json c:\talents2.yml c:\destination.json
```

Or use VS and update the `launchSettings.json`.
