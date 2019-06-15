$isPreview = "$env:PreviewBuild" -eq [bool]::TrueString

$gitVersionOutput = & "$PSScriptRoot\..\lib\GitVersion\GitVersion.exe"

$buildVersion = ($gitVersionOutput | ConvertFrom-Json).MajorMinorPatch
$buildBuild = ($gitVersionOutput | ConvertFrom-Json).BuildMetaData
$buildLabel = ($gitVersionOutput | ConvertFrom-Json).PreReleaseLabel

if ($isPreview) {
    $buildVersion = $buildVersion + "-" + $buildLabel + $buildBuild
}

$buildVersionComplete = $buildVersion + "+" + ${env:Build.Date:ddddMMyy} + ${env:Build.Rev:r}

Write-Host "##vso[build.updatebuildnumber]" + $buildVersionComplete
Write-Host "##vso[task.setvariable variable=buildVersion]" + $buildVersion