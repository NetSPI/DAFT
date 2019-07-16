## DAFT: Database Audit Framework & Toolkit 
This is a database auditing and assessment toolkit inspired by <a href="  https://github.com/NetSPI/PowerUpSQL/wiki">PowerUpSQL</a>.

### DAFT Command Examples
Below are a few common command examples to get you started.

#### List non-default databases
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -m "database" -n</pre>

#### List table for a database
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -d "database" -m "tables"</pre>

#### Search for senstive data by keyword
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -m "ColumnSampleData" --SearchKeywords="password,licence,ssn" --SampleSize=5</pre>

#### Search for senstive data by keyword and export results to json
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -m "ColumnSampleData" --SearchKeywords="password,licence,ssn" --SampleSize=5 -j -o "sensative_data_discovered.json"</pre>

#### Check for default or weak password
<pre>DAFT.exe -i "TEST-SYSTEM\SQLEXPRESS" -m "ServerLoginDefaultPw" -c -o "default_passwords_found.csv"</pre>

#### Execute command through SQL Server
<pre>DAFT.exe -i "Target\Instance" -m "OSCmd" -q "whoami"</pre>

### DAFT Help
Since we lack a proper wiki at the moment below is help output for the tool.



