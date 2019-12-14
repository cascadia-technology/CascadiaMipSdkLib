$projectDir = $Args[0]
$outputDir = $Args[1]
$config = $Args[2]
$projectName = $Args[3]

## Only build nuget package when config is Publish
if ($config -ne "Publish") { return }

$InformationPreference = [System.Management.Automation.ActionPreference]::Continue
Write-Information "`$projectDir = $projectDir"
Write-Information "`$outputDir = $outputDir"
Write-Information "`$config = $config"
Write-Information "`$projectName = $projectName"

function IsAdministrator
{
    $Identity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $Principal = New-Object System.Security.Principal.WindowsPrincipal($Identity)
    $Principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)
}

function IsUacEnabled
{
    (Get-ItemProperty HKLM:\Software\Microsoft\Windows\CurrentVersion\Policies\System).EnableLua -ne 0
}

try {
    $package = Get-Package -Name NuGet.CommandLine -ErrorAction Stop
}
catch {
    if (!(IsAdministrator))
    {
        if (IsUacEnabled)
        {
            [string[]]$argList = @('-NoProfile', '-File', $MyInvocation.MyCommand.Path)
            $argList += $MyInvocation.BoundParameters.GetEnumerator() | Foreach {"-$($_.Key)", "$($_.Value)"}
            $argList += $MyInvocation.UnboundArguments
            Start-Process PowerShell.exe -Verb Runas -WorkingDirectory $pwd -ArgumentList $argList
            return
        }
        else
        {
            throw "You must be administrator to run this script"
        }
    }
    $package = Install-Package -Name NuGet.CommandLine -Force -Verbose
}

try {

    $NuGet = "$(($package.Source | Get-Item).Directory)\tools\nuget.exe"

    $projectUrl = "https://github.com/cascadia-technology/CascadiaMipSdkLib"
    $iconUrl = "https://www.cascadia.tech/wp-content/uploads/2019/08/CascadiaIcon_white.ico"
    $tags = @("Cascadia","MIP SDK","XProtect","Milestone","VMS")

    Push-Location $projectDir
    [Environment]::CurrentDirectory = $projectDir
    Write-Information "Running nuget spec. . ."
    . $NuGet spec -force -AssemblyPath (Join-Path $outputDir "$projectName.dll")


    Write-Information "Modifying spec file. . ."
    $xml = [xml](Get-Content (Join-Path $projectDir "$($projectName).nuspec") -Raw)
    $metadata = $xml.SelectSingleNode("/package/metadata")
    $licenseUrlNode = $xml.SelectSingleNode("/package/metadata/licenseUrl")
    $metadata.RemoveChild($licenseUrlNode)
    $metadata.RemoveChild($xml.SelectSingleNode("/package/metadata/releaseNotes"))

    $licenseNode = $xml.CreateNode("element", "license", "")
    $typeAttrib = $xml.CreateAttribute("type")
    $typeAttrib.Value = "expression"
    $licenseNode.Attributes.Append($typeAttrib)
    $licenseNode.InnerText = "MIT"
    $metadata.AppendChild($licenseNode)

    $xml.SelectSingleNode("/package/metadata/projectUrl").InnerText = $projectUrl
    $xml.SelectSingleNode("/package/metadata/iconUrl").InnerText = $iconUrl
    $xml.SelectSingleNode("/package/metadata/tags").InnerText = [string]::Join(" ", $tags)

    $xml.Save((Join-Path $projectDir "$($projectName).nuspec"))
    
    Write-Information "Running nuget pack. . ."
    . $NuGet pack -properties Configuration=$config
}
catch {
    Write-Error -Exception $_.Exception
}
finally {
    #Write-Information "Press any key to continue. . ."
    #Read-Host
}
