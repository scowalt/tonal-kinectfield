using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;

using SuperWebSocket;

namespace tonal_kinectfield
{
    class Program
    {
        static WebSocketServer appServer;
        static void Main(string[] args)
        {
            InitializeKinect();

            InitializeWebSockets();

            // close program on console return
            Console.Read();

            appServer.Stop();
        }

        private static void InitializeKinect()
        {
            // get the sensor
            KinectSensor kinectSensor = KinectSensor.GetDefault();

            // get the coordinate mapper from the sensor
            CoordinateMapper coordinateMapper = kinectSensor.CoordinateMapper;

            // get the depth extents (???)
            FrameDescription frameDescription = kinectSensor.DepthFrameSource.FrameDescription;

            // get the size of the space
            int displayWidth = frameDescription.Width;
            int displayHeight = frameDescription.Height;

            // open the reader of the body frames
            BodyFrameReader bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();

            // and frame handler
            bodyFrameReader.FrameArrived += Reader_FrameArrived;

            // open the sensor
            kinectSensor.Open();
        }

        private static void InitializeWebSockets()
        {
            appServer = new WebSocketServer();

            bool success = appServer.Setup(7446);
            if (!success)
            {
                // TODO ???
                throw new Exception("failed to setup WebSocket server");
            }

            bool started = appServer.Start();
            appServer.NewSessionConnected += Socket_NewSession;

            if (!started)
            {
                throw new Exception("failed to start WebSocket server");
            }
        }

        private static void Socket_NewSession(WebSocketSession session)
        {
            Console.WriteLine("new Websocket client");
        }

        private static void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            List<WebSocketSession> sessions = appServer.GetAllSessions().ToList<WebSocketSession>();
            foreach (WebSocketSession session in sessions)
            {
                session.Send("test");
            }
        }
    }
}
