//Matrix dot product
Math.dot = function (matrixA, matrixB) {
    var resultMatrix = [];
    var rowIndex = 0;
    var colIndex = 0;
    var cellValue = 0;
    var resultMatrixRow = [];    

    var aColumns = matrixA[0].length;
    var bColumns = matrixB[0].length;
    var aRows = matrixA.length;
    var bRows = matrixB.length;

    var aCellValue;
    var bCellValue;

    var bColIndex, aColIndex = 0;

    //if the columns of the 1st matrix don't match the columns of the second matrix then it can't be multiplied
    if (aColumns != bRows) {
        throw { Error: 1, Message: 'columns in the first matrix must match row in second matrix' };
    }

    for (rowIndex = 0; rowIndex < matrixA.length; rowIndex += 1) {
        resultMatrixRow = [];
        for (bColIndex = 0; bColIndex < bColumns; bColIndex += 1) {
            for (aColIndex = 0; aColIndex < aColumns; aColIndex += 1) {
                aCellValue = matrixA[rowIndex][aColIndex];
                bCellValue = matrixB[aColIndex][bColIndex];

                resultMatrixRow[bColIndex] = (resultMatrixRow[bColIndex] || 0) + (aCellValue * bCellValue);
            }
        }

        resultMatrix.push(resultMatrixRow);
    }
   
    return resultMatrix;
};

// Converts from degrees to radians.
Math.radians = function (degrees) {
    return degrees * Math.PI / 180;
};

// Converts from radians to degrees.
Math.degrees = function (radians) {
    return radians * 180 / Math.PI;
};

function ViewModel(options) {
    var data = options || {};

    return {
        armConfig: data.armConfig,
        endPosition: data.endPosition || { x: 0, y: 0, z: 0 }
    };
};

var jointTypes = {
    Rotary: 0,
    Prismatic: 1
};

function Joint(options) {
    var data = options || {};

    return {
        jointType: data.jointType || jointTypes.Rotary,
        homeAngle: data.homeAngle || 0,
        angle: data.angle || 0,
        maxAngle: data.maxAngle || 0,
        minAngle: data.minAngle || 0,
        offsetAngle: data.offsetAngle || 0,
        offsetLength: data.offsetLength || 0,
        name: data.name || 'base'
    };
}

function Link(options) {
    var data = options || {};

    return {
        startJoint: data.startJoint || null,
        endJoint: data.endJoint || null,
        length: data.length || 0 //from start joint axis to end joint axis
    };
}

