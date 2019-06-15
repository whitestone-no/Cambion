$isPreview = "$env:PreviewBuild" -eq [bool]::TrueString

$gitVersionOutput = & "$PSScriptRoot\..\lib\GitVersion\GitVersion.exe"

$buildVersion = ($gitVersionOutput | ConvertFrom-Json).MajorMinorPatch
$buildBuild = ($gitVersionOutput | ConvertFrom-Json).BuildMetaData
$buildLabel = "prerelease"

if ($isPreview)
{
    $buildVersionSemver = $buildVersion + "-" + $buildLabel + $buildBuild
}

$buildVersionComplete = $buildVersionSemver + "+" + $env:BUILD_BUILDNUMBER

Write-Host "buildVersion:         " $buildVersion
Write-Host "buildVersionSemver:   " $buildVersionSemver

Write-Host "##vso[build.updatebuildnumber]" $buildVersionComplete
Write-Host "##vso[task.setvariable variable=buildVersionSemver]" $buildVersionSemver
Write-Host "##vso[task.setvariable variable=buildVersion]" $buildVersion"."$buildBuild