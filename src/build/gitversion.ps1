$isPreview = "$env:PreviewBuild" -eq [bool]::TrueString

$gitVersionOutput = & "$PSScriptRoot\..\lib\GitVersion\GitVersion.exe"
$buildVersion = ($gitVersionOutput | ConvertFrom-Json).MajorMinorPatch
$buildBuild = ($gitVersionOutput | ConvertFrom-Json).BuildMetaData

$buildLabel = "prerelease"
$buildConfiguration = "Debug"

$buildVersionSemver = $buildVersion + "-" + $buildLabel + $buildBuild

if (-Not $isPreview)
{
    $buildVersionSemver = $buildVersion
	$buildConfiguration = "Release"
}

$buildVersionComplete = $buildVersionSemver + "+" + (Get-Date).ToString("yyyyMMdd") + "." + $env:BUILD_BUILDID

Write-Host "buildVersion:         " $buildVersion
Write-Host "buildVersionSemver:   " $buildVersionSemver

Write-Host "##vso[build.updatebuildnumber]" $buildVersionComplete
Write-Host "##vso[task.setvariable variable=buildVersionSemver]" $buildVersionSemver
Write-Host "##vso[task.setvariable variable=buildVersion]" $buildVersion"."$buildBuild
Write-Host "##vso[task.setvariable variable=buildConfiguration]" $buildConfiguration