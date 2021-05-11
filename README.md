# TCCQIF

Takes a UTF16-LE tsv (quote enclosed tab delimited file) and generates a .qif version of it

You need the .Net Framework developer pack installed: https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-developer-pack-offline-installer

Copmpile with:

%windir%\Microsoft.Net\Framework\v4.0.30319\csc /t:exe c:\Users\timot\Documents\tccqif.cs -r:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\Microsoft.VisualBasic.dll"

Usage: tccqif.exe myfile.tsv
