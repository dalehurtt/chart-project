using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Charts {

    public class DailyReportData {

        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public decimal Close { get; set; }
        public PointSignal PointSignal { get; set; }
        public bool PChanged { get; set; }
        public decimal HighLow { get; set; }
        public decimal Reversal { get; set; }
        public FloatSignal FloatSignal { get; set; }
        public decimal Buy { get; set; }
        public decimal Sell { get; set; }
        public bool FChanged { get; set; }
        public decimal MA1 { get; set; }
        public decimal MA2 { get; set; }
        public string MASignal { get; set; }
        public decimal VolStrength { get; set; } 

        public List<AverageVolumeData> AddToReport (List<AverageVolumeData> values) {
            AverageVolumeData last = values.Last();
            VolStrength = last.Strength;
            return values;
        }

        public List<DailyAverageData> AddToReport (List<DailyAverageData> values) {
            DailyAverageData last = values.Last();
            MA1 = last.MA1;
            MA2 = last.MA2;
            MASignal = last.Signal;
            return values;
        }

        public List<DailyFloatData> AddToReport (List<DailyFloatData> values) {
            DailyFloatData last = values.Last ();
            DailyFloatData prev = values.SkipLast (1).Last ();
            Buy = last.High;
            FloatSignal = last.Signal;
            Sell = last.Low;
            FChanged = last.High != prev.High || last.Low != prev.Low || last.Signal != prev.Signal;
            return values;
        }

        public List<DailyPointData> AddToReport (List<DailyPointData> values) {
            DailyPointData last = values.Last ();
            DailyPointData prev = values.SkipLast (1).Last ();
            Close = last.Close;
            Date = last.Date;
            HighLow = last.HighLow;
            PointSignal = last.Signal;
            Reversal = last.Target;
            PChanged = last.HighLow != prev.HighLow || last.Signal != prev.Signal;
            return values;
        }

        public string ToCsv () {
            return $"{Ticker},{Date:yyyy-MM-dd},{Close:F2},{PointSignal},{HighLow:F2},{Reversal:F2},{(PChanged ? "Yes" : "")},{FloatSignal},{Buy:F2},{Sell:F2},{(FChanged ? "Yes" : "")},{MA1:F2},{MA2:F2},{MASignal},{VolStrength:P2}";
        }
    }

    public class DailyReport : List<DailyReportData> {

        private int MANumDays1;
        private int MANumDays2;

        public DailyReport (int maNumDays1, int maNumDays2) {
            MANumDays1 = maNumDays1;
            MANumDays2 = maNumDays2;
        }

        public string Header() {
            return $"Ticker,Date,Close,P Signal,High/Low,Reversal,Changed,F Signal,Buy,Sell,Changed,MA({MANumDays1}),MA({MANumDays2}),M Signal,Vol Strength";
        }

        public void OutputAsCsv (string filePath) {
            try {
                using StreamWriter file = new StreamWriter (filePath);
                file.WriteLine (this.Header ());
                foreach (DailyReportData value in this) {
                    file.WriteLine (value.ToCsv ());
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} {Program.currentTicker}", true, "OutputReportAsCsv");
            }
        }
    }
}
