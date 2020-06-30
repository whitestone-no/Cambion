$isPreview = "$env:PreviewBuild" -eq [bool]::TrueString

$gitVersionOutput = & "$PSScriptRoot\GitVersion\GitVersion.exe"
$buildVersion = ($gitVersionOutput | ConvertFrom-Json).MajorMinorPatch
$buildBuild = ($gitVersionOutput | ConvertFrom-Json).BuildMetaData
$buildLabel = ($gitVersionOutput | ConvertFrom-Json).PreReleaseLabel

$buildConfiguration = "Debug"
$buildVersionSemver = $buildVersion + "-" + $buildLabel + $buildBuild

if (-Not $isPreview)
{
    $buildVersionSemver = $buildVersion
	$buildConfiguration = "Release"
}

$buildVersionComplete = $buildVersionSemver + "+" + (Get-Date).ToString("yyyyMMdd") + "." + $env:BUILD_BUILDID

Write-Host ("##vso[build.updatebuildnumber]{0}" -f $buildVersionComplete)
Write-Host ("##vso[task.setvariable variable=buildVersionSemver]{0}" -f $buildVersionSemver)
Write-Host ("##vso[task.setvariable variable=buildVersion]{0}.{1}" -f $buildVersion, $buildBuild)
Write-Host ("##vso[task.setvariable variable=buildConfiguration]{0}" -f $buildConfiguration)