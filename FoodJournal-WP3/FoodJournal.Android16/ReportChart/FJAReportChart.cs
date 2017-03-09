using System;
using System.Collections.Generic;
using System.Linq;
using AChartEngine.Chart;
using AChartEngine.Model;
using AChartEngine.Renderer;
using Android.Graphics;
using Android.Graphics.Drawables;
using FoodJournal.Values;
using Android.Content;

namespace FoodJournal.Android15.ReportChart
{
    public class FJAReportChart : BarChart
    {
        public FJABarChartPropertyModel Property { get; set; }
        public Context Context { get; set; }
        public FJAReportChart(XYMultipleSeriesDataset dataset, XYMultipleSeriesRenderer renderer, Type type, FJABarChartPropertyModel property, Context ctx)
            : base(dataset, renderer, type)
        {
            Property = property;
            Context = ctx;
        }

        public override void DrawSeries(Canvas canvas, Paint paint, IList<Java.Lang.Float> points,
      SimpleSeriesRenderer seriesRenderer, float yAxisValue, int seriesIndex, int startIndex)
        {
            int seriesNr = MDataset.SeriesCount;
            int length = points.Count;
            var pts = points.Select(s => s.FloatValue()).ToList();

            paint.SetStyle(Paint.Style.Fill);
            float halfDiffX = GetHalfDiffX(points, length, seriesNr);
            for (int i = 0, z = 0; i < length; i += 2, z++)
            {
                float x = points[i].LongValue();
                float y = points[i + 1].LongValue();
                var day = Property.Days[z];

                DrawCustomBar(canvas, x, yAxisValue, x, y, halfDiffX, seriesNr, seriesIndex, paint, day);
            }
        }

        protected void DrawCustomBar(Canvas canvas, float xMin, float yMin, float xMax, float yMax, float halfDiffX, int seriesNr, int seriesIndex, Paint paint, FJABarChartDayModel day)
        {
            int scale = this.MDataset.GetSeriesAt(seriesIndex).ScaleNumber;

            float startX = xMin - (float)seriesNr * halfDiffX + (float)(seriesIndex * 2) * halfDiffX;
            this.drawBar(canvas, startX, yMax, startX + 2.0f * halfDiffX, yMin, scale, seriesIndex, paint, day);
        }

        private byte MakeColor(byte refval, Single pct)
        {
            int val = refval + (byte)((255 - refval) * pct);
            return (byte)(val < 0 ? 0 : val > 255 ? 255 : val);
        }
        private void drawBar(Canvas canvas, float xMin, float yMin, float xMax, float yMax, int scale, int seriesIndex, Paint paint, FJABarChartDayModel day)
        {
            // select amounts for all periods of the day for which we are going to draw a bar
            var amounts = day.Amounts.OrderByDescending(a => (int)a.Key).Select(a => a);
            // select how contribution of each period from total gain amount of the day
            var portions = amounts.Select(x => (x.Value * 100) / day.TotalAmount).ToList();

            if (portions.Any(a => !float.IsNaN(a)))
            {
                int dcnt = day.Amounts.Count;
                Color refcolor = Context.Resources.GetColor(Resource.Color.baralternate_color);
                var colors = new Color[dcnt];
                for (float i = 0; i < dcnt; i++)
                    colors[(int)i] = new Color(MakeColor(refcolor.R, i / dcnt), MakeColor(refcolor.G, i / dcnt), MakeColor(refcolor.B, i / dcnt));
                colors = colors.Reverse().ToArray();

                //set max value of y-axis
                MRenderer.YAxisMax = day.TotalAmount.ToSingle() < 500 ? 500 : day.TotalAmount.ToSingle();
                Renderer.YAxisMax = day.TotalAmount.ToSingle() < 500 ? 500 : day.TotalAmount.ToSingle();

                if (Math.Abs(yMin - yMax) < 1.0f)
                    yMax = yMin < yMax ? yMin + 1.0f : yMin - 1.0f;

                var startPoint = yMin;
                var avg = yMax - yMin;
                int c = 0;
                foreach (var item in portions)
                {
                    paint.Color = colors[c];
                    var currentTartgetPoint = ((avg * item) / 100) + startPoint;

                    canvas.DrawRect((float)Math.Round(xMin),
                        (float)Math.Round(startPoint), (float)Math.Round(xMax),
                        (float)Math.Round(currentTartgetPoint), paint);
                    startPoint = currentTartgetPoint;
                    c++;
                }
            }

        }

    }
}