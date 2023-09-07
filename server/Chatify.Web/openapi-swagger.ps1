$directoryPath = "$PSScriptRoot"
$directoryPath = Split-Path -Path $directoryPath -Parent
$directoryPath = Split-Path -Path $directoryPath -Parent
echo $directoryPath

Copy-Item -Path "$PSScriptRoot\swagger\v1\swagger.yaml" -Destination "$directoryPath\web\libs\api\openapi\swagger.yaml"