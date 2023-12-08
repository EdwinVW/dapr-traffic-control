& ./mosquitto/stop-mosquitto.ps1
& ./rabbitmq/stop-rabbitmq.ps1
& ./maildev/stop-maildev.ps1

# specify 'consul' as the first argument if you've used consul for name resolution
if ($Args[0] -eq "consul") 
{  
    & ./consul/stop-consul.ps1
} 