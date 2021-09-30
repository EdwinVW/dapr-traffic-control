# Dapr Traffic Control Sample

| Attribute            | Details                   |
| -------------------- | ------------------------- |
| Dapr runtime version | v1.4.0                    |
| Dapr.NET SDK version | v1.4.0                    |
| Dapr CLI version     | v1.4.0                    |
| Language             | C#                        |
| Platform             | .NET 5                    |
| Environment          | Self hosted or Kubernetes |

This repository contains a sample application that simulates a traffic-control system using Dapr. For this sample I've used a speeding-camera setup as can be found on several Dutch highways. A set of cameras are placed at the beginning and the end of a stretch of highway. Using data from these cameras, the average speed of a vehicle is measured. If this average speed is above the speeding limit on this highway, the driver of the vehicle receives a fine.

## Overview

This is an overview of the fictitious setup I'm simulating in this sample:

![Speed trap overview](img/speed-trap-overview.png)

There's 1 entry-camera and 1 exit-camera per lane. When a car passes an entry-camera, the license-number of the car and the timestamp is registered.

When the car passes an exit-camera, this timestamp is also registered by the system. The system then calculates the average speed of the car based on the entry- and exit-timestamp. If a speeding violation is detected, a message is sent to the Central Fine Collection Agency (or CJIB in Dutch). They will retrieve the information of the owner of the vehicle and send him or her a fine.

## Simulation

In order to simulate this in code, I created the following services:

![Services](img/services.png)

- The **Camera Simulation** is a .NET Core console application that will simulate passing cars.
- The **Traffic Control Service** is an ASP.NET Core WebAPI application that offers 2 endpoints: `/entrycam` and `/exitcam`.
- The **Fine Collection Service** is an ASP.NET Core WebAPI application that offers 1 endpoint: `/collectfine` for collecting fines.
- The **Vehicle Registration Service** is an ASP.NET Core WebAPI application that offers 1 endpoint: `/vehicleinfo/{license-number}` for getting the vehicle- and owner-information of speeding vehicle.

The way the simulation works is depicted in the sequence diagram below:

![Sequence diagram](img/sequence.png)

1. The Camera Simulation generates a random license-number and sends a *VehicleRegistered* message (containing this license-number, a random entry-lane (1-3) and the timestamp) to the `/entrycam` endpoint of the TrafficControlService.
1. The TrafficControlService stores the VehicleState (license-number and entry-timestamp).
1. After some random interval, the Camera Simulation sends a *VehicleRegistered* message to the `/exitcam` endpoint of the TrafficControlService (containing the license-number generated in step 1, a random exit-lane (1-3) and the exit timestamp).
1. The TrafficControlService retrieves the VehicleState that was stored at vehicle entry.
1. The TrafficControlService calculates the average speed of the vehicle using the entry- and exit-timestamp. It also stores the VehicleState with the exit timestamp for audit purposes, but this is left out of the sequence diagram for clarity.
1. If the average speed is above the speed-limit, the TrafficControlService calls the `/collectfine` endpoint of the FineCollectionService. The request payload will be a *SpeedingViolation* containing the license-number of the vehicle, the identifier of the road, the speeding-violation in KMh and the timestamp of the violation.
1. The FineCollectionService calculates the fine for the speeding-violation.
1. The FineCollectionSerivice calls the `/vehicleinfo/{license-number}` endpoint of the VehicleRegistrationService with the license-number of the speeding vehicle to retrieve its vehicle- and owner-information.
1. The FineCollectionService sends a fine to the owner of the vehicle by email.

All actions described in this sequence are logged to the console during execution so you can follow the flow.

## Dapr

This sample uses Dapr for implementing several aspects of the application. In the diagram below you see a schematic overview of the setup:

![Dapr setup](img/dapr-setup.png)

