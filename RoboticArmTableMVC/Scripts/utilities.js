var JointOrientation = {
    Bend: 0,
    Rotate: 1,
    Gripper: 2
};

var ArmMode = {
    RotateBase: 0,
    MoveObject: 1,
    GrabObject: 2,
    MoveBase: 3,
    BendShoulder: 4,
    BendElbow: 5,
    RotateWrist: 6,
    BendWrist: 7
};

var utilities = new function () {
    var self = this;  

    this.calculateSize = function (sizeInches) {
        var factor = 96 / 25.4;

        return (sizeInches * factor);
    };

    this.getRadians = function (degrees) {
        return degrees * (Math.PI / 180);
    };

    this.getDegrees = function (radians) {
        return radians * (180 / Math.PI);
    };
    
};