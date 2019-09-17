$projectDir = $Args[0]
$outputDir = $Args[1]
$config = $Args[2]
$projectName = $Args[3]

if ($config -ne "Publish") { return }

try {
    $package = Get-Package -Name NuGet.CommandLine -ErrorAction Stop
}
catch {
    $package = Install-Package -Name NuGet.CommandLine -Force -Verbose
}

$NuGet = "$(($package.Source | Get-Item).Directory)\tools\nuget.exe"

$projectUrl = "https://github.com/cascadia-technology/CascadiaMipSdkLib"
$iconUrl = "https://www.cascadia.tech/wp-content/uploads/2019/08/CascadiaIcon_white.ico"
$tags = @("Cascadia","MIP SDK","XProtect","Milestone","VMS")

Push-Location $projectDir
[Environment]::CurrentDirectory = $projectDir

. $NuGet spec -force

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
. $NuGet pack