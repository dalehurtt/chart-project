using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using HtmlAgilityPack;
using Rop;

namespace Charts {

    // ============================== DAILYSTOCKDATA CLASS ==============================

    public class DailyStockData {

        public decimal AdjustedClose { get; }
        public decimal Close { get; }
        public DateTime Date { get; }
        public decimal High { get; }
        public decimal Low { get; }
        public decimal Open { get; }
        public Int64 Volume { get; }

        public static string Header = $"Date,Open,High,Low,Close,Adjusted Close,Volume";

        public DailyStockData () { }

        public DailyStockData (string date, string open, string high, string low, string close, string adjclose, string volume) {
            try {
                Date = Convert.ToDateTime (date);
                Open = Convert.ToDecimal (open);
                High = Convert.ToDecimal (high);
                Low = Convert.ToDecimal (low);
                Close = Convert.ToDecimal (close);
                AdjustedClose = Convert.ToDecimal (adjclose);
                Volume = Convert.ToInt64 (volume);
            }
            catch { }
        }

        public static DailyStockData FromCsv (string csvLineAsString) {
            DailyStockData values = null;
            try {
                string [] parsed = csvLineAsString.Split (',');
                values = new DailyStockData (parsed [0], parsed [1], parsed [2], parsed [3], parsed [4], parsed [5], parsed [6]);
            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "DailyStockData.FromCsv");
            }
            return values;
        }

        public string ToCsv () {
            return $"{Date:yyyy-MM-dd},{Open},{High},{Low},{Close},{AdjustedClose},{Volume}";
        }
    }

    // ============================== DAILYSTOCKLIST CLASS ==============================

    public class DailyStockList : List<DailyStockData> {

        private static Result<HtmlDocument, (string, string)> DownloadData (string ticker) {
            try {
                var url = $"https://finance.yahoo.com/quote/{ticker}/history?p={ticker}";
                var web = new HtmlWeb ();
                var doc = web.Load (url);

                return Result<HtmlDocument, (string, string)>.Succeeded (doc);
            }
            catch (Exception ex) {
                return Result<HtmlDocument, (string, string)>.Failed ((Utils.ExToString (ex), "DownloadData"));
            }
        }

        public static Result<DailyStockList, (string, string)> GetData (string ticker) {
            try {
                DailyStockList list = new DailyStockList ();

                var downloadResult = DownloadData (ticker);
                if (downloadResult.IsSuccess) {
                    HtmlDocument doc = downloadResult.Success;
                    var table = doc.DocumentNode.Descendants ("table").First ();
                    var tbody = table.Descendants ("tbody").First ();
                    var rows = tbody.Descendants ("tr");
                    foreach (var row in rows) {
                        var cells = row.Descendants ("td");
                        if (cells.Count () >= 7) {
                            DailyStockData values = new DailyStockData (
                                cells.ElementAt (0).InnerText,
                                cells.ElementAt (1).InnerText, cells.ElementAt (2).InnerText,
                                cells.ElementAt (3).InnerText, cells.ElementAt (4).InnerText,
                                cells.ElementAt (5).InnerText, cells.ElementAt (6).InnerText.Replace (",", ""));
                            list.Add (values);
                        }
                    }
                    list.Reverse ();
                    return Result<DailyStockList, (string, string)>.Succeeded (list);
                }
                else {
                    return Result<DailyStockList, (string, string)>.Failed (downloadResult.Failure);
                }
            }
            catch (Exception ex) {
                return Result<DailyStockList, (string, string)>.Failed (($"{Utils.ExToString (ex)}", "GetData"));
            }
        }

        public static DailyStockList OutputToCsv (DailyStockList list, string filePath) {
            try {
                using StreamWriter file = new StreamWriter (filePath);
                file.WriteLine (DailyStockData.Header);
                foreach (DailyStockData value in list) {
                    file.WriteLine (value.ToCsv ());
                }
            }
            catch (Exception ex) {
                Utils.WriteToConsole (Utils.ExToString (ex), true, "OutputToCsv");
            }
            return list;
        }
    }
}
