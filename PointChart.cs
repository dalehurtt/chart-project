using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Charts {

    // ============================== DAILYPOINTDATA CLASS ==============================

    public class DailyPointData {
        public decimal Close { get; set; }
        public DateTime Date { get; set; }
        public decimal HighLow { get; set; }
        public int HighLowIndex { get; set; }
        public decimal Point { get; set; }
        public int PointIndex { get; set; }
        public PointSignal Signal { get; set; }
        public decimal Target { get; set; }
        public int TargetIndex { get; set; }

        public static string Header = $"Date,Close,Point,Signal,High/Low,Target";

        public string ToCsv () {
            return $"{Date:yyyy-MM-dd},{Close:F2},{Point:F2},{Signal},{HighLow:F2},{Target:F2}";
        }
    }

    // ============================== DAILYPOINTLIST CLASS ==============================

    public class DailyPointList : List<DailyPointData> {

        private static readonly decimal percentageChange = 0.015M;

        public DailyPointList (List<DailyStockData> values) {
            List<decimal> scale = null;
            List<int> highs = new List<int> ();
            List<int> lows = new List<int> ();

            try {
                scale = GetHighLowAndIncrement (values);

                var cnt = values.Count;

                DailyPointData lastPoints = new DailyPointData {
                    Close = 0,
                    HighLow = 0,
                    HighLowIndex = -1,
                    Point = 0,
                    PointIndex = -1,
                    Signal = PointSignal.Unknown,
                    Target = 0,
                    TargetIndex = -1
                };

                for (var i = 0; i < cnt; i++) {
                    DailyPointData newPoints = CalculatePoint (values [i], scale, lastPoints, highs, lows);

                    // Check to see if the price reversed.
                    if (lastPoints.Signal != newPoints.Signal) {
                        if (lastPoints.Signal == PointSignal.Buy || lastPoints.Signal == PointSignal.Up) {
                            highs.Add (lastPoints.HighLowIndex);
                        }
                        else if (lastPoints.Signal == PointSignal.Sell || lastPoints.Signal == PointSignal.Down) {
                            lows.Add (lastPoints.HighLowIndex);
                        }
                    }

                    lastPoints = newPoints;
                    this.Add (newPoints);
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "DailyPointList");
            }
        }

        private DailyPointData CalculatePoint (DailyStockData value, List<decimal> scale, DailyPointData lastPoints, List<int> highs, List<int> lows) {
            DailyPointData newPoints = null;
            decimal newPoint;
            int newIndex;

            try {
                (newPoint, newIndex) = FindPointValue (scale, value.Close, lastPoints.Signal);

                newPoints = new DailyPointData {
                    Close = value.Close,
                    Date = value.Date
                };

                switch (lastPoints.Signal) {
                    case PointSignal.Unknown:
                        // This is the first data point.
                        if (lastPoints.PointIndex == -1) {
                            newPoints.HighLowIndex = newIndex;
                            newPoints.HighLow = newPoint;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            newPoints.Signal = PointSignal.Unknown;
                            newPoints.TargetIndex = -1;
                            newPoints.Target = 0;
                        }
                        // The Close went up.
                        else if (lastPoints.Close < value.Close) {
                            newPoints.HighLowIndex = newIndex;
                            newPoints.HighLow = newPoint;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            newPoints.Signal = PointSignal.Up;
                            newPoints.TargetIndex = newIndex + 3;
                            newPoints.Target = scale [newPoints.TargetIndex];
                        }
                        // The Close went down.
                        else if (lastPoints.Close > value.Close) {
                            // Because Unknown is rounded as if it were Up, we need to adjust it back down.
                            newIndex -= 1;
                            newPoint = scale [newIndex];
                            newPoints.HighLowIndex = newIndex;
                            newPoints.HighLow = newPoint;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            newPoints.Signal = PointSignal.Down;
                            newPoints.TargetIndex = newIndex >= 3 ? newIndex - 3 : 0;
                            newPoints.Target = scale [newPoints.TargetIndex];
                        }
                        // The Close was the same.
                        else {
                            newPoints.HighLowIndex = newIndex;
                            newPoints.HighLow = newPoint;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            newPoints.Signal = PointSignal.Unknown;
                            newPoints.TargetIndex = -1;
                            newPoints.Target = 0;
                        }
                        break;

                    case PointSignal.Buy:
                    case PointSignal.Up:
                        // The Close went below the Target.
                        if (value.Close <= lastPoints.Target) {
                            newIndex -= 1;
                            newPoint = scale [newIndex];
                            var newTargetIndex = newIndex >= 3 ? newIndex - 3 : 0;
                            newPoints.HighLowIndex = newIndex;
                            newPoints.HighLow = newPoint;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            if (lows.Count > 0 && newIndex > lows.Last ()) {
                                newPoints.Signal = PointSignal.Sell;
                            }
                            else {
                                newPoints.Signal = PointSignal.Down;
                            }
                            newPoints.TargetIndex = newTargetIndex;
                            newPoints.Target = scale [newTargetIndex];
                        }
                        // The Close went up.
                        else if (value.Close > lastPoints.HighLow) {
                            var newTargetIndex = newIndex + 3;
                            newPoints.HighLowIndex = newIndex;
                            newPoints.HighLow = newPoint;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            newPoints.Signal = PointSignal.Up;
                            newPoints.TargetIndex = newTargetIndex;
                            newPoints.Target = scale [newTargetIndex];
                        }
                        // The Close was the same or not high enough to make the Target.
                        else {
                            newPoints.HighLowIndex = lastPoints.HighLowIndex;
                            newPoints.HighLow = lastPoints.HighLow;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            newPoints.Signal = PointSignal.Up;
                            newPoints.TargetIndex = lastPoints.TargetIndex;
                            newPoints.Target = lastPoints.Target;
                        }
                        break;

                    case PointSignal.Sell:
                    case PointSignal.Down:
                        // The Close went above the Target.
                        if (value.Close >= lastPoints.Target) {
                            newIndex += 1;
                            newPoint = scale [newIndex];
                            var newTargetIndex = newIndex + 3;
                            newPoints.HighLowIndex = newIndex;
                            newPoints.HighLow = newPoint;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            if (highs.Count > 0 && newIndex < highs.Last ()) {
                                newPoints.Signal = PointSignal.Buy;
                            }
                            else {
                                newPoints.Signal = PointSignal.Up;
                            }
                            newPoints.TargetIndex = newTargetIndex;
                            newPoints.Target = scale [newTargetIndex];
                        }
                        // The Close went down.
                        else if (value.Close < lastPoints.HighLow) {
                            var newTargetIndex = newIndex >= 3 ? newIndex - 3 : 0;
                            newPoints.HighLowIndex = newIndex;
                            newPoints.HighLow = newPoint;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            newPoints.Signal = PointSignal.Down;
                            newPoints.TargetIndex = newTargetIndex;
                            newPoints.Target = scale [newTargetIndex];
                        }
                        // The Close was the same or not high enough to make the Target.
                        else {
                            newPoints.HighLowIndex = lastPoints.HighLowIndex;
                            newPoints.HighLow = lastPoints.HighLow;
                            newPoints.PointIndex = newIndex;
                            newPoints.Point = newPoint;
                            newPoints.Signal = PointSignal.Down;
                            newPoints.TargetIndex = lastPoints.TargetIndex;
                            newPoints.Target = lastPoints.Target;
                        }
                        break;
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} {Program.currentTicker}", true, "CalculatePoint");
            }
            return newPoints;
        }

        private (decimal, int) FindPointValue (List<decimal> scale, decimal close, PointSignal lastSignal) {
            try {
                int cnt = scale.Count;
                decimal lastPoint = 0;
                int lastIndex = -1;

                switch (lastSignal) {
                    case PointSignal.Buy:
                    case PointSignal.Up:
                    case PointSignal.Unknown:
                        for (int i = cnt - 1; i >= 0; i--) {
                            if (close < scale [i]) {
                                lastPoint = scale [i + 1];
                                lastIndex = i + 1;
                                break;
                            }
                        }
                        break;

                    default:
                        for (int i = 0; i < cnt; i++) {
                            if (close > scale [i]) {
                                lastPoint = scale [i - 1];
                                lastIndex = i - 1;
                                break;
                            }
                        }
                        break;
                }
                return (lastPoint, lastIndex);
            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "FindPointValue");
                return (close, -1);
            }
        }

        private List<decimal> GetHighLowAndIncrement (List<DailyStockData> values) {
            List<decimal> scale = null;
            try {
                decimal high = 0, low = 0;
                foreach (DailyStockData value in values) {
                    if (value.High > high)
                        high = value.High;
                    if ((low == 0 && value.Low < high) || value.Low < low)
                        low = value.Low;
                }

                scale = new List<decimal> ();
                decimal scaleHigh = high * (1 + percentageChange);
                scale.Add (scaleHigh);
                decimal ctr = high;
                while (ctr >= low) {
                    scale.Add (ctr);
                    ctr -= (ctr * percentageChange);
                }
                scale.Add (ctr);

            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "GetHighLowAndIncrement");
            }
            return scale;
        }

        public void OutputAsCsv (string filePath) {
            try {
                using StreamWriter file = new StreamWriter (filePath);
                file.WriteLine (DailyPointData.Header);
                foreach (DailyPointData value in this) {
                    file.WriteLine (value.ToCsv ());
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "OutputPointValuesAsCsv");
            }
        }
    }

    // ============================== FLOATSIGNAL ENUM ==============================

    public enum PointSignal {
        Buy,
        Up,
        Sell,
        Down,
        Unknown
    }
}
