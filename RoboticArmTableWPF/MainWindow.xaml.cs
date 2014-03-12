using RoboticArmTableCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoboticArmTableWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ArmCanvas.SizeChanged += ArmCanvas_SizeChanged;
        }

        void ArmCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawCube();
            DrawArm();
        }

        private ArmController _controller;
        private double _scale;

        private void InitializeArm_Click(object sender, RoutedEventArgs e)
        {
            /*
            IJoint[] joints = null;
            = new IJoint[] {
                    new JointBase() { Name = "ShoulderBase", IsBaseJoint = true },
                    new JointBase() { Name = "ShoulderUpDown" },
                    new JointBase() { Name = "ShoulderRotate" },
                    new JointBase() { Name = "Elbow" },
                    new JointBase() { Name = "WristBend" },
                    new JointBase() { Name = "WristRotate" },
                };
            */

            /*ILinkage[] linkages = null;
            = new ILinkage[] {
                    new LinkageBase(joints[0], joints[1]) { Length = 0 }, //ShoulderBase to ShoulderUpDown
                    new LinkageBase(joints[1], joints[2]) { Length = 0 }, //ShoulderUpDate to ShoulderRotate
                    new LinkageBase(joints[2], joints[3]) { Length = 6 }, //ShoulderRotate to Elbow
                    new LinkageBase(joints[3], joints[4]) { Length = 3 }, //Elbow To Wrist
                    new LinkageBase(joints[4], joints[5]) { Length = 0 }  //Wrist to Wrist Rotate
                };
            */

            IArm arm = null;

            try
            {
                if (_controller == null)
                {
                    _controller = new ArmController(arm);

                    _controller.ArmInitialized += controller_ArmInitialized;
                    _controller.ArmMoved += _controller_ArmMoved;
                }

                _controller.InitializeArm();

                StatusLabel.Content = "Arm Initialized";
            }
            catch (Exception ex)
            {
                StatusLabel.Content = ex.Message;
            }
        }

        void _controller_ArmMoved(object sender, EventArgs e)
        {
            DrawArm();
        }

        void DrawArm()
        {
            _linkBrush = Brushes.Green;

            ArmCanvas.Children.Clear();
            //int top = 5;

            if (_motorCoordinates != null)
            {
                _motorCoordinates.Clear();
            }

            if (_controller != null)
            {
                _scale = CalculateScale();

                foreach (ILinkage linkage in _controller.OrderedLinkage)
                {
                    //Label label = new Label() { Content = linkage.Name, Foreground = Brushes.White, Height = 25 };

                    //Canvas.SetLeft(label, 5);
                    //Canvas.SetTop(label, top);

                    //ArmCanvas.Children.Add(label);

                    //top += 30;
                    DrawLinkage(linkage);
                }
            }
        }

        private Dictionary<IJoint, Point> _motorCoordinates = new Dictionary<IJoint, Point>();

        void DrawMotor(IJoint joint)
        {
            Shape shape = new Ellipse()
            {
                Width = 15,
                Height= 15               
            };

            shape.Fill = Brushes.Red;
            shape.Stroke = Brushes.White;

            double motorCenterX = (double)ArmCanvas.ActualWidth/2;
            double motorCenterY = 0+shape.Height/2;

            if (_motorCoordinates.ContainsKey(joint))
            {
                motorCenterX = _motorCoordinates[joint].X;
                motorCenterY = _motorCoordinates[joint].Y;
            }

            Canvas.SetLeft(shape, motorCenterX - shape.Width / 2);
            Canvas.SetBottom(shape, motorCenterY - shape.Height/2);

            ArmCanvas.Children.Add(shape);

            Label label = new Label() { Content = joint.Name, Foreground = Brushes.White, Height = 25 };

            Canvas.SetLeft(label, motorCenterX + shape.Width);
            Canvas.SetBottom(label, motorCenterY - label.Height/2);

            ArmCanvas.Children.Add(label);

            if (!_motorCoordinates.ContainsKey(joint))
            {
                _motorCoordinates.Add(joint, new Point(Canvas.GetLeft(shape) + shape.Width / 2, Canvas.GetBottom(shape) + shape.Height / 2)); // motorCenterX, 0+(shape.Height/2)));
            }
        }

        double CalculateScale()
        {
            var linkageHeight = (from l in _controller.Linkages
                                 select CalculateLineLength(l,1)).Sum();

            if (linkageHeight > ArmCanvas.ActualHeight)
            {
                return (ArmCanvas.ActualHeight - 30) / linkageHeight;
            }
            else
            {
                return 1;
            }
        }

        double CalculateLineLength(ILinkage linkage, double scale)
        {
            double factor = 96 / 25.4;

            return (linkage.Length * factor) * scale;
        }

        Point DetermineXY(Point currentX, double length, double angle)
        {
            double x2 = currentX.X + length * Math.Cos(angle / (180 / Math.PI));
            double y2 = currentX.Y + length * Math.Sin(angle / (180 / Math.PI));

            return new Point(x2, y2);
        }

        void DrawCube()
        {
            MeshGeometry3D armBase = new MeshGeometry3D()
            {
                Positions = new Point3DCollection(new Point3D[] { new Point3D(-5, -5, 0), new Point3D(-5, 5, 0), new Point3D(5, 5, 0), new Point3D(5, -5, 0) }),
                TriangleIndices = new Int32Collection(new int[] { 0, 1, 2, //back
                                                                  2, 3, 0  //back
                                                                }),
                Normals = new Vector3DCollection(new Vector3D[] { new Vector3D(0, 0, 1), 
                                                                  new Vector3D(0, 0, 1), 
                                                                  new Vector3D(0, 0, 1),
                                                                  new Vector3D(0, 0, 1)
                                                                }),
                TextureCoordinates = new PointCollection(new Point[] { 
                    new Point(0, 1),
                    new Point(1, 1),
                    new Point(0, 0),
                    new Point(1, 0)
                })
            };

            ArmViewPort.Children.Add(new ModelVisual3D()
            {
                Content = new Model3DGroup()
                {
                    Children = new Model3DCollection(
                        new Model3D[] { 
                                new AmbientLight(Colors.Yellow),
                                new DirectionalLight(Colors.White, new Vector3D(-3, -4, -5)),
                                new GeometryModel3D() { 
                                    Geometry = armBase,
                                    Material = new DiffuseMaterial(Brushes.Yellow),
                                    BackMaterial = new DiffuseMaterial(Brushes.Green),
                                    Transform = new TranslateTransform3D(2,0,-1)
                                } 
                        })
                }
            });
        }

        private Brush _linkBrush = Brushes.Green;

        void DrawLinkage(ILinkage linkage)
        {
            DrawMotor(linkage.StartJoint);

            //Draw Line for Link
            //  Determine the angle based on the motor degrees
            Point motorCenter = _motorCoordinates.ContainsKey(linkage.StartJoint) ? _motorCoordinates[linkage.StartJoint] : new Point(0, 0);

            Point endXY = DetermineXY(motorCenter, CalculateLineLength(linkage, _scale), linkage.StartJoint.CurrentRotationDegree);

            Vector v1 = new Vector(1, 1);
            Vector v2 = new Vector(1, 5);
           
            Line linkageLine = new Line()
            {
                X1 = motorCenter.X,
                Y1 = ArmCanvas.ActualHeight - motorCenter.Y,
                X2 = endXY.X,
                Y2 = ArmCanvas.ActualHeight - endXY.Y,
                Stroke = _linkBrush, //Brushes.White,
                StrokeThickness = 1
            };

            ArmCanvas.Children.Add(linkageLine);

            //  Determine the length based on the length of the linkage            
            _motorCoordinates.Add(linkage.EndJoint, new Point(linkageLine.X2, ArmCanvas.ActualHeight - linkageLine.Y2));

            DrawMotor(linkage.EndJoint);

            Random rand = new Random();

            _linkBrush = new SolidColorBrush(Color.FromRgb((byte)rand.Next(255),(byte)rand.Next(255),(byte)rand.Next(255)));
        }

        void controller_ArmInitialized(object sender, EventArgs e)
        {
            JointEditor.ItemsSource = _controller.Joints;
            DrawArm();
        }

        private void ChangeJoints_Click(object sender, RoutedEventArgs e)
        {
            DrawArm();
        }
    }
}
