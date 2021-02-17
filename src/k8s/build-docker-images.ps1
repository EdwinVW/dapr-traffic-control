docker build -t dapr-trafficcontrol/mosquitto:1.0 ./mosquitto
docker build -t dapr-trafficcontrol/trafficcontrolservice:1.0 ../trafficcontrolservice
docker build -t dapr-trafficcontrol/finecollectionservice:1.0 ../finecollectionservice
docker build -t dapr-trafficcontrol/vehicleregistrationservice:1.0 ../vehicleregistrationservice
docker build -t dapr-trafficcontrol/simulation:1.0 ../simulation
