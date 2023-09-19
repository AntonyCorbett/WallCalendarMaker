using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using Svg;
using WallCalendarMakerCore.CommonDocuments;
using WallCalendarMakerCore.Holidays;

namespace WallCalendarMakerCore;

public class Maker : IMaker
{
    public MakerOptions Options { get; } = new();

    public Maker(Action<MakerOptions>? action = null)
    {
        action?.Invoke(Options);
    }

    public async Task GenerateAsync(string path, bool useBom = true)
    {
        (await GenerateAsync()).Write(path, useBom);
    }

    public async Task<SvgDocument> GenerateAsync()
    {
        var doc = CreateBlankDocument();

        var boxGroup = new SvgGroup { ID = "BoxGroup" };

        var numRows = GetRowCount();

        var dayNameHeightMillimeters = GetDayNameHeightAllowanceMillimeters();
        var monthNameHeightMillimeters = GetMonthNameHeightAllowanceMillimeters();

        var outlineBoxGroupWidth = doc.Width.Value - (2 * Options.XMarginMillimeters);

        var outlineBoxGroupHeight =
            doc.Height.Value -
            (2 * Options.YMarginMillimeters) -
            dayNameHeightMillimeters -
            monthNameHeightMillimeters;

        var outlineBox = CreateOutlineBox(outlineBoxGroupWidth, outlineBoxGroupHeight, dayNameHeightMillimeters + monthNameHeightMillimeters);

        doc.Children.Add(outlineBox);

        var individualBoxWidth = outlineBoxGroupWidth / 7;
        var individualBoxHeight = outlineBoxGroupHeight / numRows;
        var individualBoxMargin = individualBoxHeight / 20;
        var marginAdjustment = individualBoxMargin / 2;

        // draw day boxes...
        var boxNum = 1;
        foreach (var row in Enumerable.Range(0, numRows))
        {
            foreach (var col in Enumerable.Range(0, 7))
            {
                var x = outlineBox.X.Value + marginAdjustment + (col * individualBoxWidth);
                var y = outlineBox.Y.Value + marginAdjustment + (row * individualBoxHeight);

                var boxId = $"Box{boxNum++}";
                var box = CreateSingleBox(x, y, individualBoxWidth, individualBoxHeight, individualBoxMargin, boxId);

                // some custom attributes
                box.CustomAttributes.Add(BoxAttributes.Column, col.ToString());
                box.CustomAttributes.Add(BoxAttributes.Row, row.ToString());

                boxGroup.Children.Add(box);
            }
        }

        doc.Children.Add(boxGroup);

        DrawMonthAndYear(doc);
        DrawDayNames(doc, boxGroup);
        DrawDayNumbers(doc, boxGroup, individualBoxMargin);
        SpecifyLiveBoxOpacity(boxGroup);
        SpecifyDeadBoxOpacity(boxGroup);
        SpecifyBoxCorners(boxGroup);
        await DrawHolidaysAsync(doc, boxGroup);

        return doc;
    }

    private async Task DrawHolidaysAsync(SvgDocument doc, SvgGroup boxGroup)
    {
        if (!Options.DrawHolidays)
        {
            return;
        }

        var holidays = await GetHolidaysInMonthAsync();
        var holidayNum = 1;
        foreach (var holiday in holidays)
        {
            var box = (SvgRectangle?)boxGroup.Children.SingleOrDefault(x =>
                TryGetBoxDayNumber((SvgRectangle)x, out var dayNum) && dayNum == holiday.Date.Day);

            if (box != null)
            {
                var s = (holiday.Title + (string.IsNullOrWhiteSpace(holiday.Notes) ? "" : $" ({holiday.Notes})"))
                    .Trim();

                var text = new SvgText(s)
                {
                    ID = $"Holiday{holidayNum++}",
                    Font = Options.HolidaysFont.Name,
                    FontSize = new SvgUnit(SvgUnitType.Point, Options.HolidaysFont.PointSize),
                    FontStyle = Options.HolidaysFont.Italic ? SvgFontStyle.Italic : SvgFontStyle.Normal,
                    FontWeight = Options.HolidaysFont.Bold ? SvgFontWeight.Bold : SvgFontWeight.Normal,
                    Fill = new SvgColourServer(Options.HolidaysFont.Color),
                    X = new SvgUnitCollection { new(SvgUnitType.Millimeter, box.X.Value + PointSizeToMillimeters(Options.HolidaysFont.PointSize)/2)},
                    Y = new SvgUnitCollection { new(SvgUnitType.Millimeter, box.Y.Value + box.Height.Value - 1) }
                };

                doc.Children.Add(text);
            }
        }
    }

