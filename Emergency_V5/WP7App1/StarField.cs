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
    /// Controls the in-game starfield
    /// </summary>
    public class StarField
    {
        private Random globalRandom = new Random();

        /// <summary>
        /// Star field speed
        /// </summary>
        public double Speed = 1.0;

        /// <summary>
        /// Number of star planes to simulate
        /// </summary>
        private const int PLANE_COUNT = 8;
        private int[] _starPlanes = new int[PLANE_COUNT];

        /// <summary>
        /// number of stars on screen, determined by plane count and initial star count
        /// </summary>
        private int _starCount = 0;

        public int mWIDTH = 480;
        public int mHEIGHT = 800;

        /// <summary>
        /// List of stars
        /// </summary>
        Star[] _stars;

        /// <summary>
        /// Panel containing all stars
        /// </summary>
        private Panel _panelField;

        private static Brush _starColor1 = new SolidColorBrush(Colors.White);
        private static Brush _starColor2 = new SolidColorBrush(Color.FromArgb(255, 255, 150, 150));
        private static Brush _starColor3 = new SolidColorBrush(Color.FromArgb(255, 150, 150, 255));
        private static Brush _starColor4 = new SolidColorBrush(Color.FromArgb(255, 255, 255, 150));
        private static Brush _starColor5 = new SolidColorBrush(Color.FromArgb(255, 255, 150, 255));
        private static Brush[] _starColors = new Brush[] { _starColor1 };

        protected StarField() { }

        public StarField(Panel panelField)
        {
            _panelField = panelField;
            CreateStars();
        }

        static double _viewAngle = 80;
        static double _q = (_viewAngle / 2.0) * 2 * Math.PI / 360; // viewAngle / 2 in radians
        static double _sinQ = Math.Sin(_q);
        static double _sqrtOneMinusSinQ2 = Math.Sqrt(1 - _sinQ * _sinQ);

        private void CreateStars()
        {
            int xMax = mWIDTH;
            int yMax = mHEIGHT;

            // if at a given distance D we have A count of stars
            // and q = viewAngle / 2 
            // then 
            // at distance D+H we have ( A + H / (sqrt(1-sin(q)*sin*q)) ) ^ 2
            // note this accounts only for distinct plane surfaces (incorrect, but OK), not volumes (correct way)

            double a = 4; // initial count of stars at the closest distance (0)

            _starCount = 0;
            for (int h = 0; h < PLANE_COUNT; h++)
            {
                double aNew = (a + h / _sqrtOneMinusSinQ2);
                aNew = aNew * aNew;
                _starPlanes[h] = (int) aNew;
                _starCount += _starPlanes[h];
                //System.Diagnostics.Debug.WriteLine("Distance: {0}, stars: {1}", h, aNew);
            }

            _stars = new Star[_starCount];

            // create stars in each plane, near to far
            const double NEAR_PLANE = 0.0;
            const double FAR_PLANE = 0.7;
            int currentStarIndex = 0;
            for (int h = 0; h < PLANE_COUNT; h++)
            {
                int starCountInPlane = _starPlanes[h];
                for (int starCounter = 0; starCounter < starCountInPlane; starCounter++)
                {
                    Star star = new Star();
                    _stars[currentStarIndex] = star;
                    star.X = globalRandom.Next(xMax);
                    star.Y = globalRandom.Next(yMax);
                    star.Z = (((double)h) / PLANE_COUNT) * (FAR_PLANE - NEAR_PLANE) + NEAR_PLANE; // convert H from 0..PLANE_COUNT to NEAR_PLANE..FAR_PLANE range
                    //double s0 = 1; // speed at NEAR_PLANE
                    //star.PrecomputedSpeed = s0 / (s0 + ((star.Z + 0.2) * (1 + (star.Z + 0.2) * 3) * 10) / _sqrtOneMinusSinQ2); // speed at current plane
                    star.PrecomputedSpeed = 1 / ((star.Z + 0.06) * 100); // artificial speed, looks nicer than the more reallistic one
                    star.Brightness = globalRandom.Next(100) / 100.0 + 1.2;

                    Ellipse it = new Ellipse();
                    it.Width = (2 - star.Z);
                    it.Height = (2 - star.Z);
                    it.Fill = _starColors[globalRandom.Next(_starColors.Length)];
                    it.Opacity = star.Brightness * (1 - star.Z) * (1 - star.Z);
                    Canvas.SetLeft(it, star.X);
                    Canvas.SetTop(it, star.Y);

                    star.It = it;
                    _panelField.Children.Add(it);
                    currentStarIndex++;
                }
            }
        }

        /// <summary>
        /// Updates the star field
        /// </summary>
        /// <param name="msec">time since the last update</param>
        public void UpdateStars(double msec)
        {
            int xMax = mWIDTH;
            int yMax = mHEIGHT;

            int starsOutCount = 0;
            for (int i = 0; i < _starCount; i++)
            {
                Star star = _stars[i];

                star.X = star.X - msec  * star.PrecomputedSpeed * Speed;
                if (star.X < 0) // if star is out of screen
                {
                    starsOutCount++;
                    // create star on the sample plane
                    if (starsOutCount > 2)
                    {
                        star.X = globalRandom.Next(xMax);
                    }
                    else
                    {
                        star.X = xMax;
                    }

                    star.Y = globalRandom.Next(yMax);
                    Canvas.SetTop(star.It, star.Y);
                }

                Canvas.SetLeft(star.It, star.X);
            }
        }
    }
}
