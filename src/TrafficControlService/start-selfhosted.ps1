# specify 'consul' as the first argument to use consul for name resolution
$configFile = if ($Args[0] -eq "consul") 
{  
    "../dapr/config/consul-config.yaml"
} 
else 
{ 
    "../dapr/config/config.yaml"
} 

dapr run `
    --app-id trafficcontrolservice `
    --app-port 6000 `
    --dapr-http-port 3600 `
    --dapr-grpc-port 60000 `
    --config $configFile `
    --resources-path ../dapr/components `
    dotnet run