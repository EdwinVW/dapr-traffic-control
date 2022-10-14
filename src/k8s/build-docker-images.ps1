$path = $MyInvocation.MyCommand.Path | Split-Path

& docker build --tag dapr-trafficcontrol/mosquitto:1.0 "$path/mosquitto"
& docker build --tag dapr-trafficcontrol/trafficcontrolservice:1.0 "$path/../trafficcontrolservice"
& docker build --tag dapr-trafficcontrol/finecollectionservice:1.0 "$path/../finecollectionservice"
& docker build --tag dapr-trafficcontrol/vehicleregistrationservice:1.0 "$path/../vehicleregistrationservice"
& docker build --tag dapr-trafficcontrol/simulation:1.0 "$path/../simulation"
