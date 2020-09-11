# dapr Traffic Control Sample
This repository contains a sample application that simulates a traffic-control system using dapr. For this sample I've used a speeding-camera setup as can be found on several Dutch highways. Over the entire stretch the average speed of a vehicle is measured and if it is above the speeding limit on this highway, the driver of the vehicle receives a speeding ticket.

## Overview
This is an overview of the fictitious setup I'm simulating in this sample:

![](img/speed-trap-overview.png)

There's 1 entry-camera and 1 exit-camera per lane. When a car passes an entry-camera, the license-number of the car is registered.

In the background, information about the vehicle  is retrieved from the Department Of Motor-vehicles - DMV (or RDW in Dutch) by calling their web-service.

When the car passes an exit-camera, this is registered by the system. The system then calculates the average speed of the car based on the entry- and exit-timestamp. If a speeding violation is detected, a message is sent to the Central Judicial Collection Agency - CJCA (or CJIB in Dutch) will send a speeding-ticket to the driver of the vehicle.

## Simulation
In order to simulate this in code, I created several services as shown below:

![](img/services.png)

- The **Simulation** is a .NET Core console application that will simulate passing cars.
- The **TrafficControlService** is an ASP.NET Core WebAPI application that offers 2 endpoints: *Entrycam* and *ExitCam*.
- The **Government** service is an ASP.NET Core WebAPI application that offers 2 endpoints: *RDW* (for retrieving vehicle information) and *CJIB* (for sending speeding tickets).

The way the simulation works is depicted in the sequence diagram below:

![](img/sequence.png)

1. The **Simulation** generates a random license-number and sends a *VehicleRegistered* message (containing this license-number, a random entry-lane (1-3) and the timestamp) to the *EntryCam* endpoint of the **TrafficControlService**.
2. The **TrafficControlService** calls the *RDW* endpoint of the **GovernmentService** to retrieve the brand and model of the vehicle corresponding to the license-number.
3. The **TrafficControlService** stores the VehicleState (vehicle information and entry-timestamp) in the state-store.
4. After some random interval, the **Simulation** sends a *VehicleRegistered* message to the *ExitCam* endpoint of the **TrafficControlService** (containing the license-number generated in step 1, a random exit-lane (1-3) and the exit timestamp).
5. The **TrafficControlService** retrieves the VehicleState from the state-store.
6. The **TrafficControlService** calculates the average speed of the vehicle using the entry- and exit-timestamp.
7. If the average speed is above the speed-limit, the **TrafficControlService** will sent a *SpeedingViolationDetected* message (containing the license-number of the vehicle, the identifier of the road, the speeding-violation in KMh and the timestamp of the violation) to the *CJIB* endpoint of the **GovernmentService**.
8. The **GovernmentService** calculates the fine for the speeding-violation and simulates sending a speeding-ticket to the owner of the vehicle.

All actions described in this sequence are logged to the console during execution so you can follow the flow.

## dapr
This sample uses dapr for implementing several aspects of the application. For communicating messages, the **publish and subscribe** building-block is used. For doing request/response type communication with a service, the  **service-to-service invocation** building-block is used. And for storing the state of a vehicle, the **state management** building-block is used.

![](img/dapr-setup.png)

In this sample, the Reddis component is used for both state management as well as for pub/sub.

## Running the sample (using dapr self-hosted mode)
Execute the following steps to run the sample application:

1. Make sure you have installed dapr on your machine as described in the [dapr documentation](https://github.com/dapr/docs/blob/master/getting-started/environment-setup.md).
2. Open three separate command-shells.
3. In the first shell, execute the following command (using the dapr cli) to run the **TrafficControlService**:
  ```
  dapr run --app-id trafficcontrolservice --app-port 5000 --dapr-grpc-port 50001 dotnet run
  ```
4. In the second shell, execute the following command (using the dapr cli) to run the **GovernmentService**:
  ```
  dapr run --app-id governmentservice --app-port 6000 --dapr-grpc-port 50002 dotnet run
  ```
5. In the third shell, execute the following command to run the **Simulation**:
```
dotnet run
```

You should now see logging in each of the shells, similar to the logging shown below:

**Simulation:**
![](img/logging-simulation.png)

**TrafficControlService:**
![](img/logging-trafficcontrolservice.png)

**GovernmentService:**
![](img/logging-governmentservice.png)

## Disclaimer
The code in this repo is NOT production grade and lacks any automated testing. It is intentionally kept as simple as possible (KISS). Its primary purpose is demonstrating several dapr concepts and not being a full fledged application that can be put into production as is.

The author can in no way be held liable for damage caused directly or indirectly by using this code.