function ArmController(viewModel) {
    var initArmConfig = function () {
        var joints = {
            0: Joint(), //base joint
            1: Joint({ //shoulder joint
                homeAngle: 0,
                maxAngle: 180,
                minAngle: 0,
                offsetAngle: 0,
                offsetLength: -1.3125,
                name: 'shoulder'
            }),
            2: Joint({ //elbow joint
                homeAngle: -90,
                maxAngle: 180,
                minAngle: 0,
                offsetAngle: 0,
                offsetLength: 0,
                name: 'elbow'
            }),
            3: Joint({
                name: 'wrist twist'
            }), //wrist twist
            4: Joint({ //wrist bend
                homeAngle: 0,
                maxAngle: 180,
                minAngle: 0,
                offsetAngle: -90,
                offsetLength: -1.3125,
                name: 'wrist bend'
            }),
            5: Joint({ //gripper
                homeAngle: 0,
                maxAngle: 180,
                minAngle: 0,
                offsetAngle: 90,
                offsetLength: -1.75,
                name: 'gripper'
            })
        };

        //Initialize the angle to home angle on startup
        for (var joint in joints) {
            joints[joint].angle = joints[joint].homeAngle;
        }

        return {
            joints: joints,
            links: [Link({
                    startJoint: joints[0],
                    endJoint: joints[1]
                }), Link({
                    startJoint: joints[1],
                    endJoint: joints[2],
                    length: -8.75
                }), Link({
                    startJoint: joints[2],
                    endJoint: joints[3],
                    length: -5.75
                }), Link({
                    startJoint: joints[3],
                    endJoint: joints[4]
                }), Link({
                    startJoint: joints[4],
                    endJoint: joints[5]
                })
            ],
            frames: []
        };
    };

    if (viewModel == null) {
        throw { Code: 1, Message: 'viewModel is required.' };
    }

    if (viewModel.armConfig == null) {
        viewModel.armConfig = initArmConfig();
    }    

    var IDENTITY_MATRIX = [[1, 0, 0, 0],
                       [0, 1, 0, 0],
                       [0, 0, 1, 0],
                       [0, 0, 0, 1]];

    var rotateFrameAlongZ = function (startFrameMatrix, angle) {
        var rotateMatrix = [[Math.cos(angle), -Math.sin(angle), 0, 0],
                            [Math.sin(angle), Math.cos(angle), 0, 0],
                            [0, 0, 1, 0],
                            [0, 0, 0, 1]];

        return Math.dot(rotateMatrix, startFrameMatrix);
    };

    var rotateFrameAlongX = function (startFrameMatrix, angle) {
        var rotateMatrix = [[1, 0, 0, 0],
                            [0, Math.cos(angle), -Math.sin(angle), 0],
                            [0, Math.sin(angle), Math.cos(angle), 0],
                            [0, 0, 0, 1]];

        return Math.dot(rotateMatrix, startFrameMatrix);
    };

    var translateFrameAlongZ = function (startFrameMatrix, distance) {
        var translationMatrix = [[1, 0, 0, 0],
                                 [0, 1, 0, 0],
                                 [0, 0, 1, distance],
                                 [0, 0, 0, 1]];

        return Math.dot(translationMatrix, startFrameMatrix);
    };

    var translateFrameAlongX = function (startFrameMatrix, distance) {
        var translationMatrix = [[1, 0, 0, distance],
                                 [0, 1, 0, 0],
                                 [0, 0, 1, 0],
                                 [0, 0, 0, 1]];

        return Math.dot(translationMatrix, startFrameMatrix);
    };

    var initFrames = function () {
        /*
        ai   - length of link i
        si   - twist of link i
        di   - link offset of link i (prismatic variable)
        0i   - joint angle of joint i (revolute variable)

        ai   - Angle between Zi and Zi-1 along xi axis
        Di   - Offset Distance between joints along Z axis
        ri   - Distance between joints along Xi axis
        ϴi   - Rotation of the joint along Zi axis
        */
        var frames = [];
        var frameIndex = 0;
        var di = 0;
        var ai = 0;
        var ri = 0;
        var ϴi = 0;
        var linkIndex = 0;
        var link = null;


        //var translateAlongZ = null;

        //var rotateAlongZ = null;

        //var translateAlongX = null;

        //var rotateAlongX = null

        //for (linkIndex = 0; linkIndex < viewModel.armConfig.links.length; linkIndex += 1) {
        //for (linkIndex in viewModel.armConfig.links) {
        //    link = viewModel.armConfig.links[linkIndex];

        //    ai = Math.radians(link.endJoint.offsetAngle);
        //    Di = link.endJoint.offsetLength;
        //    ri = link.length;
        //    ϴi = Math.radians(link.endJoint.angle);

        //    //translateAlongZ = [[1, 0, 0, 0],
        //    //                   [0, 1, 0, 0],
        //    //                   [0, 0, 1, di],
        //    //                   [0, 0, 0, 1]];

        //    //rotateAlongZ = [[Math.cos(ϴi), -Math.sin(ϴi), 0, 0],
        //    //                [Math.sin(ϴi), Math.cos(ϴi), 0, 0],
        //    //                [0, 0, 1, 0],
        //    //                [0, 0, 0, 1]];

        //    //translateAlongX = [[1, 0, 0, ai],
        //    //                   [0, 1, 0, 0],
        //    //                   [0, 0, 1, 0],
        //    //                   [0, 0, 0, 1]];

        //    //rotateAlongX = [[1, 0, 0, 0],
        //    //                [0, Math.cos(ai), -Math.sin(ai), 0],
        //    //                [0, Math.sin(ai), Math.cos(ai), 0],
        //    //                [0, 0, 0, 1]];

        //    var matrixAfterTranslateAlongZ = Math.dot(IDENTITY_MATRIX, translateAlongZ);
        //    var matrixAfterRotateAlongZ = Math.dot(matrixAfterTranslateAlongZ, rotateAlongZ);
        //    var matrixAfterTranslateAlongX = Math.dot(matrixAfterRotateAlongZ, translateAlongX);

        //    frames.push(Math.dot(matrixAfterTranslateAlongX, rotateAlongX));
        //}

        //viewModel.armConfig.frames = frames;
    };

    //Inverse Kinematics
    var calculateAngles = function () {

    };

    //Forward kinematics
    var calculateEndPoint = function () {
        var endPoint = viewModel.endPosition;
        var calculatedMatrix = [];
        var frameIndex = 0;
        var linkIndex;
        var link = null;
        var di = 0;
        var ai = 0;
        var ri = 0;
        var ϴi = 0;

        //calculatedMatrix = IDENTITY_MATRIX;
        calculatedMatrix = [[endPoint.x], [endPoint.y], [endPoint.z], [1]];

        //viewModel.armConfig.links.length
        for (linkIndex = 0; linkIndex < 2; linkIndex += 1) {
            link = viewModel.armConfig.links[linkIndex];

            ai = Math.radians(link.endJoint.offsetAngle);
            Di = link.endJoint.offsetLength;
            ri = link.length;
            ϴi = Math.radians(link.endJoint.angle);

            calculatedMatrix = translateFrameAlongZ(calculatedMatrix, Di);
            calculatedMatrix = rotateFrameAlongZ(calculatedMatrix, ϴi);
            calculatedMatrix = translateFrameAlongX(calculatedMatrix, ri);
            calculatedMatrix = rotateFrameAlongX(calculatedMatrix, ai);
        }

        endPoint.x = calculatedMatrix[0][0];
        endPoint.y = calculatedMatrix[1][0];
        endPoint.z = calculatedMatrix[2][0];

        return endPoint;
    };

    initFrames();

    return (function () {
        var endPoint = calculateEndPoint();

        viewModel.endPosition.x = endPoint.x;
        viewModel.endPosition.y = endPoint.y;
        viewModel.endPosition.z = endPoint.z;

        return {
            onChange: function (viewModel) {
                //this should be overriden                
            },
            MoveLeft: function (step) {
                console.log('Left: ' + step);
                viewModel.endPosition.x -= 1;
                calculateAngles();
                this.onChange(viewModel);
            },
            MoveRight: function (step) {
                console.log('Right: ' + step);
                viewModel.endPosition.x += 1;
                calculateAngles();
                this.onChange(viewModel);
            },
            MoveUp: function (step) {
                console.log('Up: ' + step);
                viewModel.endPosition.z += 1;
                calculateAngles();
                this.onChange(viewModel);
            },
            MoveDown: function (step) {
                console.log('Down: ' + step);
                viewModel.endPosition.z -= 1;
                calculateAngles();
                this.onChange(viewModel);
            },
            MoveForward: function (step) {
                console.log('Forward: ' + step);
                viewModel.endPosition.y += 1;
                calculateAngles();
                this.onChange(viewModel);
            },
            MoveBackward: function (step) {
                console.log('Backward: ' + step);
                viewModel.endPosition.y -= 1;
                calculateAngles();
                this.onChange(viewModel);
            },
            OpenClaw: function (step) {
                console.log('Open: ' + step);
                calculateAngles();
                this.onChange(viewModel);
            },
            CloseClaw: function (step) {
                console.log('Close: ' + step);
                calculateAngles();
                this.onChange(viewModel);
            },
            RotateClaw: function (step) {
                calculateAngles();
                this.onChange(viewModel);
            }
        }
    }());
}

function ArmJoystick(options) {
    var data = options || {};

    if (options.controller == null) {
        throw { Code: 1, Message: 'controller is required.' };
    }

    return {
        MoveLeft: function (step) {
            options.controller.MoveLeft(data.step);
        },
        MoveRight: function (step) {
            options.controller.MoveRight(data.step);
        },
        MoveUp: function (step) {
            options.controller.MoveUp(data.step);
        },
        MoveDown: function (step) {
            options.controller.MoveDown(data.step);
        },
        MoveForward: function (step) {
            options.controller.MoveForward(data.step);
        },
        MoveBackward: function (step) {
            options.controller.MoveBackward(data.step);
        }
    };
}

function ClawJoystick(options) {
    var data = options || {};

    if (options.controller == null) {
        throw { Code: 1, Message: 'controller is required.' };
    }

    return {
        OpenClaw: function (step) {
            options.controller.OpenClaw(step);
        },
        CloseClaw: function (step) {
            options.controller.CloseClaw(step);
        },
        RotateClaw: function (step) {
            options.controller.RotateClaw(step);
        }
    };
}
