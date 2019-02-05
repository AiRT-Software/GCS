# AIRT Project

This file explains how to build the Ground Control System binaries.

Hardware
--------
This system has been developed for running on an Android tablet. However, since it has been built with Unity, it can be compiled for virtually any platform, including iOS, Windows, Linux, etc.

Prerequisites
-------------
All the required libraries are included in the package.


For building the binaries
------------------------------------
Before compiling, take into account:

The client contains the binary of the server (the server can be updated from a more recent client without requiring Internet connection). Follow the instructions on how to build the OCS to generate the .deb package. Rename the generated .deb package to airt-project.bytes and move it into jiminy/airt_unity/Assets/Resources/Update

Open the project with Unity and compile to the platform of your choice.
