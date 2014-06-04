param
(
	[string]$file
)

if(!$file)
{
	Write-Host "Path is not set."
	return
}

if(!(Test-Path $file))
{
	Write-Host "Path not found."
	return
}

# set git exe as alias - TODO
New-item alias:git -value 'C:\Program Files (x86)\Git\bin\git.exe' | Out-Null

if(!(Get-Command git -TotalCount 1 -ErrorAction SilentlyContinue))
{
	Write-Host "git command could not be found."
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