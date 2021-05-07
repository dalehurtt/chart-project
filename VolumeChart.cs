using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Charts {

    public class DailyVolumeData {

        public DateTime Date { get; set; }
        public long Volume { get; set; }

    }

    public class DailyVolumeList : List<DailyVolumeData> {

        public string Ticker { get; set; }

        public DailyVolumeList (List<DailyStockData> values, string ticker) {
            Ticker = ticker;
            try {
                foreach (DailyStockData value in values) {
                    DailyVolumeData vol = new () {
                        Date = value.Date,
                        Volume = value.Volume
                    };
                    this.Add (vol);
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "DailyVolumeList");
            }
        }
    }

    public class AverageVolumeData {

        public DateTime Date { get; set; }
        public long Volume { get; set; }
        public long AverageVolume { get; set; }
        public decimal Strength { get; set; }

        public static string Header = $"Date,Volume,Average Volume,Strength";

        public string ToCsv () {
            return $"{Date:yyyy-MM-dd},{Volume},{AverageVolume},{Strength}";
        }
    }

    public class AverageVolumeList : List<AverageVolumeData> {

        public string Ticker { get; set; }
        public int NumDays { get; set; }

        public AverageVolumeList (List<DailyVolumeData> values, string ticker, int numDays) {
            Ticker = ticker;
            NumDays = numDays;
            int _i = -1, _j = -1;
            try {
                for (var i = 0; i < values.Count; i++) {
                    _i = i;
                    DailyVolumeData vol = values [i];
                    int downTo = i >= (numDays - 1) ? i - (numDays - 1) : 0;
                    long accVol = 0;
                    for (var j = i; j >= downTo; j--) {
                        _j = j;
                        accVol += values [j].Volume;
                    }
                    int divDays = i >= (numDays - 1) ? numDays : i + 1;
                    long avgVol = accVol / divDays;
                    decimal strength = (decimal) vol.Volume / avgVol;
                    this.Add (new AverageVolumeData {
                        Date = vol.Date,
                        Volume = vol.Volume,
                        AverageVolume = avgVol,
                        Strength = strength
                    });
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole ($"{Utils.ExToString (ex)} {_i} {_j}", true, "AverageVolumeList");
            }
        }

        public void OutputAsCsv (string filePath) {
            try {
                using StreamWriter file = new StreamWriter (filePath);
                file.WriteLine (AverageVolumeData.Header);
                foreach (AverageVolumeData value in this) {
                    file.WriteLine (value.ToCsv ());
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "OutputAsCsv");
            }
        }
    }
}
