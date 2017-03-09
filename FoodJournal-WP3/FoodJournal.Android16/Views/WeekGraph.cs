
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
//using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using FoodJournal.Values;
using FoodJournal.Model;
using FoodJournal.Resources;
using FoodJournal.WinPhone.Common.Resources;
using FoodJournal.Logging;

namespace FoodJournal.Android15
{
    [Register("foodjournal.android15.WeekGraph")]
    public class WeekGraph : View
    {

        Property _property;
        public Property property
        {
            get
            {
                return _property;
            }
            set
            {
                _property = value;
                this.Invalidate();
            }
        }


        public EventHandler<DateTime> BarClick
        {
            get;
            set;
        }

        float lastTouchX = 0;

        public WeekGraph(Context context) :
            base(context)
        {
            Initialize();
        }

        public WeekGraph(Context context, Android.Util.IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public WeekGraph(Context context, Android.Util.IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        void Initialize()
        {
            Touch += (sender, e) =>
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    var pointX = e.Event.GetX();
                    var pointY = e.Event.GetY();
                    lastTouchX = pointX;

                }

                this.Clickable = true;
                Click += (objSender, evt) =>
                {
                    var targetBar = dateOfValues.Where(x => x.Value[0] < lastTouchX && x.Value[1] > lastTouchX).Select(a => a).FirstOrDefault();
                    if (targetBar.Value != null && BarClick != null)
                    {
                        BarClick(this, targetBar.Key);
                    }
                };
                //System.Diagnostics.Debug.WriteLine(targetBar.Key + "  -- " +string.Join(",", targetBar.Value));
            };
        }

        private Rect[] GetTextBounds(string[] texts, Paint paint, out Rect Largest)
        {
            Rect[] result = new Rect[texts.Length];
            Largest = new Rect(0, 0, 0, 0);
            for (int i = 0; i < texts.Length; i++)
            {
                result[i] = new Rect();
                paint.GetTextBounds(texts[i], 0, texts[i].Length, result[i]);
                result[i] = new Rect(0, 0, result[i].Width(), result[i].Height());
                if (result[i].Right > Largest.Right)
                    Largest.Right = result[i].Right;
                if (result[i].Bottom > Largest.Bottom)
                    Largest.Bottom = result[i].Bottom;
            }
            return result;
        }

        private new Single Top(Amount value, Amount max)
        {
            Single pct = value / max;
            if (pct > 1)
                pct = 1;
            return pct;//(int)(162 + -1 * pct * 150);
        }

        private byte MakeColor(byte refval, Single pct)
        {
            int val = refval + (byte)((255 - refval) * pct);
            return (byte)(val < 0 ? 0 : val > 255 ? 255 : val);
        }

        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);