    private async Task<IEnumerable<HolidaysService.AnEvent>> GetHolidaysInMonthAsync()
    {
        var holidaysService = new HolidaysService();
        var holidays = await holidaysService.ExecuteAsync();
        if (holidays == null)
        {
            return Enumerable.Empty<HolidaysService.AnEvent>();
        }

        return holidays.EnglandAndWales.Events.Where(x =>
            x.Date.Year == Options.MonthDefinition.Year && x.Date.Month == Options.MonthDefinition.Month);
    }

    private void SpecifyBoxCorners(SvgGroup boxGroup)
    {
        foreach (var box in boxGroup.Children.OfType<SvgRectangle>().Where(IsLiveBox))
        {
            switch (Options.BoxCornerMode)
            {
                case BoxCornerMode.Normal:
                case BoxCornerMode.Merge:
                    break;

                case BoxCornerMode.Rounded1:
                    var radius1 = box.Width / 20;
                    box.CornerRadiusX = radius1;
                    box.CornerRadiusY = radius1;
                    break;

                case BoxCornerMode.Rounded2:
                    var radius2 = box.Width / 16;
                    box.CornerRadiusX = radius2;
                    box.CornerRadiusY = radius2;
                    break;

                case BoxCornerMode.Rounded3:
                    var radius3 = box.Width / 12;
                    box.CornerRadiusX = radius3;
                    box.CornerRadiusY = radius3;
                    break;

                case BoxCornerMode.Rounded4:
                    var radius4 = box.Width / 8;
                    box.CornerRadiusX = radius4;
                    box.CornerRadiusY = radius4;
                    break;


                default:
                    throw new NotSupportedException();
            }
        }
    }

#pragma warning disable U2U1003 // Avoid declaring methods used in delegate constructors static
    private static bool IsLiveBox(SvgRectangle box) => TryGetBoxDayNumber(box, out _);
#pragma warning restore U2U1003 // Avoid declaring methods used in delegate constructors static

    private float GetDayNameHeightAllowanceMillimeters()
    {
        return PointSizeToMillimeters(Options.DayNamesFont.PointSize);
    }

    private static float PointSizeToMillimeters(float pointSize)
    {
        return (float)((pointSize * 25.4) / 72.0);
    }

    private static float UnitSizeToMillimeters(float unitSize)
    {
        return (float) (unitSize * 25.4) / 96.0F;
    }

    private float GetMonthNameHeightAllowanceMillimeters()
    {
        if (Options is {DrawMonth: false, DrawYear: false})
        {
            return 0;
        }

        var largestPointSize = Math.Max(
            Options.DrawMonth ? Options.MonthFont.PointSize : 0,
            Options.DrawYear ? Options.YearFont.PointSize : 0);

        return 2 * PointSizeToMillimeters(largestPointSize);
    }

    private void SpecifyLiveBoxOpacity(SvgGroup boxGroup)
    {
        foreach (var box in boxGroup.Children.OfType<SvgRectangle>().Where(IsLiveBox))
        {
            SetLiveBoxElementOpacity(box);
        }
    }

    private void SetLiveBoxElementOpacity(SvgElement element)
    {
        switch (Options.LiveBoxMode)
        {
            case LiveBoxMode.Visible:
                // do nothing
                break;
            case LiveBoxMode.Opacity5:
                element.Opacity = 0.05F;
                break;
            case LiveBoxMode.Opacity10:
                element.Opacity = 0.1F;
                break;
            case LiveBoxMode.Opacity25:
                element.Opacity = 0.25F;
                break;
            case LiveBoxMode.Opacity50:
                element.Opacity = 0.50F;
                break;
            case LiveBoxMode.Opacity75:
                element.Opacity = 0.75F;
                break;
            case LiveBoxMode.Opacity85:
                element.Opacity = 0.85F;
                break;
            case LiveBoxMode.Opacity95:
                element.Opacity = 0.95F;
                break;
            default:
                throw new NotSupportedException();
        }
    }

