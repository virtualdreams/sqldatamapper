<#
	Notes:
		Author: Thomas Kindler
		Date:	10.10.2014
	
	Parameters
		file: path to file
		git: path to git binary
	
	Example:
		PS C:\ git-hash.ps1 -file "/foo/bar"
		powershell -file "/path/to/git-hash.ps1" -file "/path/to/file" [-git "/path/to/git"]
#>
param
(
	[string]$file,
	[string]$git = 'C:\Program Files (x86)\Git\bin\git.exe'
)

### test if target file exists
if(!$file -or !(Test-Path $file))
{
	Write-Host "Path is not set or found."
	return
}

### test if git executeable exists
if(!(Test-Path $git))
{
	Write-Host "git command could not be found."
	return
}

### set git exe as alias
New-item alias:git -value $git | Out-Null
if(!(Get-Command git -TotalCount 1 -ErrorAction SilentlyContinue))
{
	Write-Host "git alias could not be found."
	return
}

### pattern to search
$pattern = "\[assembly: AssemblyInformationalVersion\(""(.*)""\)\]"
$update = $false
$revision = git describe --long --always --dirty=-dev

if($?)
{
	$content = Get-Content $file -Encoding UTF8
	$replace = @()
	foreach($line in $content) {
		if($line -match $pattern) {
			$value = [string]$matches[1]
			if($value -ne $revision) {
				$line = "[assembly: AssemblyInformationalVersion(""{0}"")]" -f $revision
				
				Write-Host ([string]::Format("Hash updated to: {0}", $revision))
				
				$update = $true
			}
		}
		$replace += [Array]$line
	}
	
	if($update -eq $true) {
		$replace | Out-File $file -Encoding UTF8 -force
	}
}
else
{
	Write-Host "git returned an error: $lastexitcode."
}