            try
            {
                // 3000 -|
                //       |
                // 2000 -|
                //       |
                //    0 ---------------------------
                //         Tu We Th Fr Sa Su Mo Avg
                //  # Breakf  # Lunch  # Dinner  # Snack


                /*
            string[] yaxis = { "3.000", "2.250", "1.500", "750", "0" };
            string[] xaxis = { "Tue", "Wed", "Thu", "Fri", "Sat", "Sun", "Today", "Avg" };
            string[] legend = { "Breakfast", "Lunch", "Dinner", Property };//"Snack" };
            Single[][] values = new Single[4][];
            values [0] = new Single[]{ 0.1F, 0.13F, 0.2F, 0.03F, 0.12F, 0.13F, 0.12F, 0.03F };
            values [1] = new Single[]{ .3F, .2F, .3F, .2F, .3F, .2F, .3F, .2F };
            values [2] = new Single[]{ .2F, .3F, .2F, .3F, .2F, .3F, .2F, .3F };
            values [3] = new Single[]{ .3F, .2F, .3F, .2F, .3F, .2F, .3F, .2F };
            */
                string[] yaxis;
                string[] xaxis = new string[8];
                string[] legend;
                Single[][] values;

                Single goalBar;// = 0.7F;


                // calculate data
                Single goal = UserSettings.Current.GetGoal(property);

                Amount Goal;

                if (Single.IsNaN(goal) || goal == 0)
                    Goal = Amount.Zero;
                else
                    Goal = Amount.FromProperty(goal, property);

                Amount Max = Goal;

                HashSet<Period> UsedPeriods = new HashSet<Period>();

                int firstdate = -1;

                // query values
                for (int i = 0; i < 7; i++)
                {

                    DateTime date = DateTime.Now.Date.AddDays(-6 + i);

                    int j = 0;
                    Amount amount;
                    Amount total = Amount.Zero;
                    foreach (Period p in PeriodList.All)
                    {
                        amount = Cache.GetPeriodPropertyValue(date, p, property);
                        total += amount;
                        if (!amount.IsZero && !UsedPeriods.Contains(p))
                            UsedPeriods.Add(p);
                        j++;
                    }

                    if (!total.IsZero && firstdate == -1)
                        firstdate = i;

                    if (i == 6)
                        xaxis[i] = AppResources.Today;
                    else if (i >= firstdate)
                        xaxis[i] = date.ToString("ddd");
                    else
                        xaxis[i] = "";

                    if (Max < total)
                        Max = total;
                }

                xaxis[7] = "";

                // determine legend periods
                List<Period> DisplayPeriods = new List<Period>(); // in order
                foreach (Period p in PeriodList.All)
                    if (UsedPeriods.Contains(p))
                        DisplayPeriods.Add(p);

                goalBar = Top(Goal, Max);

                yaxis = new string[] {
					Max.ValueString (),
					(Max * .75).ValueString (),
					(Max * .5).ValueString (),
					(Max * .25).ValueString (),
					"0"
				};

                legend = new string[DisplayPeriods.Count];
                values = new float[DisplayPeriods.Count][];

                for (int j = 0; j < DisplayPeriods.Count; j++)
                {
                    Single sum = 0;
                    legend[j] = Strings.FromEnum(DisplayPeriods[j]);
                    values[j] = new float[8];

                    for (int i = 0; i < 7; i++)
                    {
                        DateTime date = DateTime.Now.Date.AddDays(-6 + i);
                        Amount amount = Cache.GetPeriodPropertyValue(date, DisplayPeriods[j], property);

                        values[j][i] = Top(amount, Max);
                        sum += values[j][i];
                    }
                    values[j][7] = (sum / (7 - firstdate));
                }

                /*
                Color[] colors = {
                    Color.Rgb (200, 150, 0),
                    Color.Rgb (239, 179, 0),
                    Color.Rgb (255, 202, 105),
                    Color.Rgb (255, 221, 173)
                };
                */

                Color[] colors; // = {					Resources.GetColor(Resource.Color.bar_color),					Resources.GetColor(Resource.Color.baralternate_color)				};

                int dcnt = DisplayPeriods.Count;

                colors = new Color[dcnt];
                for (float i = 0; i < dcnt; i++)
                {
                    Color refcolor = this.property.Color;// Resources.GetColor (Resource.Color.baralternate_color);
                    colors[(int)i] = new Color(MakeColor(refcolor.R, i / dcnt), MakeColor(refcolor.G, i / dcnt), MakeColor(refcolor.B, i / dcnt));
                }
                // start drawing

                Paint paint = new Paint();
                paint.Color = Color.Black;
                paint.TextSize = Resources.GetDimensionPixelSize(Resource.Dimension.graphtextsize);

                Paint faintpaint = new Paint();
                faintpaint.Color = Color.Rgb(230, 230, 230);// Color.LightGray;

                int middlemargin = Resources.GetDimensionPixelSize(Resource.Dimension.graphpadding); //10; // margin inbetween drawing regions only

                Rect ylargest;
                Rect xlargest;
                Rect llargest;
                Rect[] ybounds = GetTextBounds(yaxis, paint, out ylargest);
                Rect[] xbounds = GetTextBounds(xaxis, paint, out xlargest);
                Rect[] lbounds = GetTextBounds(legend, paint, out llargest);

                // if labels are rotated 45 degrees; height = sqrt((label width + labelhieght)^2/2)
                //int xaxisheight = Math.Sqrt (((xlargest.Width + xlargest.Height) ^ 2) / 2);
                int xaxisheight = xlargest.Height();

				Rect bounds = canvas.ClipBounds;
				Rect legendrect = new Rect(ylargest.Width() + middlemargin, bounds.Height() - llargest.Height(), bounds.Width() - middlemargin, bounds.Height() - middlemargin);
				Rect plotrect = new Rect(ylargest.Width() + middlemargin, ylargest.Height(), bounds.Width(), legendrect.Top - xaxisheight - (int)(middlemargin * 1.5) - ylargest.Height() / 2);
                Rect yaxisrect = new Rect(0, 0, ylargest.Width(), plotrect.Height() + llargest.Height());
                Rect xaxisrect = new Rect(plotrect.Left, plotrect.Bottom + middlemargin, plotrect.Right, legendrect.Top - (int)(middlemargin * 1.5));

                // draw avg background
                canvas.DrawRect(plotrect.Left + plotrect.Width() * (xaxis.Length - 1) / xaxis.Length, plotrect.Top, plotrect.Right, plotrect.Bottom, faintpaint);

                // canvas.DrawRect (legendrect, faintpaint);
                // canvas.DrawRect (xaxisrect, faintpaint);

                // y axis
                for (int i = 0; i < yaxis.Length; i++)
                {
                    // draw y axis text
                    Single y = plotrect.Top + plotrect.Height() * i / (yaxis.Length - 1);
                    canvas.DrawText(yaxis[i], yaxisrect.Right - ybounds[i].Width(), y + ybounds[i].Height() / 2, paint);
                    // draw notch
                    canvas.DrawLine(yaxisrect.Right + middlemargin / 2, y, plotrect.Left, y, paint);

                    // draw horizontal line
                    canvas.DrawLine(plotrect.Left, y, plotrect.Right, y, (i == yaxis.Length - 1) ? paint : faintpaint);
                }
                canvas.DrawLine(plotrect.Left, plotrect.Top, plotrect.Left, plotrect.Bottom + middlemargin / 2, paint);

                // x axis
                for (int i = 0; i < xaxis.Length; i++)
                {
                    // draw x axis text
                    Single x = xaxisrect.Left + plotrect.Width() * (i + 0.5F) / xaxis.Length;
                    canvas.DrawText(xaxis[i], x - xbounds[i].Width() / 2, xaxisrect.Bottom, paint);
                    // draw notch
                    x = xaxisrect.Right - plotrect.Width() * i / xaxis.Length - 1;
                    canvas.DrawLine(x, plotrect.Bottom, x, plotrect.Bottom + middlemargin / 2, paint);
                }

                // legend
                int cubesize = middlemargin;
                int legendw = (cubesize + middlemargin * 2) * legend.Length;
                for (int i = 0; i < legend.Length; i++)
                    legendw += lbounds[i].Right;

                legendrect.Left += (legendrect.Width() - legendw) / 2;

                Paint colorPaint = new Paint();
                int dx = 0;
                for (int i = 0; i < legend.Length; i++)
                {
                    // cube
                    colorPaint.Color = colors[i % colors.Length];
                    Single x = legendrect.Left + dx; // legendrect.Width () * i / legend.Length;
                    Single y = legendrect.Top + legendrect.Height() / 2;
                    canvas.DrawRect(x, y - (int)(cubesize / 2.5), x + cubesize, y + (int)(cubesize / 1.5), colorPaint);
                    // legend text
                    canvas.DrawText(legend[i], x + cubesize + middlemargin / 2, y + (int)(cubesize / 1.5), paint);
                    dx += cubesize + middlemargin * 2 + lbounds[i].Right;
                }
                dateOfValues.Clear();
                int barwidth = middlemargin * 2;
                Single[] bottoms = new Single[xaxis.Length];
                for (int i = 0; i < values.Length; i++)
                {

                    colorPaint.Color = colors[i % colors.Length];

                    for (int j = 0; j < values[i].Length; j++)
                    {

                        DateTime date = DateTime.Now.Date.AddDays(-6 + j);

                        if (i == 0)
                            bottoms[j] = plotrect.Bottom;

                        Single x = plotrect.Left + plotrect.Width() * (j + 0.5F) / xaxis.Length;
                        Single h = plotrect.Height() * values[i][j];
                        var dLeft = x - barwidth / 2;
                        var dTop = bottoms[j] - h;
                        var dRight = x + barwidth / 2;
                        var dBottom = bottoms[j];
                        if (dateOfValues.Count < 7)
                            dateOfValues.Add(new KeyValuePair<DateTime, float[]>(date, new float[2] { dLeft, dRight }));
                        canvas.DrawRect(dLeft, dTop, dRight, dBottom, colorPaint);

                        bottoms[j] -= h;

                    }
                }
                foreach (var item in dateOfValues)
                {
                    System.Diagnostics.Debug.WriteLine(item.Key.Day + "  " + item.Value[0] + "-" + item.Value[1]);
                }
                // draw goal
                Paint dashespaint = new Paint();
                dashespaint.Color = Color.DarkGray;
                dashespaint.SetStyle(Paint.Style.Stroke);
                dashespaint.SetPathEffect(new DashPathEffect(new float[] { 10, 5 }, 0));
                //canvas.DrawLine (plotrect.Left, plotrect.Bottom - plotrect.Height () * goalBar, plotrect.Right, plotrect.Bottom - plotrect.Height () * goalBar, dashespaint);

                // use a path to draw dashes
                Path baseline = new Path();
                baseline.MoveTo(plotrect.Left, plotrect.Bottom - plotrect.Height() * goalBar);
                baseline.LineTo(plotrect.Right, plotrect.Bottom - plotrect.Height() * goalBar);
                canvas.DrawPath(baseline, dashespaint);

            }
            catch (Exception ex)
            {
                LittleWatson.ReportException(ex);
            }
        }
        List<KeyValuePair<DateTime, float[]>> dateOfValues = new List<KeyValuePair<DateTime, float[]>>();
    }
}



