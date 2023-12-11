$projectDir = (Get-Item $PSScriptRoot).Parent.FullName
$exePath = "$projectDir\bin\Release\net8.0\Chatify.Web.exe"
New-Service -Name TestService2 -BinaryPathName $exePath