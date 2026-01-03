# CSCI 253 Term Project — Unity XR Environment

**Course:** CSCI 253 — Human-Computer Interaction  
**Author:** Ritvik  
**Engine:** Unity  
**Unity Version:** 6000.0.61f1 LTS  
**Frameworks:** XR Interaction Toolkit  

---

## Overview

This project is a **Unity-based virtual reality (VR) environment** developed as a term project for **CSCI 253 — Human-Computer Interaction**. The project provides a **shared, modular VR environment** designed to support experimentation and evaluation of multiple HCI interaction techniques.

The environment acts as a **common testbed** where different interaction hypotheses—such as object manipulation, navigation, and feedback systems—can be implemented and tested consistently.

---

## HCI Focus

### Design Goals
- Design intuitive **3D object manipulation** techniques  
- Provide **clear and immediate user feedback**  
- Support **modular experimentation** of interaction techniques  
- Maintain consistent interaction behavior across scenes  
- Separate **environment logic** from **interaction logic**

---

## Key Features

- XR Interaction Toolkit–based VR setup  
- Modular **additive scene architecture**  
- Bootstrap scene for initialization  
- Persistent **Systems scene** containing XR Rig and managers  
- Shared **Environment scene** for world geometry  
- Additive **Feature scenes** for HCI experimentation  
- Custom object manipulation system  
- Global grab and interaction event handling  
- Event-driven feedback system  
- Haptic feedback integration  

---

## Setup

### Opening the Project
1. Clone the repository from GitHub.
2. Open **Unity Hub**.
3. Click **Add Project**.
4. Select the cloned project directory.
5. Open the project using the correct Unity version.

---

## Running the Project (Editor)

1. Open the **Bootstrap** scene.
2. Press **Play** in the Unity Editor.
3. The Bootstrap scene loads the Systems, Environment, and Feature scenes additively.
4. Interact using VR controllers.

---

## Controls & Interaction

**Interaction Model:** XR Interaction Toolkit default bindings

- **Grab:** Controller grip / select  
- **Interact:** Trigger  
- **Move:** Thumbstick (if enabled)  
- **Look:** Head movement  

---

## Build Instructions

**Target Platform:** PC VR

1. Open **File → Build Settings**.
2. Select the target VR platform.
3. Ensure the **Bootstrap** scene is first in *Scenes In Build*.
4. Build the project.
5. Run the executable with a VR headset connected.

---

## Project Context

This project was developed as part of a **team-based HCI assignment**.  
Each team member implemented and evaluated different interaction techniques within a shared VR environment.

This repository represents the **environment and system infrastructure** used to support those experiments.

---

## Limitations

- Designed primarily for desktop VR  
- Feature scenes may require manual enabling or disabling  
- Performance depends on headset and hardware configuration  

---

## License

Educational use only.
