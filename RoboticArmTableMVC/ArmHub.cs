using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using RoboticArmTableCore;
using System.Collections.Generic;

namespace RoboticArmTableMVC
{
 
    public class ArmHub : Hub
    {
        public class Vector
        {
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }
        }

        public class Obstacle
        {
            public Vector[] Points { get; set; }
        }

        public class Triangle
        {
            public string Color { get; set; }
            public Vector[] Points { get; set; }
        }

        public class InitParameters
        {
            public float currentX { get; set; }
            public float currentZ { get; set; }
            [JsonConverter(typeof(JasonTypeConverter<ArmBase>))]
            public IArm ArmDefinition { get; set; }
            public dynamic[] Obstacles { get; set; }
            public dynamic[] MoveableObjects { get; set; }
        }

        public void Init(InitParameters parameters)
        {
            Clients.All.init(parameters);
        }

        public void SendCommand(string command, dynamic body)
        {
            Clients.All.sendCommand(command, body);
        }

        public string GetObstacles()
        {
            return Clients.All.getObstacles();
        }

        public void PlotMovementMatrix(dynamic matrix)
        {
            Clients.All.plotMovementMatrix(matrix);
        }

        public void PlotCurrentGripperLocation(float x, float y, float z)
        {
            Clients.All.plotCurrentGripperLocation(x, y, z);
        }

        public void BroadcastObstacles(dynamic obstacles)
        {
            Clients.All.broadcastObstacles(obstacles);
        }

        public void MoveCurrentGripperLocation(float x, float y, float z)
        {
            Clients.All.moveCurrentGripperLocation(x, y, z);
        }

        public void DrawTriangleDetails(List<Triangle> triangles)
        {
            Clients.All.drawTriangleDetails(triangles);
        }

        public void UpdateBaseLocation(float x, float z)
        {
            Clients.All.updateBaseLocation(x, z);
        }
    }
}