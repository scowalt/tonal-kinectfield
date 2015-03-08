using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Kinect stuff
using Microsoft.Kinect;

// WebSocket library
using SuperWebSocket;

// JSON serialization
using Newtonsoft.Json;

namespace tonal_kinectfield
{
    class Program
    {
        static Body[] bodies;
        static WebSocketServer appServer;
        static KinectSensor kinectSensor;
        static readonly int PORT = 7446;
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
            kinectSensor = KinectSensor.GetDefault();

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

            bool success = appServer.Setup(PORT);
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

            Console.WriteLine("WebSocket Server Initialized on port " + PORT);
        }

        private static void Socket_NewSession(WebSocketSession session)
        {
            Console.WriteLine("New WebSocket client connected");

            // get information about the kinect sensor
            FrameDescription frameDescription = kinectSensor.DepthFrameSource.FrameDescription;
            string json = JsonConvert.SerializeObject(frameDescription);
            session.Send(json);
        }

        private static void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            if (appServer == null)
            {
                Console.WriteLine("WebSocket server doesn't exist");
                return;
            }            

            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    dataReceived = true;

                    if (bodies == null)
                    {
                        bodies = new Body[bodyFrame.BodyCount];
                    }

                    bodyFrame.GetAndRefreshBodyData(bodies);
                }
            }

            if (dataReceived)
            {
                List<WebSocketSession> sessions = appServer.GetAllSessions().ToList<WebSocketSession>();
                foreach (WebSocketSession session in sessions)
                {
                    foreach (Body body in bodies)
                    {
                        if (body.IsTracked)
                        {
                            string json = JsonConvert.SerializeObject(body);
                            session.Send(json);
                        }
                    }
                    
                }
            }
        }
    }
}
