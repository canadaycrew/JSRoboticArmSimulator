using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboticArmTableCore
{
    public enum JointOrientation
    {
        Bend,
        Rotate
    }

    public interface IArm
    {
        IJoint[] Joints { get; set; }
        ILinkage[] Linkages { get; set; }
    }

    public class CustomConverter<T> : JsonConverter
        {           
            public override bool CanConvert(System.Type objectType)
            {
                return true;
            }

            public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
            {
                return serializer.Deserialize<T>(reader);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }
        }

    public class JasonTypeConverter<T> : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return false;
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal = new object();
            if (reader.TokenType == JsonToken.StartObject)
            {
                T instance = (T)serializer.Deserialize(reader, typeof(T));
                retVal = instance;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                retVal = serializer.Deserialize<T>(reader);
            }
            return retVal;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class ArmBase : IArm
    {
        //[JsonConverter(typeof(CustomConverter<JointBase[]>))]
        [JsonConverter(typeof(JasonTypeConverter<JointBase[]>))]
        public IJoint[] Joints { get; set; }

        //[JsonConverter(typeof(CustomConverter<LinkageBase[]>))]
        [JsonConverter(typeof(JasonTypeConverter<LinkageBase[]>))]
        public ILinkage[] Linkages { get; set; }

        public ArmBase()
        {
            Joints = new IJoint[] {
                    new JointBase() { Name = "ShoulderBase", IsBaseJoint = true, Orientation = JointOrientation.Rotate, CurrentRotationDegree = 0, MaximumRotationDegree = 360 },
                    new JointBase() { Name = "ShoulderRotate", Orientation = JointOrientation.Rotate, CurrentRotationDegree = 0, MinimumRotationDegree = 0, MaximumRotationDegree = 180 },
                    new JointBase() { Name = "ShoulderUpDown", Orientation = JointOrientation.Bend, CurrentRotationDegree = 90, MinimumRotationDegree = 0,  MaximumRotationDegree = 180 },
                    new JointBase() { Name = "Elbow", Orientation = JointOrientation.Bend, CurrentRotationDegree = 90, MinimumRotationDegree = -45, MaximumRotationDegree = 225 },
                    new JointBase() { Name = "WristBend", Orientation = JointOrientation.Bend, CurrentRotationDegree = 90, MinimumRotationDegree = -45, MaximumRotationDegree = 225 },
                    new JointBase() { Name = "WristRotate", Orientation = JointOrientation.Rotate, CurrentRotationDegree = 0, MinimumRotationDegree = 0, MaximumRotationDegree = 360 },
                };

            Linkages = new ILinkage[] {
                    new LinkageBase(Joints[0], Joints[1]) { Length = 0, Name = "ShoulderBaseToUpDown" }, //ShoulderBase to ShoulderUpDown
                    new LinkageBase(Joints[1], Joints[2]) { Length = 0, Name = "ShoulderUpDownToRotate" }, //ShoulderUpDate to ShoulderRotate
                    new LinkageBase(Joints[2], Joints[3]) { Length = 152.4, Name = "Bicep" }, //ShoulderRotate to Elbow
                    new LinkageBase(Joints[3], Joints[4]) { Length = 76.2, Name = "Forearm" }, //Elbow To Wrist
                    new LinkageBase(Joints[4], Joints[5]) { Length = 25, Name = "WristToWristRotate" }  //Wrist to Wrist Rotate
                };
        }
    }

    public interface IJoint
    {
        event EventHandler Turned;

        short CurrentRotationDegree { get; set; }
        JointOrientation Orientation { get; set; }
        string Name { get; set; }
        void Turn(short degrees);
        bool IsBaseJoint { get; set; }
        short MaximumRotationDegree { get; set; }
        short MinimumRotationDegree { get; set; }
        short HomeDegree { get; set; }
        int Index { get; set; }
    }

    public interface ILinkage
    {
        string Name { get; set; }
        double Length { get; set; } //Length in inches
        double Radius { get; set; } //Radius in inches
        double Width { get; set; } //Width in inches
        IJoint StartJoint { get; set; }
        IJoint EndJoint { get; set; }
        double Reach { get; set; }
    }

    public class LinkageBase : ILinkage
    {
        public string Name { get; set; }
        public double Length { get; set; }
        public double Radius { get; set; }
        public double Width { get; set; }
        public double Reach { get; set; }

        [JsonConverter(typeof(JasonTypeConverter<JointBase>))]
        public IJoint StartJoint { get; set; }

        [JsonConverter(typeof(JasonTypeConverter<JointBase>))]
        public IJoint EndJoint { get; set; }

        public LinkageBase() { }

        public LinkageBase(IJoint startJoint, IJoint endJoint)
        {
            StartJoint = startJoint;
            EndJoint = endJoint;
        }
    }

    public class JointBase : IJoint
    {
        public event EventHandler Turned;

        public JointBase()
        {
            Orientation = JointOrientation.Bend;
        }

        public JointBase(IJoint joint)
        {
            // TODO: Complete member initialization
            this.CurrentRotationDegree = joint.CurrentRotationDegree;
            this.Name = joint.Name;
            this.MaximumRotationDegree = joint.MaximumRotationDegree;
            this.IsBaseJoint = joint.IsBaseJoint;
            this.Orientation = joint.Orientation;
            this.Index = joint.Index;
        }

        public short CurrentRotationDegree { get; set; }
        public string Name { get; set; }
        public short MaximumRotationDegree { get; set; }
        public short MinimumRotationDegree { get; set; }
        public bool IsBaseJoint { get; set; }
        public JointOrientation Orientation { get; set; }
        public int Index { get; set; }
        public short HomeDegree { get; set; }

        public void Turn(short degrees)
        {
            if (Turned != null)
            {
                Turned(this, EventArgs.Empty);
            }
        }
    }

    public class ObjectDetails
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }
    }

    public class JointInformation
    {
        public JointInformation()
        {
            Joints = new List<IJoint>();
        }

        public List<IJoint> Joints { get; set; }
    }

    public class ArmController
    {
        public event EventHandler ArmInitialized;
        public event EventHandler ArmMoved;

        public ObjectDetails GraspedObject { get; set; }

        public IJoint[] Joints { get; set; }
        public ILinkage[] Linkages { get; set; }
        public IJoint[] OrderedJoints { get; private set; }
        public ILinkage[] OrderedLinkage { get; private set; }

        public ArmController(IArm arm)
        {
            if (arm == null)
            {
                arm = new ArmBase();
            }

            Joints = arm.Joints;
            Linkages = arm.Linkages;
        }

        public void InitializeArm()
        {
            //Order the linkages by joint
            IJoint baseJoint = (from j in Joints
                                where j.IsBaseJoint == true
                                select j).FirstOrDefault();

            if (baseJoint == null)
            {
                throw new ArgumentNullException("You must specify one of the joints as the base joint");
            }

            IJoint[] orderedJoints = new IJoint[Joints.Length];
            ILinkage[] orderedLinkage = new ILinkage[Linkages.Length];

            ILinkage firstLink = (from l in Linkages
                                  where l.StartJoint == baseJoint
                                  select l).FirstOrDefault();

            if (firstLink == null)
            {
                throw new ArgumentNullException("You must have a linkage connected to the base joint.");
            }

            ILinkage currentLinkage = firstLink;
            IJoint currentJoint = baseJoint;
            byte jointIndex = 1;
            byte linkIndex = 1;

            orderedJoints[0] = baseJoint;
            orderedLinkage[0] = currentLinkage;

            HashSet<ILinkage> processedLinkages = new HashSet<ILinkage>();

            while (currentLinkage != null)
            {
                processedLinkages.Add(currentLinkage);

                orderedJoints[jointIndex] = currentLinkage.EndJoint;
                currentLinkage.EndJoint.Index = jointIndex;

                jointIndex++;

                currentLinkage = (from l in Linkages
                                  where (l.StartJoint == currentLinkage.EndJoint
                                  || l.EndJoint == currentLinkage.StartJoint)
                                  && !processedLinkages.Contains(l)
                                  select l).FirstOrDefault();

                if (currentLinkage != null)
                {
                    orderedLinkage[linkIndex] = currentLinkage;
                    linkIndex++;
                }
            }

            OrderedJoints = orderedJoints;
            OrderedLinkage = orderedLinkage;

            //CalculateXYMatrix();

            OnArmInitialized();
        }

        protected void OnArmMoved()
        {
            if (ArmMoved != null)
            {
                ArmMoved(this, EventArgs.Empty);
            }
        }

        protected void OnArmInitialized()
        {
            if (ArmInitialized != null)
            {
                ArmInitialized(this, EventArgs.Empty);
            }
        }

        public class Point
        {
            public Point()
            {
            }

            public Point(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }

            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }

        /// <summary>
        /// This moves the object the arm is manipulating to a specific X,Y,Z coordinate
        /// </summary>
        /// <param name="point"></param>
        public void MoveToPoint(Point point)
        {

        }

        /// <summary>
        /// This determines the X,Y,Z coordinate of the object the arm is manipulating
        /// </summary>
        /// <returns></returns>
        private Point DeterminePiontOfObject()
        {
            Point returnValue = new Point();



            return returnValue;
        }

        private void CalculateXYMatrix()
        {

            //start at zero degrees for all joints
            foreach (IJoint joint in OrderedJoints)
            {
                joint.CurrentRotationDegree = 0;
            }

            //Dictionary<double, JointInformation> xyJointInformation = new Dictionary<double, JointInformation>();
            //Dictionary<IJoint, double[][]> xyMatrix = new Dictionary<IJoint, double[][]>();

            int xyPermutations = 1;

            //[x,y,jointindex] holds angle of joint
            Dictionary<string, double> xyMatrix = new Dictionary<string, double>();
            //double[,,] xyMatrix = new double[5000,5000,5];

            short angle = 0;

            IJoint baseBendingJoint = (from j in OrderedJoints
                                       where j.Orientation == JointOrientation.Bend
                                       select j).First();
            IJoint baseJoint = OrderedJoints[0];

            short minimumAngle = (from j in OrderedJoints
                                select j.MinimumRotationDegree).Min();
            short maximumAngle = (from j in OrderedJoints
                                select j.MaximumRotationDegree).Min();

            for (int calculatingJointIndex = 0; calculatingJointIndex < OrderedJoints.Length; calculatingJointIndex++)
            {
                for (int jointX = 0; jointX < OrderedJoints.Length; jointX++)
                {
                    if (jointX != calculatingJointIndex)
                    {
                        IJoint joint = OrderedJoints[jointX];

                        for (angle = minimumAngle; angle <= maximumAngle; angle++)
                        {
                            double x = 0;
                            double y = 0;
                            double z = 0;

                            JointInformation xyJointInfo = new JointInformation();

                            if (angle >= joint.MinimumRotationDegree && angle <= joint.MaximumRotationDegree)
                            {
                                //baseBendingJoint.CurrentRotationDegree = angle;
                                joint.CurrentRotationDegree = angle;

                                for (int linkageIndex = 0; linkageIndex < OrderedLinkage.Length; linkageIndex++)
                                {
                                    ILinkage linkage = OrderedLinkage[linkageIndex];

                                    //Sin/cos/tan value
                                    //double functionValue = 1;
                                    double linkX = 0;
                                    double linkY = 0;
                                    double linkZ = 0;

                                    linkX = Math.Cos(linkage.StartJoint.CurrentRotationDegree / (180 / Math.PI)) * linkage.Length;
                                    linkY = Math.Sin(linkage.StartJoint.CurrentRotationDegree / (180 / Math.PI)) * linkage.Length;

                                    if (linkage.StartJoint.Orientation == JointOrientation.Rotate)
                                    {
                                        linkZ = Math.Tan(linkage.StartJoint.CurrentRotationDegree / (180 / Math.PI)) * linkage.Length;
                                    }

                                    x += linkX;
                                    y += linkY;
                                    z += linkZ;

                                    try
                                    {
                                        xyMatrix[x.ToString() + "-" + y.ToString() + "-" + z.ToString() + "-" + linkage.StartJoint.Index.ToString()] = angle;
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return;
        }

        public void MoveLeft()
        {

        }

        public void MoveRight()
        {

        }

        public void MoveUp()
        {

        }

        public void MoveDown()
        {

        }
    }
}
