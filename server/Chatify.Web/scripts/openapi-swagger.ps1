$directoryPath = (Get-Item $PSScriptRoot).Parent.Parent.Parent.FullName
Copy-Item -Path "$PSScriptRoot\swagger\v1\swagger.yaml" -Destination "$directoryPath\web\libs\api\openapi\swagger.yaml"