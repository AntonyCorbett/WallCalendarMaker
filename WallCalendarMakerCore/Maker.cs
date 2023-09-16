﻿using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using Svg;
using WallCalendarMakerCore.CommonDocuments;

namespace WallCalendarMakerCore;

public class Maker : IMaker
{
    public MakerOptions Options { get; } = new();

    public Maker(Action<MakerOptions>? action = null)
    {
        action?.Invoke(Options);
    }

    public void Generate(string path, bool useBom = true)
    {
        Generate().Write(path, useBom);
    }

    public SvgDocument Generate()
    {
        var doc = CreateBlankDocument();

        var boxGroup = new SvgGroup { ID = "BoxGroup" };

        var numRows = GetRowCount();

        var dayNameHeightMillimeter = GetDayNameHeightAllowanceMillimeters();

        var outlineBoxGroupWidth = doc.Width.Value - (2 * Options.XMarginMillimeters);
        var outlineBoxGroupHeight = doc.Height.Value - (2 * Options.YMarginMillimeters) - dayNameHeightMillimeter;

        var outlineBox = CreateOutlineBox(outlineBoxGroupWidth, outlineBoxGroupHeight, dayNameHeightMillimeter);

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

        AddDayNames(doc, boxGroup);
        AddDayNumbers(doc, boxGroup, individualBoxMargin);
        SpecifyLiveBoxOpacity(boxGroup);
        SpecifyDeadBoxOpacity(boxGroup);
        SpecifyBoxCorners(boxGroup);

        return doc;
    }

    private void SpecifyBoxCorners(SvgGroup boxGroup)
    {
        foreach (var box in boxGroup.Children.OfType<SvgRectangle>().Where(IsLiveBox))
        {
            switch (Options.BoxCornerMode)
            {
                case BoxCornerMode.Normal:
                    break;

                case BoxCornerMode.Rounded:
                    var radius = box.Width / 10;
                    box.CornerRadiusX = radius;
                    box.CornerRadiusY = radius;
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
        return (float)((Options.FontPointSizeDays * 25.4) / 72.0);
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

    private void AddDayNumbers(SvgDocument doc, SvgGroup boxGroup, float boxMarginMillimeters)
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
            Font = Options.FontNameNumbers,
            FontSize = new SvgUnit(SvgUnitType.Point, Options.FontPointSizeNumbers)
        };

        var extraMargin = Options.BoxCornerMode == BoxCornerMode.Rounded ? box.Width / 30 : 0;

        numberText.ID = $"DayNumber{dayNumber}";
        numberText.X = new SvgUnitCollection
        {
            box.X + box.Width -
            new SvgUnit(SvgUnitType.Millimeter, boxMarginMillimeters) -
            new SvgUnit(SvgUnitType.Pixel, numberText.Bounds.Width) -
            extraMargin
        };

        numberText.Y = new SvgUnitCollection { box.Y + numberText.FontSize + extraMargin + (isOverflow ? box.Height / 2 : 0)};

        return numberText;
    }

    private void AddDayNames(SvgDocument doc, SvgGroup boxGroup)
    {
        var group = new SvgGroup { ID = "DayNameGroup" };

        foreach (var box in boxGroup.Children.Take(7).OfType<SvgRectangle>())
        {
            var col = GetBoxColumn(box);
            var dayOfWeek = (int)CalculateDayOfWeek(col);
            var dayOfWeekString = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[dayOfWeek];

            var s = new SvgText(dayOfWeekString)
            {
                ID = $"DayName{dayOfWeek}", // do not localize day name here
                Font = Options.FontNameDays,
                FontSize = Options.FontPointSizeDays,
                X = new SvgUnitCollection {box.X + box.Width / 2},
                Y = new SvgUnitCollection {box.Y - new SvgUnit(SvgUnitType.Millimeter, 3)},
                TextAnchor = SvgTextAnchor.Middle
            };

            group.Children.Add(s);
        }

        doc.Children.Add(group);
    }

    private DayOfWeek CalculateDayOfWeek(int col)
    {
        return (DayOfWeek)(((int) Options.MonthDefinition.FirstDayOfWeek + col) % 7);
    }

    private static SvgRectangle CreateSingleBox(
        float x, float y, float widthMillimeters, float heightMillimeters, float boxMargin, string id)
    {
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

    private SvgRectangle CreateOutlineBox(float widthMillimeters, float heightMillimeters, float dayNameHeightMillimeterAllowance)
    {
        return new SvgRectangle
        {
            ID = "OutlineBox",
            X = new SvgUnit(SvgUnitType.Millimeter, Options.XMarginMillimeters),
            Y = new SvgUnit(SvgUnitType.Millimeter, Options.YMarginMillimeters + dayNameHeightMillimeterAllowance),
            Width = new SvgUnit(SvgUnitType.Millimeter, widthMillimeters),
            Height = new SvgUnit(SvgUnitType.Millimeter, heightMillimeters),
            Fill = new SvgColourServer(Color.Azure),
        };
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