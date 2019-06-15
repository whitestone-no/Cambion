$isPreview = "$env:PreviewBuild" -eq [bool]::TrueString

$gitVersionOutput = & "$PSScriptRoot\..\lib\GitVersion\GitVersion.exe"

$buildVersion = ($gitVersionOutput | ConvertFrom-Json).MajorMinorPatch
$buildBuild = ($gitVersionOutput | ConvertFrom-Json).BuildMetaData
$buildLabel = "prerelease"

if ($isPreview)
{
    $buildVersion = $buildVersion + "-" + $buildLabel + $buildBuild
}

$buildVersionComplete = $buildVersion + "+" + $env:BUILD_BUILDNUMBER

Write-Host "buildVersion:         " $buildVersion
Write-Host "buildBuild:           " $buildBuild
Write-Host "buildVersionComplete: " $buildVersionComplete

Write-Host "##vso[build.updatebuildnumber]" $buildVersionComplete
Write-Host "##vso[task.setvariable variable=currentVersion]" $buildVersion