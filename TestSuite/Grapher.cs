using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

using Mediation.FileIO;
using Mediation.Utilities;
using System.IO;

namespace TestSuite
{
    public class Grapher
    {
        // Generates an excel spreadsheet with graphs of each summary element by depth and saves each graph as a PNG file.
        public static void CreateGTGraphs(string domainName, string timeStamp, int depth, List<List<Tuple<String, String>>> summaries)
        {
            // The top level log directory.
            string directory = Parser.GetTopDirectory() + @"GameTrees\" + domainName + @"\" + timeStamp + @"\";

            // The graph image directory.
            string imageDir = directory + @"graphs\";

            // Check if the image directory exists.
            if (!File.Exists(imageDir))
                // If not, create it.
                Directory.CreateDirectory(imageDir);

            // This is needed for a lot of the Excel initialization tasks.
            object misValue = System.Reflection.Missing.Value;

            // Create an Excel application object.
            Excel.Application xl = new Excel.Application();

            // Create an Excel work sheet variable.
            Excel.Worksheet xlWorkSheet;

            // Create an Excel work books variable and point it at the application object's work books..
            Excel.Workbooks xlWorkBooks = xl.Workbooks;

            // Make the Excel application visible? Not sure, it's magic.
            xl.Visible = true;

            // Open the summary CSV file in the Excel work book.
            xlWorkBooks.OpenText(directory + "summary.csv", misValue, misValue, Excel.XlTextParsingType.xlDelimited,
                                     Excel.XlTextQualifier.xlTextQualifierNone, misValue, misValue,
                                     misValue, misValue, misValue, misValue, misValue, misValue, misValue,
                                     misValue, misValue, misValue, misValue);

            // Grab the work sheet that represents the CSV file.
            xlWorkSheet = (Excel.Worksheet)xlWorkBooks[1].Worksheets.get_Item(1);

            // Loop through every summary element, excluding the depth count which should be first.
            for (int summary = 1; summary < summaries[0].Count; summary++)
            {
                // Create a new Excel chart holder in the work sheet.
                Excel.ChartObjects xlCharts = (Excel.ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);

                // Create a new Excel chart and position it below the table and other charts.
                Excel.ChartObject myChart = (Excel.ChartObject)xlCharts.Add(10, (15 * summaries[0].Count) + (220 * (summary - 1)), 360, 210);

                // Select the chart object's chart. Don't ask me, this Excel interface is strange.
                Excel.Chart chartPage = myChart.Chart;

                // Make the chart object active.
                myChart.Select();

                // Set the chart's style. 227 should be a white background with a blue line.
                chartPage.ChartStyle = 227;

                // Set the type of chart. We are using a line chart.
                chartPage.ChartType = Excel.XlChartType.xlLine;

                // Turn the legend off.
                chartPage.HasLegend = false;

                // Create a new series collection to hold the series that will contain the chart data. Again, Excel's interface is dumb.
                Excel.SeriesCollection seriesCollection = (Excel.SeriesCollection)chartPage.SeriesCollection();

                // Create a new series.
                Excel.Series series1 = seriesCollection.NewSeries();

                // Select the chart's X values. These should be the depth counts.
                series1.XValues = xlWorkSheet.Range["summary!$A$2:$A$" + depth];

                // Select the chart's Y values. These should be the current summary element data.
                series1.Values = xlWorkSheet.Range["summary!$" + ToLetter(summary) + "$2:$" + ToLetter(summary) + "$" + depth];

                // Name the chart according to the current summary element.
                series1.Name = summaries[0][summary].First;

                // Export the current chart as a PNG image.
                chartPage.Export(imageDir + series1.Name + ".png", "PNG", false);
            }

            // Save the current work book as an XLS file.
            xlWorkBooks[1].SaveAs(directory + "graphsummary-" + timeStamp + ".xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);

            // Close the current work book.
            xlWorkBooks[1].Close(true, misValue, misValue);

            // Quit the Excel application.
            xl.Quit();

            // Do some garbage collection? Not sure, all examples I saw had this.
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBooks);
            releaseObject(xl);
        }

