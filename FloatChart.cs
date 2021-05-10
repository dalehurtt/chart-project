using System;
using System.Collections.Generic;
using System.IO;

namespace Charts {

    // ============================== DAILYFLOATDATA CLASS ==============================

    public class DailyFloatData {

        public DateTime EndDate { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public FloatSignal Signal { get; set; }
        public DateTime StartDate { get; set; }

        public static readonly string Header = $"End Date,Start Date,Buy,Sell,Signal";

        public string ToCsv () {
            return $"{EndDate:yyyy-MM-dd},{StartDate:yyyy-MM-dd},{High:F2},{Low:F2},{Signal}";
        }
    }

    // ============================== DAILYFLOATLIST CLASS ==============================

    public class DailyFloatList : List<DailyFloatData> {

        private string Ticker;

        public DailyFloatList (List<DailyStockData> values, long flt, string ticker) {
            Ticker = ticker;
            try {
                var cnt = values.Count;
                for (var i = 0; i < cnt; i++) {
                    this.Add (CalculateFloat (values, flt, i));
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} {Ticker}", true, "DailyFloatList");
            }
        }

        private DailyFloatData CalculateFloat (List<DailyStockData> values, long flt, int idx) {
            DailyFloatData fv = null;
            try {
                var cnt = values.Count;
                long vol = 0;
                decimal high = 0;
                decimal low = 0;
                DateTime? endDate = null;
                DateTime? startDate = null;
                DailyStockData today = null;

                for (var i = idx; i >= 0; i--) {
                    today = values [i];
                    if (!endDate.HasValue)
                        endDate = today.Date;
                    if (high < today.High)
                        high = today.High;
                    if (low == 0 || low > today.Low)
                        low = today.Low;
                    vol += today.Volume;
                    if (vol >= flt) {
                        startDate = today.Date;
                        break;
                    }
                }

                fv = new DailyFloatData {
                    EndDate = endDate.Value,
                    High = high,
                    Low = low,
                    Signal = (vol < flt) ? FloatSignal.Unknown : FloatSignal.Neutral,
                    StartDate = (vol < flt) ? today.Date : startDate.Value
                };

                if (vol >= flt && idx > 0) {
                    today = values [idx];
                    var yesterday = values [idx - 1];
                    if (today.High == high && yesterday.High < high) { fv.Signal = FloatSignal.Buy; }
                    else if (today.Low == low && yesterday.Low > low) { fv.Signal = FloatSignal.Sell; }
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} {Ticker}", true, "CalculateFloat");
            }
            return fv;
        }

        public void OutputAsCsv (string filePath) {
            try {
                using StreamWriter file = new (filePath);
                file.WriteLine (DailyFloatData.Header);
                foreach (DailyFloatData value in this) {
                    file.WriteLine (value.ToCsv ());
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} {Ticker}", true, "OutputAsCsv");
            }
        }
    }

    // ============================== FLOATSIGNAL ENUM ==============================

    public enum FloatSignal {
        Buy,
        Neutral,
        Sell,
        Unknown
    }
}
