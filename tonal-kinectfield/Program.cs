using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Kinect;

namespace tonal_kinectfield
{
    class Program
    {
        static void Main(string[] args)
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

            // close program on console return
            Console.Read();
        }

        private static void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            Console.WriteLine("frame arrived");
        }
    }
}
