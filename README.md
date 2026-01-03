project:
  title: "CSCI 253 Term Project — Unity XR Environment"
  course: "CSCI 253 — Human-Computer Interaction"
  author: "Ritvik"
  engine: "Unity"
  unity_version: "6000.0.61f1"
  frameworks:
    - "XR Interaction Toolkit"

overview:
  description: >
    This project is a Unity-based virtual reality (VR) environment developed
    as a term project for CSCI 253 — Human-Computer Interaction.
    The project provides a shared, modular VR environment designed to support
    experimentation and evaluation of multiple HCI interaction techniques.
    The environment acts as a common testbed where different interaction
    hypotheses such as object manipulation, navigation, and feedback systems
    can be implemented and tested consistently.

hci_focus:
  goals:
    - "Design intuitive 3D object manipulation techniques"
    - "Provide clear and immediate user feedback"
    - "Support modular experimentation of interaction techniques"
    - "Maintain consistent interaction behavior across scenes"
    - "Separate environment logic from interaction logic"

features:
  - "XR Interaction Toolkit–based VR setup"
  - "Modular additive scene architecture"
  - "Bootstrap scene for initialization"
  - "Persistent Systems scene containing XR Rig and managers"
  - "Shared Environment scene for world geometry"
  - "Additive feature scenes for HCI experimentation"
  - "Custom object manipulation system"
  - "Global grab and interaction event handling"
  - "Event-driven feedback system"
  - "Haptic feedback integration"

project_structure:
  Assets:
    - "Scenes/"
    - "Scripts/"
    - "Prefabs/"
    - "Art/"
    - "XR/"
  Other:
    - "Packages/"
    - "ProjectSettings/"
    - "README.yaml"

setup:
  open_project:
    steps:
      - "Clone the repository from GitHub"
      - "Open Unity Hub"
      - "Add the cloned project directory"
      - "Open the project using the correct Unity version"

run_in_editor:
  steps:
    - "Open the Bootstrap scene"
    - "Press Play in the Unity Editor"
    - "Bootstrap loads Systems, Environment, and Feature scenes additively"
    - "Interact using VR controllers"

controls:
  interaction_model: "XR Interaction Toolkit default bindings"
  actions:
    grab: "Controller grip / select"
    interact: "Trigger"
    move: "Thumbstick (if enabled)"
    look: "Head movement"

build:
  target: "PC VR"
  steps:
    - "Open File > Build Settings"
    - "Select target VR platform"
    - "Ensure Bootstrap scene is first in Scenes In Build"
    - "Build the project"
    - "Run executable with VR headset connected"

assignment_context:
  description: >
    This project was developed as part of a team-based HCI assignment.
    Each team member implemented and evaluated different interaction
    techniques within a shared VR environment.
    This repository represents the environment and system infrastructure
    used to support those experiments.

limitations:
  - "Designed primarily for desktop VR"
  - "Feature scenes may require manual enabling or disabling"
  - "Performance depends on headset and hardware configuration"

license:
  usage: "Educational use only"