1. For doing request/response type communication between the FineCollectionService and the VehicleRegistrationService, the **service invocation** building block is used.
1. For sending speeding violations to the FineCollectionService, the **publish and subscribe** building block is used. RabbitMQ is used as message broker.
1. For storing the state of a vehicle, the **state management** building block is used. Redis is used as state store.
1. Fines are sent to the owner of a speeding vehicle by email. For sending the email, the Dapr SMTP **output binding** is used.
1. The Dapr **input binding** for MQTT is used to send simulated car info to the TrafficControlService. Mosquitto is used as MQTT broker.
1. The FineCollectionService needs credentials for connecting to the smtp server and a license-key for a fine calculator component. It uses the **secrets management** building block with the local file component to get the credentials and the license-key.
1. The TrafficControlService has an alternative implementation based on Dapr **actors**. See [Run the application with actors](#run-the-application-with-dapr-actors) for instructions on how to run this.

Here is the sequence diagram again, but now with all the Dapr building blocks and components:

![Sequence diagram with Dapr building blocks](img/sequence-dapr.png)

## Run the application in Dapr self-hosted mode

In self-hosted mode everything will run on your local machine. To prevent port-collisions, all services listen on a different HTTP port. When running the services with Dapr, you need additional ports voor HTTP and gRPC communication with the sidecars. By default these ports are `3500` and `50001`. But to prevent confusion, you'll use totally different port numbers in the assignments. The services will use the following ports:

| Service                    | Application Port | Dapr sidecar HTTP port | Dapr sidecar gRPC port |
| -------------------------- | ---------------- | ---------------------- | ---------------------- |
| TrafficControlService      | 6000             | 3600                   | 60000                  |
| FineCollectionService      | 6001             | 3601                   | 60001                  |
| VehicleRegistrationService | 6002             | 3602                   | 60002                  |

The ports can be specified on the command-line when starting a service with the Dapr CLI. The following command-line flags can be used:

- `--app-port`
- `--dapr-http-port`
- `--dapr-grpc-port`

Execute the following steps to run the sample application in self hosted mode:

Start infrastructure components:

1. Make sure you have installed Dapr on your machine in self-hosted mode as described in the [Dapr documentation](https://docs.dapr.io/getting-started/install-dapr/).
1. Open a new command-shell.
1. Change the current folder to the `src/infrastructure` folder of this repo.
1. Start the infrastructure services by executing `start-all.ps1` script. This script will start Mosquitto (MQTT broker), RabbitMQ (pub/sub broker) and Maildev. Maildev is a development SMTP server that does not actually send out emails (by default). Instead, it offers a web frontend that will act as an email in-box showing the emails that were sent to the SMTP server. This is very convenient for demos of testscenarios.

Start the services:

1. Open a new command-shell.

1. Change the current folder to the `src/VehicleRegistrationService` folder of this repo.

1. Execute the following command (using the Dapr cli) to run the VehicleRegistrationService:

    ```console
    dapr run --app-id vehicleregistrationservice --app-port 6002 --dapr-http-port 3602 --dapr-grpc-port 60002 --config ../dapr/config/config.yaml --components-path ../dapr/components dotnet run
    ```

    >  Alternatively you can also run the `start-selfhosted.ps1` script.

1. Open a new command-shell.

1. Change the current folder to the `src/FineCollectionService` folder of this repo.

1. Execute the following command (using the Dapr cli) to run the FineCollectionService:

    ```console
    dapr run --app-id finecollectionservice --app-port 6001 --dapr-http-port 3601 --dapr-grpc-port 60001 --config ../dapr/config/config.yaml --components-path ../dapr/components dotnet run
    ```

    > Alternatively you can also run the `start-selfhosted.ps1` script.

1. Open a new command-shell.

1. Change the current folder to the `src/TrafficControlService` folder of this repo.

1. Execute the following command (using the Dapr cli) to run the TrafficControlService:

    ```console
    dapr run --app-id trafficcontrolservice --app-port 6000 --dapr-http-port 3600 --dapr-grpc-port 60000 --config ../dapr/config/config.yaml --components-path ../dapr/components dotnet run
    ```

    > Alternatively you can also run the `start-selfhosted.ps1` script.

1. Open a new command-shell.

1. Change the current folder to the `src/Simulation` folder of this repo.

1. Execute the following command to run the Camera Simulation:

     ```console
     dotnet run
     ```

You should now see logging in each of the shells, similar to the logging shown below:

**Camera Simulation:**  

![Simulation logging](img/logging-simulation.png)

**TrafficControlService:**  

![TrafficControlService loggin](img/logging-trafficcontrolservice.png)

**FineCollectionService:**  

![FineCollectionService logging](img/logging-finecollectionservice.png)

**VehicleRegistrationService:**  

![VehicleRegistrationService logging](img/logging-vehicleregistrationservice.png)

To see the emails that are sent by the FineCollectionService, open a browser and browse to [http://localhost:4000](http://localhost:4000). You should see the emails coming in:

![Mailbox](img/mailbox.png)

## Visual Camera Simulation

This repository also contains a graphical version of the Camera Simulation:

![Visual simulation](img/visualsim.png)

The cars are all driving at different speeds and the simulation uses different "personas" to simulate drivers that tend to drive faster than others. They will also try to overtake where possible. These drivers are most likely to get a speeding ticket.

The simulation runs in a web-browser. In order to start the web-application host and run the simulation, execute the following steps:

1. Open a new command-shell.

1. Change the current folder to the `src/VisualSimulation` folder of this repo.

1. Execute the following command to run the Visual Camera Simulation:

     ```console
     dotnet run
     ```

1. Open a browser window and navigate to [http://localhost:5000](http://localhost:5000).

1. Use the arrow keys to scroll and zoom in/out.

## Run the application with Dapr actors

The TrafficControlService has an alternative implementation based on Dapr actors. 

The `TrafficController` in the TrafficControlService has 2 implementations of the `VehicleEntry` and `VehicleExit` methods. The top two methods contain all the code for handling vehicle registrations and storing vehicle state using the state management building block. The bottom two methods use a `VehicleActor` that does all the work. A new instance of the `VehicleActor` is created for each registered vehicle. In stead of using the state management building block, the actor uses its built-in `StateManager `.

You can find the code of the actor in the file `src/TrafficControlService/Actors/VehicleActor.cs`.

To use the actor based implementation, uncomment the statement that defines the `USE_ACTORMODEL` symbol at the top of the controller:

```csharp
#define USE_ACTORMODEL
```

Now you can restart the application just as before. The behavior is exactly the same, but you will see that the logging of the TrafficControlService will be emitted by the `VehicleActor`:  

![](img/logging-trafficcontrolservice-actors.png)

## Run the application on Kubernetes

Execute the following steps to run the sample application on Kubernetes:

1. Make sure you have installed Dapr on your machine on a Kubernetes cluster as described in the [Dapr documentation](https://docs.dapr.io/getting-started/install-dapr/).

1. Open a new command-shell.

1. Change the current folder to the `src/k8s` folder of this repo.

1. Run the `build-docker-images.ps1` script. This script will build Docker images for all the services and a custom Mosquitto image used when running on Kubernetes.

1. Execute the `start.ps1` script. All services will be created in the `dapr-trafficcontrol` namespace.

You can check whether everything is running correctly by examining the container logs. There are several ways of doing that. Let's do it using the Docker CLI:

1. Find out the container Id of the services:

    ```console
    docker ps
    ```

  > For every service, 2 containers will be running: the service and the Dapr sidecar. Make sure you pick the Id of a container running the .NET service and not the Dapr sidecar.

1. View the log for each of the services (replace the Id with the Id of one of your services):

    ```console
    docker logs e2ed262f836e
    ```

To see the emails that are sent by the FineCollectionService, open a browser and browse to [http://localhost:30000](http://localhost:30000).

To stop the application and remove everything from the Kubernetes cluster, execute the `stop.ps1` script.

## Dapr for .NET Developers

If you want to learn more about Dapr, read this book that was co-authored by the creator of this sample application:

![Dapr for .NET Developers](img/dapr-for-net-devs-cover-thumb.png)

[Dowload the PDF](https://aka.ms/dapr-ebook)  |  [Read it online](https://docs.microsoft.com/dotnet/architecture/dapr-for-net-developers/)

Although the book is targeted at .NET developers, it covers all the concepts and generic APIs of Dapr. So it should also be useful for developers that use a different technology stack.

## Disclaimer

The code in this repo is NOT production grade and lacks any automated testing. It is intentionally kept as simple as possible (KISS). Its primary purpose is demonstrating several Dapr concepts and not being a full fledged application that can be put into production as is.

The author can in no way be held liable for damage caused directly or indirectly by using this code.
