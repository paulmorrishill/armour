#Armour
##Open source robotic arm control software.
This project was designed to be a replacement for the useless software provided with the Dobot robotic arm, but should work well with any robotic arm that can be modelled using Denavit-Hartenberg parameters.
This is in a very, very early stage, and is only a console app at the moment, allowing control of the arm through the keyboard.

#Firmware
The current implementation uses G-Codes as the protocol for arm control and assumes that your arm is set up for one degree per mm.
I use the repetier firmware which works pretty well but any 3D printer firmware should work.

#Host software
Obviously this software is the host software but it's not the greatest tool to use to test your connections and pin assignments. For that I use the [repetier host](http://www.repetier.com/download-now/).

#Dobot
The dobot is configured with the three axes laid out in the following arangement.

![ramps wiring](documentation/axis-labeling-with-angles.png)

##Wiring the Dobot
I'd recommend using the Ramps 1.4 driver board as it provides outputs for all functions relating to the arm.

###Ramps
Here is a picture of the Ramps board wired up to the Dobot outputs.

![ramps wiring](documentation/ramps-wiring.jpg)

To wire up the arm start by putting 3 jumpers across all 3 microstepping switches beneath each controller - this will set controllers to 16 times microstepping.

Then put the A4988 stepper drivers into the slots shown in the image.

The axis connected are actually XY & extruder 1. This is due to the double row of pins on the Z connector, which means you can't connect the arm's Z stepper cable directly.

###Endstops
For some reason the people who made the Dobot decided not to implement any kind of end stop on the arm. I assume they intended to use the 2 axis angle sensors that are mounted on each segment. I couldn't find any code that would allow me to hook into these sensors so I opted for the simple route and put some physical end stops in.

![ramps wiring](documentation/end-stop-blocks.jpg)

The switch mounting block can be 3D printed from the stl file in dobot/HomeSwitchBlock.stl

##Configuring the repetier firmware
- Download the [Arduino](https://www.arduino.cc/) IDE if you don't already have it.
- Download the latest version of the [repetier firmware](http://www.repetier.com/documentation/repetier-firmware/).
- Open Repetier/Repetier.ino in Arduino
- Open configuration.h and set the following values

```

    #define MOTHERBOARD 33

    //I believe that these are the steps per degree on each of the motors based on
    //1:10 gear ratio on steppers
    //16x microstepping
    //200 steps per revolution
    //And assuming that 1 degree of rotation equals 1 degree of arm rotation
    #define XAXIS_STEPS_PER_MM 88.8888
    #define YAXIS_STEPS_PER_MM 88.8888
    #define ZAXIS_STEPS_PER_MM 88.8888

    //Stop complaints due to missing temperature sensors
    #define MIN_DEFECT_TEMPERATURE -1000
    #define MAX_DEFECT_TEMPERATURE 2900

    //Set up the axis to be aligned with the DH parameters
    #define X_MIN_POS 0
    #define Y_MIN_POS 0
     //Note that this depends on your end stops and the angle at which your arm's Z axis is at home
    #define Z_MIN_POS -63

    //Invert directions to correspond with the angles required for
    #define INVERT_X_DIR 0
    #define INVERT_Y_DIR 0
    #define INVERT_Z_DIR 1
    #define X_HOME_DIR -1
    #define Y_HOME_DIR 1
    #define Z_HOME_DIR -1

    //Servos
    #define FEATURE_SERVO 1
    #define SERVO0_PIN 11
    #define SERVO1_PIN 6
    #define SERVO2_PIN -1
    #define SERVO3_PIN -1
    #define SERVO0_NEUTRAL_POS  1400
    #define SERVO1_NEUTRAL_POS  1400
    #define SERVO2_NEUTRAL_POS  -1
    #define SERVO3_NEUTRAL_POS  -1

    //Improve movement precision
    #define DELTA_SEGMENTS_PER_SECOND_PRINT 1000
    #define DELTA_SEGMENTS_PER_SECOND_MOVE 1000

    #define ENDSTOP_X_BACK_MOVE 5
    #define ENDSTOP_Y_BACK_MOVE 5
    #define ENDSTOP_Z_BACK_MOVE 2
    #define ENDSTOP_X_BACK_ON_HOME 1
    #define ENDSTOP_Y_BACK_ON_HOME 1
    #define ENDSTOP_Z_BACK_ON_HOME 0

    #define MAX_FEEDRATE_X 1000
    #define MAX_FEEDRATE_Y 2500
    #define MAX_FEEDRATE_Z 2500
    #define HOMING_FEEDRATE_X 20
    #define HOMING_FEEDRATE_Y 20
    #define HOMING_FEEDRATE_Z 20
    #define HOMING_ORDER HOME_ORDER_XYZ
    #define RAMP_ACCELERATION 1

    #define MAX_ACCELERATION_UNITS_PER_SQ_SECOND_X 100
    #define MAX_ACCELERATION_UNITS_PER_SQ_SECOND_Y 100
    #define MAX_ACCELERATION_UNITS_PER_SQ_SECOND_Z 100
    #define MAX_TRAVEL_ACCELERATION_UNITS_PER_SQ_SECOND_X 100
    #define MAX_TRAVEL_ACCELERATION_UNITS_PER_SQ_SECOND_Y 100
    #define MAX_TRAVEL_ACCELERATION_UNITS_PER_SQ_SECOND_Z 100

    #define MAX_JERK 0
    #define MAX_ZJERK 0

    // ################ Endstop configuration (if you have endstops) #####################

    #define ENDSTOP_PULLUP_X_MIN true
    #define ENDSTOP_X_MIN_INVERTING true
    #define MIN_HARDWARE_ENDSTOP_X true
    #define ENDSTOP_PULLUP_Y_MIN true
    #define ENDSTOP_Y_MIN_INVERTING true
    #define MIN_HARDWARE_ENDSTOP_Y true
    #define ENDSTOP_PULLUP_Z_MIN true
    #define ENDSTOP_Z_MIN_INVERTING true
    #define MIN_HARDWARE_ENDSTOP_Z true
    #define ENDSTOP_PULLUP_X_MAX true
    #define ENDSTOP_X_MAX_INVERTING false
    #define MAX_HARDWARE_ENDSTOP_X false
    #define ENDSTOP_PULLUP_Y_MAX true
    #define ENDSTOP_Y_MAX_INVERTING false
    #define MAX_HARDWARE_ENDSTOP_Y false
    #define ENDSTOP_PULLUP_Z_MAX true
    #define ENDSTOP_Z_MAX_INVERTING false
    #define MAX_HARDWARE_ENDSTOP_Z false
    #define max_software_endstop_r true
```

- Open pins.h, find the constants below **in motherboard 33** and overwrite

```

    #define ORIG_Z_STEP_PIN         26
    #define ORIG_Z_DIR_PIN          28
    #define ORIG_Z_ENABLE_PIN       24
    #define ORIG_Z_MIN_PIN          18
    #define ORIG_Z_MAX_PIN          19

    #define ORIG_E0_STEP_PIN         49
    #define ORIG_E0_DIR_PIN          49
    #define ORIG_E0_ENABLE_PIN       49

```

- Compile and program the arduino mega
- Open Repetier host and confirm that all the arm functions are working. Servos can be adjusted using [M340](http://reprap.org/wiki/Gcode#M340:_Control_the_servos)

# Development
To run this project you'll need visual studio.
Where possible write with TDD.

## Forward kinematics
## Inverse kinematics