    private void SpecifyDeadBoxOpacity(SvgGroup boxGroup)
    {
        foreach (var box in boxGroup.Children.OfType<SvgRectangle>())
        {
            if (!IsLiveBox(box))
            {
                switch (Options.DeadBoxMode)
                {
                    case DeadBoxMode.Visible:
                        // do nothing
                        break;
                    case DeadBoxMode.Invisible:
                        box.Display = "none";
                        break;
                    case DeadBoxMode.Opacity25:
                        box.Opacity = 0.25F;
                        break;
                    case DeadBoxMode.Opacity50:
                        box.Opacity = 0.50F;
                        break;
                    case DeadBoxMode.Opacity75:
                        box.Opacity = 0.75F;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }

    private static int GetBoxColumn(SvgRectangle box) => GetBoxIntegerValue(box, BoxAttributes.Column);

    private static int GetBoxRow(SvgRectangle box) => GetBoxIntegerValue(box, BoxAttributes.Row);

    private static bool TryGetBoxDayNumber(SvgRectangle box, out int result)
    {
        var rv = TryGetBoxIntegerValue(box, BoxAttributes.DayNumber, out var r);
        result = r;
        return rv;
    }

    private static bool TryGetBoxDayOfWeek(SvgRectangle box, out int result)
    {
        var rv = TryGetBoxIntegerValue(box, BoxAttributes.DayOfWeek, out var r);
        result = r;
        return rv;
    }

    private static int GetBoxIntegerValue(SvgRectangle box, string attributeKey)
    {
        if (!box.TryGetAttribute(attributeKey, out var value) ||
            !int.TryParse(value, out var val))
        {
            throw new NotSupportedException("Could not find box attribute!");
        }

        return val;
    }

    private static bool TryGetBoxIntegerValue(SvgRectangle box, string attributeKey, out int result)
    {
        if (box.TryGetAttribute(attributeKey, out var valueStr) &&
            int.TryParse(valueStr, out var val2))
        {
            result = val2;
            return true;
        }

        result = 0;
        return false;
    }

    private int GetRowCount()
    {
        switch (Options.RowMode)
        {
            case RowMode.FiveRows:
                return 5;
            case RowMode.SixRows:
                return 6;
            default:
                throw new NotSupportedException();
        }
    }

    private void DrawDayNumbers(SvgDocument doc, SvgGroup boxGroup, float boxMarginMillimeters)
    {
        var group = new SvgGroup { ID = "DayNumberGroup" };

        var dayNumber = 0;

#pragma warning disable S6562
        var firstDate = new DateTime(Options.MonthDefinition.Year, Options.MonthDefinition.Month, 1);
        var lastDate = firstDate.AddMonths(1).AddDays(-1);
#pragma warning restore S6562

        foreach (var box in boxGroup.Children.OfType<SvgRectangle>())
        {
            var col = GetBoxColumn(box);
            var dayOfWeek = CalculateDayOfWeek(col);

            if (dayNumber == 0 && firstDate.DayOfWeek == dayOfWeek)
            {
                dayNumber = 1;
            }

            if (dayNumber > 0)
            {
                if (dayNumber <= lastDate.Day)
                {
                    var numberText = CreateDayNumberText(dayNumber, box, boxMarginMillimeters, false);
                    group.Children.Add(numberText);

                    // specify box attributes
                    box.CustomAttributes.Add(BoxAttributes.DayNumber, dayNumber.ToString());
                    box.CustomAttributes.Add(BoxAttributes.DayOfWeek, dayOfWeek.ToString());
                }

                ++dayNumber;
            }
        }

        if (dayNumber <= lastDate.Day)
        {
            Debug.Assert(Options.RowMode == RowMode.FiveRows);
            // any overflow days (only ever an issue in "5 row" mode) must be accommodated in the last row...

            var lastRow = boxGroup.Children.OfType<SvgRectangle>().TakeLast(7).ToArray();
            var index = 0;

            while (dayNumber <= lastDate.Day)
            {
                var box = lastRow[index];

                var numberText = CreateDayNumberText(dayNumber, box, boxMarginMillimeters, true);
                group.Children.Add(numberText);

                DrawCentreLineInOverflowBox(box, doc);

                ++index;
                ++dayNumber;
            }
        }

        doc.Children.Add(group);
    }

    private void DrawCentreLineInOverflowBox(SvgRectangle box, SvgDocument doc)
    {
        var line = new SvgLine
        {
            StartX = box.X,
            EndX = box.X + box.Width,
            StartY = box.Y + box.Height / 2,
            EndY = box.Y + box.Height / 2,
            Stroke = new SvgColourServer(Color.Black),
            StrokeWidth = new SvgUnit(SvgUnitType.Millimeter, 0.2f),
        };

        SetLiveBoxElementOpacity(line);

        doc.Children.Add(line);
    }

    private SvgText CreateDayNumberText(
        int dayNumber, SvgRectangle box, float boxMarginMillimeters, bool isOverflow)
    {
        var numberText = new SvgText(dayNumber.ToString())
        {
            Font = Options.NumbersFont.Name,
            FontSize = new SvgUnit(SvgUnitType.Point, Options.NumbersFont.PointSize),
            FontStyle = Options.NumbersFont.Italic ? SvgFontStyle.Italic : SvgFontStyle.Normal,
            FontWeight = Options.NumbersFont.Bold ? SvgFontWeight.Bold : SvgFontWeight.Normal,
            TextAnchor = SvgTextAnchor.End
        };

        var extraMargin = CalculateExtraMarginForRoundedCorners(box.Width.Value);

        numberText.ID = $"DayNumber{dayNumber}";
        numberText.X = new SvgUnitCollection
        {
            new SvgUnit(SvgUnitType.Millimeter,
                box.X.Value + box.Width.Value -
                boxMarginMillimeters -
                extraMargin)
        };

        numberText.Y = new SvgUnitCollection
        {
            new SvgUnit(SvgUnitType.Millimeter, 
                box.Y.Value + 
                extraMargin +
                (float)(0.9 * PointSizeToMillimeters(Options.NumbersFont.PointSize)) + 
                (isOverflow ? box.Height.Value / 2 : 0))
        };

        return numberText;
    }

    private float CalculateExtraMarginForRoundedCorners(float boxWidthMillimeters)
    {
        switch (Options.BoxCornerMode)
        {
            case BoxCornerMode.Normal:
            case BoxCornerMode.Merge:
                return 0;
            case BoxCornerMode.Rounded1:
                return boxWidthMillimeters / 60;
            case BoxCornerMode.Rounded2:
                return boxWidthMillimeters / 50;
            case BoxCornerMode.Rounded3:
                return boxWidthMillimeters / 40;
            case BoxCornerMode.Rounded4:
                return boxWidthMillimeters / 30;

            default:
                throw new NotSupportedException();
        }
    }

    private void DrawMonthAndYear(SvgDocument doc)
    {
        if (Options is {DrawMonth: false, DrawYear: false})
        {
            return;
        }

        var monthString = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[Options.MonthDefinition.Month - 1];

        var dt = new DateTime(Options.MonthDefinition.Year, Options.MonthDefinition.Month, 1, 0, 0, 0, DateTimeKind.Local);
        var yearString = dt.ToString("yyyy", CultureInfo.CurrentCulture);

        var sMonth = new SvgTextSpan
        {
            Text = monthString + (Options.DrawYear ? " " : ""),
            ID = "MonthName",
            Font = Options.MonthFont.Name,
            FontSize = new SvgUnit(SvgUnitType.Point, Options.MonthFont.PointSize),
            FontStyle = Options.MonthFont.Italic ? SvgFontStyle.Italic : SvgFontStyle.Normal,
            FontWeight = Options.MonthFont.Bold ? SvgFontWeight.Bold : SvgFontWeight.Normal,
        };

        var sYear = new SvgTextSpan
        {
            Text = yearString,
            ID = "Year",
            Font = Options.YearFont.Name,
            FontSize = new SvgUnit(SvgUnitType.Point, Options.YearFont.PointSize),
            FontStyle = Options.YearFont.Italic ? SvgFontStyle.Italic : SvgFontStyle.Normal,
            FontWeight = Options.YearFont.Bold ? SvgFontWeight.Bold : SvgFontWeight.Normal,
        };

        var s = new SvgText
        {
            X = new SvgUnitCollection { new(SvgUnitType.Millimeter, doc.Width.Value / 2)},
            Y = new SvgUnitCollection { new(SvgUnitType.Millimeter, PointSizeToMillimeters(Options.MonthFont.PointSize) + Options.YMarginMillimeters) },
            TextAnchor = SvgTextAnchor.Middle
        };

        if (Options.DrawMonth)
        {
            s.Children.Add(sMonth);
        }

        if (Options.DrawYear)
        {
            s.Children.Add(sYear);
        }

        doc.Children.Add(s);
    }

    private void DrawDayNames(SvgDocument doc, SvgGroup boxGroup)
    {
        var group = new SvgGroup { ID = "DayNameGroup" };

        var ySpacer = (float)(PointSizeToMillimeters(Options.DayNamesFont.PointSize) / 1.25);

        foreach (var box in boxGroup.Children.Take(7).OfType<SvgRectangle>())
        {
            var col = GetBoxColumn(box);
            var dayOfWeek = (int)CalculateDayOfWeek(col);
            var dayOfWeekString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[dayOfWeek];

            var s = new SvgText(dayOfWeekString)
            {
                ID = $"DayName{dayOfWeek}", // do not localize day name here
                Font = Options.DayNamesFont.Name,
                FontSize = new SvgUnit(SvgUnitType.Point, Options.DayNamesFont.PointSize),
                FontStyle = Options.DayNamesFont.Italic ? SvgFontStyle.Italic : SvgFontStyle.Normal,
                FontWeight = Options.DayNamesFont.Bold ? SvgFontWeight.Bold : SvgFontWeight.Normal,

                X = new SvgUnitCollection { new(SvgUnitType.Millimeter, box.X.Value + box.Width.Value / 2) },
                Y = new SvgUnitCollection { new(SvgUnitType.Millimeter, box.Y.Value - ySpacer) },

                TextAnchor = SvgTextAnchor.Middle
            };

            //var w1 = UnitSizeToMillimeters(s.Bounds.Width);
            //var y = new SvgUnit(SvgUnitType.Millimeter, s.Y[0].Value + 3);
            //var line = new SvgLine
            //{
            //    StartX = new SvgUnit(SvgUnitType.Millimeter, s.X[0].Value - w1/2),
            //    EndX = new SvgUnit(SvgUnitType.Millimeter, s.X[0].Value + w1/2),
            //    StartY = y,
            //    EndY = y,
            //    StrokeWidth = new SvgUnit(SvgUnitType.Millimeter, 1),
            //    Stroke = new SvgColourServer(Color.Black)
            //};

            //doc.Children.Add(line);
            
        group.Children.Add(s);
        }

        doc.Children.Add(group);
    }

    private DayOfWeek CalculateDayOfWeek(int col)
    {
        return (DayOfWeek)(((int) Options.MonthDefinition.FirstDayOfWeek + col) % 7);
    }

    private SvgRectangle CreateSingleBox(
        float x, float y, float widthMillimeters, float heightMillimeters, float boxMargin, string id)
    {
        if (Options.BoxCornerMode == BoxCornerMode.Merge)
        {
            x -= boxMargin/2;
            y -= boxMargin/2;

            boxMargin = 0;
        }

        return new SvgRectangle
        {
            ID = id,
            X = new SvgUnit(SvgUnitType.Millimeter, x),
            Y = new SvgUnit(SvgUnitType.Millimeter, y),
            Width = new SvgUnit(SvgUnitType.Millimeter, widthMillimeters - boxMargin),
            Height = new SvgUnit(SvgUnitType.Millimeter, heightMillimeters - boxMargin),
            Fill = new SvgColourServer(Color.White),
            Stroke = new SvgColourServer(Color.Black),
            StrokeWidth = new SvgUnit(SvgUnitType.Millimeter, 0.2f),
        };
    }

    private SvgRectangle CreateOutlineBox(float widthMillimeters, float heightMillimeters, float headerHeightMillimeterAllowance)
    {
        var result = new SvgRectangle
        {
            ID = "OutlineBox",
            X = new SvgUnit(SvgUnitType.Millimeter, Options.XMarginMillimeters),
            Y = new SvgUnit(SvgUnitType.Millimeter, Options.YMarginMillimeters + headerHeightMillimeterAllowance),
            Width = new SvgUnit(SvgUnitType.Millimeter, widthMillimeters),
            Height = new SvgUnit(SvgUnitType.Millimeter, heightMillimeters),
            Fill = new SvgColourServer(System.Drawing.Color.DeepSkyBlue),
        };

        if (!Options.DrawOutlineBox)
        {
            result.Display = "none";
        }

        return result;
    }

    private SvgDocument CreateBlankDocument()
    {
        switch (Options.PageSize)
        {
            case PageSize.A3:
                return new A3LandscapeDocument(Options.DrawMargin, Options.XMarginMillimeters, Options.YMarginMillimeters);

            case PageSize.A4:
                return new A4LandscapeDocument(Options.DrawMargin, Options.XMarginMillimeters, Options.YMarginMillimeters);

            case PageSize.A5:
                return new A5LandscapeDocument(Options.DrawMargin, Options.XMarginMillimeters, Options.YMarginMillimeters);

            default:
                throw new NotSupportedException();
        }
    }
}