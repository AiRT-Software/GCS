# AIRT software

AIRT software is a system created to design and execute complex shots of video and photography indoors, using a drone. The system allows, through a tablet, to perform the entire workflow: from the initial capture of the 3D environment for the design of the flight plan, to the final execution of the recording flight.

The software has a client-server design. The server (OCS) runs on the drone's onboard computer, and is responsible for communications with the flight control system, the 3D capture camera, the recording camera and with the gimbal. In addition,
it also handles communications with the client (GCS). The client runs on a tablet, and provides the user with all the tools to
design the flight plan, visualize the status of the drone at all times and control the different flights that will be carried out in a work session.

The client module offers users a complete environment for the design and execution of flight plans, using a
tablet. Its main features are:
* System configuration. All the necessary calibration and configuration processes (for example, of the positioning system) are carried out through the same client.
* Capture the scene in real time. At the same time that the drone is performing the initial flight, the user can see the status of the capture of the clouds of points in the environment. In this way, you can decide in real time which areas need more capture time, due to its complexity or other factors, such as the angle of view from the drone.
* Design of flight plans. Once the drone has captured the environment as a point cloud, the user can visualize the result in real time in a 3D environment, by which it can be moved virtually. A flight plan consists of a series of points in the scene through which the drone should pass, and the configuration of the recording camera at said waypoints (orientation and recording parameters). Once a flight plan has been designed, the client can display a preview on the same tablet, simulating the actual flight.
* Control of the drone in real time. When the drone is active, the client receives information in real time about its status, and allows the user to control the functions available from the same tablet.

User Guide available in Zenodo https://zenodo.org/record/1258206

This project has received funding from the European Union's Horizon 2020 research and innovation programme under grant agreement No 732433.
