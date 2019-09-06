using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem
{
    public class WeatherManager
    {
        public static WeatherControl Control { get; private set; }

        public static void SetWeatherControl(WeatherControl control)
        {
            Control = control;
        }

        public static MoveControl MoveControl { get; private set; }

        public static void SetMoveControl(MoveControl moveControl)
        {
            MoveControl = moveControl;
        }
    }
}