        // Generates an excel spreadsheet with graphs of each summary element by depth and saves each graph as a PNG file.
        public static void CreateGraphs (string domainName, string timeStamp, int depth, string directory, List<List<Tuple<String, String>>> summaries)
        {
            directory += timeStamp + @"\";

            // The graph image directory.
            string imageDir = directory + @"graphs\";

            // Check if the image directory exists.
            if (!File.Exists(imageDir))
                // If not, create it.
                Directory.CreateDirectory(imageDir);

            // This is needed for a lot of the Excel initialization tasks.
            object misValue = System.Reflection.Missing.Value;

            // Create an Excel application object.
            Excel.Application xl = new Excel.Application();

            // Create an Excel work sheet variable.
            Excel.Worksheet xlWorkSheet;

            // Create an Excel work books variable and point it at the application object's work books..
            Excel.Workbooks xlWorkBooks = xl.Workbooks;

            // Make the Excel application visible? Not sure, it's magic.
            xl.Visible = true;

            // Open the summary CSV file in the Excel work book.
            xlWorkBooks.OpenText(directory + "summary.csv", misValue, misValue, Excel.XlTextParsingType.xlDelimited,
                                     Excel.XlTextQualifier.xlTextQualifierNone, misValue, misValue,
                                     misValue, misValue, misValue, misValue, misValue, misValue, misValue,
                                     misValue, misValue, misValue, misValue);

            // Grab the work sheet that represents the CSV file.
            xlWorkSheet = (Excel.Worksheet)xlWorkBooks[1].Worksheets.get_Item(1);

            // Loop through every summary element, excluding the depth count which should be first.
            for (int summary = 1; summary < summaries[0].Count; summary++)
            {
                // Create a new Excel chart holder in the work sheet.
                Excel.ChartObjects xlCharts = (Excel.ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);

                // Create a new Excel chart and position it below the table and other charts.
                Excel.ChartObject myChart = (Excel.ChartObject)xlCharts.Add(10, (15 * summaries[0].Count) + (220 * (summary - 1)), 360, 210);

                // Select the chart object's chart. Don't ask me, this Excel interface is strange.
                Excel.Chart chartPage = myChart.Chart;

                // Make the chart object active.
                myChart.Select();

                // Set the chart's style. 227 should be a white background with a blue line.
                chartPage.ChartStyle = 227;

                // Set the type of chart. We are using a line chart.
                chartPage.ChartType = Excel.XlChartType.xlLine;

                // Turn the legend off.
                chartPage.HasLegend = false;

                // Create a new series collection to hold the series that will contain the chart data. Again, Excel's interface is dumb.
                Excel.SeriesCollection seriesCollection = (Excel.SeriesCollection)chartPage.SeriesCollection();

                // Create a new series.
                Excel.Series series1 = seriesCollection.NewSeries();

                // Select the chart's X values. These should be the depth counts.
                series1.XValues = xlWorkSheet.Range["summary!$A$2:$A$" + depth];

                // Select the chart's Y values. These should be the current summary element data.
                series1.Values = xlWorkSheet.Range["summary!$" + ToLetter(summary) + "$2:$" + ToLetter(summary) + "$" + depth];

                // Name the chart according to the current summary element.
                series1.Name = summaries[0][summary].First;

                // Export the current chart as a PNG image.
                chartPage.Export(imageDir + series1.Name + ".png", "PNG", false);
            }

            // Save the current work book as an XLS file.
            xlWorkBooks[1].SaveAs(directory + "graphsummary-" + timeStamp + ".xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);

            // Close the current work book.
            xlWorkBooks[1].Close(true, misValue, misValue);

            // Quit the Excel application.
            xl.Quit();

            // Do some garbage collection? Not sure, all examples I saw had this.
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBooks);
            releaseObject(xl);
        }

        // Looks like this manually asks the garbage collector to clean up after the Excel application. I'm keeping it because all examples have it.
        private static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }

        // Given an integer, returns the corresponding letter of the alphabet.
        static string ToLetter(int index)
        {
            // The capital letters of the alphabet.
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            // The letter we will return.
            var value = "";

            // Check if we're higher than Z.
            if (index >= letters.Length)
                // If we are, move the request back inside the alphabet's bounds.
                value += letters[index / letters.Length - 1];

            // Grab the correct value.
            value += letters[index % letters.Length];

            // Return the letter.
            return value;
        }
    }
}
