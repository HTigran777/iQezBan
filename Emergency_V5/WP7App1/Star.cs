using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WP7App1
{
    /// <summary>
    /// Represents a signle star in the vast game universe.
    /// </summary>
    public class Star
    {
        public double X;
        public double Y;

        /// <summary>
        /// Distance from screen in the range 0..1
        /// </summary>
        public double Z;

        /// <summary>
        /// Star brightness
        /// Brightness * Z (distance) determine the star "dimness" on screen
        /// </summary>
        public double Brightness;

        /// <summary>
        /// Precomputed speed, used for optimizations
        /// </summary>
        public double PrecomputedSpeed;

        /// <summary>
        /// The star
        /// </summary>
        public Ellipse It;
    }
}
