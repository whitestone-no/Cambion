$isPreview = "$env:PreviewBuild" -eq [bool]::TrueString

$gitVersionOutput = & "../lib/GitVersion/GitVersion.exe"

$buildVersion = ($gitVersionOutput | ConvertFrom-Json).MajorMinorPatch
$buildBuild = ($gitVersionOutput | ConvertFrom-Json).BuildMetaData
$buildLabel = ($gitVersionOutput | ConvertFrom-Json).PreReleaseLabel

if ($isPreview) {
    $buildVersion = $buildVersion + "-" + $buildLabel + $buildBuild
}

$buildVersionComplete = $buildVersion + "+" + $(Date:yyyyMMdd) + $(Rev:.r)

Write-Host "##vso[build.updatebuildnumber]" + $buildVersionComplete
Write-Host "##vso[task.setvariable variable=buildVersion]" + $buildVersion