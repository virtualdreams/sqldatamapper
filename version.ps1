<#
	.SYNOPSIS
	
	This script generate version numbers from a git repository.
		
	.DESCRIPTION
	
	This script generate version numbers from a git repository. Major and minor
	version number are created from a tag if in form of 'v1.0-release...'.
	Otherwise, the version numbers are extracted from file.
	
	.PARAMETER file
	
	Path to 'AssemblyInfo.cs'
	
	.PARAMETER git
	
	Path to 'git.exe'
	
	.EXAMPLE
	
	PS C:\ git-hash.ps1 -file "/foo/bar"
	
	.EXAMPLE
	
	powershell -file ".\version.ps1" -file "/path/to/file" [-git "/path/to/git"]
	
	.NOTES
	
	Author: Thomas Kindler
	Date:	02.04.2015
	
	.LINK
	
	github: https://gist.github.com/virtualdreams/23004251c9ab4f2d04db
#>
param
(
	[string]$file = '.\Properties\AssemblyInfo.cs',
	[string]$git = "${env:ProgramW6432}\git\bin\git.exe"
)

### test if the git executeable exists
if(!(Test-Path $git))
{
	Write-Host "git command could not be found."
	return
}

### set git exe as alias
Set-Alias git $git
if(!(Get-Command git -TotalCount 1 -ErrorAction SilentlyContinue))
{
	Write-Host "git alias could not be found."
	return
}

### test if target file exists
if(!$file -or !(Test-Path $file))
{
	Write-Host "Path to 'AssemblyInfo.cs' is not set or not found."
	return
}

### get total commit count
$commits = git rev-list HEAD --count

### get a description
$information = git describe --long --always --dirty=-dev

$major = "1"
$minor = "0"
$m = $false

### look if a tag like v1.0-release is created and extract the version number
if($information -match "^v(\d+).(\d+).*") {
	$major = [string]$matches[1]
	$minor = [string]$matches[2]
	
	$m = $true
}

(Get-Content $file -Encoding UTF8) | Foreach-Object {
	if($_ -match "\[assembly: AssemblyVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)\""\)\]") {
		if($m) {
			$_ -replace "\[assembly: AssemblyVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)\""\)\]", "[assembly: AssemblyVersion(""$major.`$2.`$3.`$4"")]"
			Write-Host ([string]::Format("Assembly version (from tag): {0}.{1}.{2}.{3}", $major, [string]$matches[2], [string]$matches[3], [string]$matches[4]))
		}
		else
		{
			$_ -replace "\[assembly: AssemblyVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)\""\)\]", "[assembly: AssemblyVersion(""`$1.`$2.`$3.`$4"")]"
			Write-Host ([string]::Format("Assembly version (from file): {0}.{1}.{2}.{3}", [string]$matches[1], [string]$matches[2], [string]$matches[3], [string]$matches[4]))
		}
	}
	elseif($_ -match "\[assembly: AssemblyFileVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)\""\)\]") {
		if($m) {
			$_ -replace "\[assembly: AssemblyFileVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)\""\)\]", "[assembly: AssemblyFileVersion(""$major.$minor.`$3.$commits"")]"
			Write-Host ([string]::Format("File version (from tag): {0}.{1}.{2}.{3}", $major, $minor, [string]$matches[3], $commits))
		}
		else
		{
			$_ -replace "\[assembly: AssemblyFileVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)\""\)\]", "[assembly: AssemblyFileVersion(""`$1.`$2.`$3.$commits"")]"
			Write-Host ([string]::Format("File version (from file): {0}.{1}.{2}.{3}", [string]$matches[1], [string]$matches[2], [string]$matches[3], $commits))
		}
	}
	elseif($_ -match "\[assembly: AssemblyInformationalVersion\(""(.*)""\)\]") {
		$_ -replace "\[assembly: AssemblyInformationalVersion\(""(.*)""\)\]", "[assembly: AssemblyInformationalVersion(""$information"")]"
		Write-Host ([string]::Format("Information version: {0}", $information))
	}
	else
	{
		$_
	}
} | Out-File $file -Encoding UTF8
