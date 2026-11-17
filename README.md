# ASRS VR Digital Twin

A Unity-based Automated Storage and Retrieval System (AS/RS) simulation. The full project has been tested with a Meta Quest 3 connected by cable through Meta Horizon Link. We also use MetaXRSimulator during development.

## Features

- Two AS/RS crane systems with manual and automatic operation
- Conveyor routes that move boxes to arrival points
- Stacking zones for incoming boxes
- Rack slot tracking and automatic box placement
- Status displays for operating mode and rack availability
- Desktop testing with keyboard and mouse; VR interaction with Quest controllers in the full project

## Code structure

```text
Assets/
├── ASRS/
│   └── ASRS.cs
├── ASRSStatusUI.cs
├── BoxArrivalHandler.cs
├── BoxMover.cs
├── CameraMove.cs
├── ConveyorSystem.cs
├── RackManager.cs
└── StackingZone.cs
```

## About this repository

This is a **code-only showcase** of the Unity project, developed with Unity 2022.3.62f3. The Unity scene, 3D models, project configuration, and Quest controller setup are not included. Cloning this repository alone will not run the complete simulation or VR experience.

The published scripts implement simulation behaviour. Live PLC, sensor, or warehouse management system data integration is not included in this release.
