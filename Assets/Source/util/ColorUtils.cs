using com.perroelectrico.flip.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.perroelectrico.flip.util {

    public class ColorSet {
        public static readonly Color[] colors = {
            ColorUtils.ColorFromHtmlString("668000"),
            ColorUtils.ColorFromHtmlString("ffcc00"),
            ColorUtils.ColorFromHtmlString("00aad4"),
            ColorUtils.ColorFromHtmlString("d45500"),
            ColorUtils.ColorFromHtmlString("918a6f"),
            ColorUtils.ColorFromHtmlString("a61ba6"),
            ColorUtils.ColorFromHtmlString("1a1d28"),
            ColorUtils.ColorFromHtmlString("215478"),
            ColorUtils.ColorFromHtmlString("008033"),
        };

        public static Color[] Get(int n) {
            return colors.Take(n).ToArray();
        }
    }

    class ColorUtils {
        public static Color ColorFromInt(int r, int g, int b, int a = 255) {
            return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255);
        }

        public static Color ColorFromHtmlString(string str) {
            int r = Convert.ToInt32(str.Substring(0, 2), 16);
            int g = Convert.ToInt32(str.Substring(2, 2), 16);
            int b = Convert.ToInt32(str.Substring(4, 2), 16);
            return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f);
        }

        /// <summary>
        /// Generates N colors equally distributed accross the hue and with same saturation and value, using a HSV color model
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Color[] GenerateColors(int n) {
            double s = 0.75f;
            double V = 0.95f;
            double h = 0;
            double hIncrement = 1f / n;
            Color[] colors = new Color[n];
            for (int i = 0 ; i < n ; i++, h += hIncrement) {
                colors[i] = HsvToRgb(h, s, V);
            }
            return colors;
        }

        /// <summary>
        /// Convert HSV to RGB
        /// h is from 0-1
        /// s,v values are 0-1
        /// r,g,b values are 0-1
        /// Based upon http://ilab.usc.edu/wiki/index.php/HSV_And_H2SV_Color_Space#HSV_Transformation_C_.2F_C.2B.2B_Code_2
        /// </summary>
        public static Color HsvToRgb(double h, double S, double V) {
            // ######################################################################
            // T. Nathan Mundhenk
            // mundhenk@usc.edu
            // C/C++ Macro HSV to RGB

            h *= 360;
            double R, G, B;

            if (V <= 0) {
                R = G = B = 0;
            } else if (S <= 0) {
                R = G = B = V;
            } else {
                double hf = h / 60.0;
                int i = (int)hf;
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i) {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            return new Color(Mathf.Clamp01((float)R), Mathf.Clamp01((float)G), Mathf.Clamp01((float)B));
        }

        internal static object ToHtmlString(Color color) {
            return ((int)(color.r * 255)).ToString("X2") + ((int)(color.g * 255)).ToString("X2") + ((int)(color.b * 255)).ToString("X2");
        }
    }

}