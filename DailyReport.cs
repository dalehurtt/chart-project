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

        public static readonly string Header = "Ticker,Date,Close,P Signal,High/Low,Reversal,Changed,F Signal,Buy,Sell,Changed";

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
            return $"{Ticker},{Date:yyyy-MM-dd},{Close:F2},{PointSignal},{HighLow:F2},{Reversal:F2},{(PChanged ? "Yes" : "")},{FloatSignal},{Buy:F2},{Sell:F2},{(FChanged ? "Yes" : "")}";
        }
    }

    public class DailyReport : List<DailyReportData> {

        public void OutputAsCsv (string filePath) {
            try {
                using StreamWriter file = new StreamWriter (filePath);
                file.WriteLine (DailyReportData.Header);
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
