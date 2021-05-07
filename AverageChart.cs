using System;
using System.Collections.Generic;
using System.IO;

namespace Charts {

    public class DailyAverageData {

        public DateTime Date { get; set; }
        public decimal Close { get; set; }
        public decimal MA1 { get; set; }
        public decimal MA2 { get; set; }
        public string Signal { get; set; }

        public static readonly string Header = $"Date,Close,MA 1,MA 2,Signal";

        public string ToCsv () {
            return $"{Date:yyyy-MM-dd},{Close:F2},{MA1:F2},{MA2:F2},{Signal}";
        }
    }

    public class SimpleAverageList : List<DailyAverageData> {

        public string Ticker { get; set; }
        public int NumDays { get; set; }

        public SimpleAverageList (List<DailyStockData> values, string ticker, int numDays1, int numDays2) {
            Ticker = ticker;
            NumDays = numDays1;
            int _i = -1, _j = -1, _k = -1;
            try {
                for (var i = 0; i < values.Count; i++) {
                    _i = i;
                    DailyStockData data = values [i];
                    // MA 1
                    int downTo1 = i >= (numDays1 - 1) ? i - (numDays1 - 1) : 0;
                    decimal acc1 = 0;
                    for (var j = i; j >= downTo1; j--) {
                        _j = j;
                        acc1 += values [j].Close;
                    }
                    int divDays1 = i >= (numDays1 - 1) ? numDays1 : i + 1;
                    decimal avgPrice1 = acc1 / divDays1;
                    // MA 2
                    int downTo2 = i >= (numDays2 - 1) ? i - (numDays2 - 1) : 0;
                    decimal acc2 = 0;
                    for (var k = i; k >= downTo2; k--) {
                        _k = k;
                        acc2 += values [k].Close;
                    }
                    int divDays2 = i >= (numDays2 - 1) ? numDays2 : i + 1;
                    decimal avgPrice2 = acc2 / divDays2;
                    // Create data record
                    string signal = string.Empty;
                    if (data.Close >= avgPrice1 && data.Close >= avgPrice2) {
                        if (avgPrice1 >= avgPrice2) signal = "P12";
                        else signal = "P21";
                    }
                    else if (avgPrice1 >= data.Close && avgPrice1 >= avgPrice2) {
                        if (data.Close >= avgPrice2) signal = "1P2";
                        else signal = "12P";
                    }
                    else {
                        if (data.Close >= avgPrice1) signal = "2P1";
                        else signal = "21P";
                    }
                    this.Add (new DailyAverageData {
                        Date = data.Date,
                        Close = data.Close,
                        MA1 = avgPrice1,
                        MA2 = avgPrice2,
                        Signal = signal
                    });
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)}\ni: {_i} j: {_j} k: {_k}", true, "SimpleAverageList");
            }
        }

        public void OutputAsCsv (string filePath) {
            try {
                using StreamWriter file = new (filePath);
                file.WriteLine (DailyAverageData.Header);
                foreach (DailyAverageData value in this) {
                    file.WriteLine (value.ToCsv ());
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "OutputAsCsv");
            }
        }
    }

    public class ExponentialAverageList : List<DailyAverageData> {

        public string Ticker { get; set; }
        public int NumDays { get; set; }

    }
}
