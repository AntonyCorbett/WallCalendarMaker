using System.Diagnostics.Contracts;
using System.Drawing;
using System.Globalization;
using Svg;

namespace WallCalendarMakerCore;

public class YearMaker : MakerBase, IYearMaker
{
    public YearMakerOptions Options { get; } = new();

    public YearMaker(Action<YearMakerOptions>? action = null)
    {
        action?.Invoke(Options);
    }

    public void Generate(string path, bool useBom = true)
    {
        Generate().Write(path, useBom);
    }

    public SvgDocument Generate()
    {
        Options.Validate();

        var doc = CreateBlankDocument(
            Options.PageSize, 
            Options.DrawMargin, 
            Options.LMarginMillimeters, 
            Options.TMarginMillimeters, 
            Options.RMarginMillimeters, 
            Options.BMarginMillimeters);

        const int numCalendarsOnXAxis = 4;
        const int numCalendarsOnYAxis = 3;

        var calendarBoxes = CreateCalendarBoxes(doc, numCalendarsOnXAxis, numCalendarsOnYAxis);

        var month = 1;
        foreach (var box in calendarBoxes)
        {
            PopulateCalendarBox(box, month++);
            doc.Children.Add(box);
        }

        return doc;
    }

    private void PopulateCalendarBox(SvgRectangle box, int month)
    {
        PopulateMonthName(box, month);
        PopulateDayNames(box);
        PopulateWithDayNumbers(box, month);
    }

    private void PopulateDayNames(SvgRectangle box)
    {
        var firstDow = (int)Options.YearDefinition.FirstDayOfWeek;
        var lastDow = (firstDow + 6) % 7;

        var yOffset = box.Height.Value * 0.25F;

        const int colCount = 7;
        var colWidth = box.Width.Value / colCount;

        var dow = firstDow;
        var col = 0;
        do
        {
            var s = CultureInfo.InvariantCulture.DateTimeFormat.DayNames[dow].Substring(0, 1);
            var text = GenerateText(s, Options.DayNamesFont);
            text.TextAnchor = SvgTextAnchor.Middle;
            
            var x = box.X.Value + (col * colWidth) + (colWidth / 2);
            var y = box.Y.Value + yOffset;

            text.X = new SvgUnitCollection { new(SvgUnitType.Millimeter, x) };
            text.Y = new SvgUnitCollection { new(SvgUnitType.Millimeter, y) };

            box.Children.Add(text);
            
            ++col;
            dow = (dow + 1) % 7;
        } while (dow != firstDow);
    }

    private void PopulateMonthName(SvgRectangle box, int month)
    {
        var heightAvailable = box.Height.Value * 0.1F; 

        var monthName = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[month - 1];
        var monthText = GenerateMonthName(monthName, box.X.Value + box.Width.Value / 2, box.Y.Value + heightAvailable);
        var rect = new SvgRectangle
        {
            X = new SvgUnit(SvgUnitType.Millimeter, box.X.Value),
            Y = new SvgUnit(SvgUnitType.Millimeter, box.Y.Value),
            Width = box.Width,
            Height = new SvgUnit(SvgUnitType.Millimeter, heightAvailable * 1.5F),
            Fill = new SvgColourServer(Options.MonthHeaderBackgroundColor),
        };
        
        box.Children.Add(rect);
        box.Children.Add(monthText);
    }

    private void PopulateWithDayNumbers(SvgRectangle box, int month)
    {
        const int rowCount = 6;
        const int colCount = 7;

        var startingDate = new DateTime(Options.YearDefinition.Year, month, 1);
        var startingCol = GetCol(startingDate.DayOfWeek);
        var heightAvailable = box.Height.Value * 0.70F; // allow 30% for month name and day titles.
        var yOffset = box.Height.Value - heightAvailable;

        var rowHeight = heightAvailable / rowCount;
        var colWidth = box.Width.Value / colCount;

        var theDate = startingDate;

        while (theDate.Month == month)
        {
            var col = GetCol(theDate.DayOfWeek);
            var row = GetRow(startingCol, colCount, startingDate, theDate);

            var x = box.X.Value + (col * colWidth) + (colWidth / 2);
            var y = box.Y.Value + yOffset + (row * rowHeight) + (rowHeight / 2);
            var number = GenerateDayNumber(theDate, x, y);

            box.Children.Add(number);

            theDate = theDate.AddDays(1);
        }
    }

    private SvgText GenerateDayNumber(DateTime theDate, float x, float y)
    {
        var numberText = GenerateText(theDate.Day.ToString(), Options.NumbersFont);
        numberText.TextAnchor = SvgTextAnchor.Middle;
        
        numberText.X = new SvgUnitCollection
        {
            new (SvgUnitType.Millimeter, x)
        };

        numberText.Y = new SvgUnitCollection
        {
            new (SvgUnitType.Millimeter, y)
        };

        return numberText;
    }

    private SvgText GenerateMonthName(string monthName, float x, float y)
    {
        var numberText = GenerateText(monthName, Options.MonthNamesFont);
        numberText.TextAnchor = SvgTextAnchor.Middle;

        numberText.X = new SvgUnitCollection
        {
            new (SvgUnitType.Millimeter, x)
        };

        numberText.Y = new SvgUnitCollection
        {
            new (SvgUnitType.Millimeter, y)
        };

        return numberText;
    }

    private int GetRow(int startingCol, int colCount, DateTime startingDate, DateTime theDate)
    {
        var dayCount = (theDate - startingDate).Days;
        return (startingCol + dayCount) / colCount;
    }

    private int GetCol(DayOfWeek dow)
    {
        switch (Options.YearDefinition.FirstDayOfWeek)
        {
            case DayOfWeek.Sunday:
                return ((int)dow % 7);
            case DayOfWeek.Monday:
                return (int)(dow + 6) % 7;
            case DayOfWeek.Tuesday:
                return (int)(dow + 5) % 7;
            case DayOfWeek.Wednesday:
                return (int)(dow + 4) % 7;
            case DayOfWeek.Thursday:
                return (int)(dow + 3) % 7;
            case DayOfWeek.Friday:
                return (int)(dow + 2) % 7;
            case DayOfWeek.Saturday:
                return (int)(dow + 1) % 7;
            default:
                throw new NotSupportedException("Unknown day!");
        }
    }

    private IEnumerable<SvgRectangle> CreateCalendarBoxes(
        SvgDocument doc, int numCalendarsOnXAxis, int numCalendarsOnYAxis)
    {
        const int horizontalSpacingMillimeters = 5;
        const int verticalSpacingMillimeters = 5;

        var widthAvailable = doc.Width.Value - Options.LMarginMillimeters - Options.RMarginMillimeters;
        var heightAvailable = doc.Height.Value - Options.TMarginMillimeters - Options.BMarginMillimeters;
        
        var maxWidthOfEachBox = (widthAvailable - ((numCalendarsOnXAxis - 1) * horizontalSpacingMillimeters)) / numCalendarsOnXAxis;
        var maxHeightOfEachBox = (heightAvailable - ((numCalendarsOnYAxis - 1) * verticalSpacingMillimeters)) / numCalendarsOnYAxis;

        var squareBoxHeightWidth = Math.Min(maxWidthOfEachBox, maxHeightOfEachBox);

        var combinedHeightOfBoxes = squareBoxHeightWidth * numCalendarsOnYAxis;
        var combinedWidthOfBoxes = squareBoxHeightWidth * numCalendarsOnXAxis;

        var spacerWidth = (widthAvailable - combinedWidthOfBoxes) / (numCalendarsOnXAxis - 1);
        var spacerHeight = (heightAvailable - combinedHeightOfBoxes) / (numCalendarsOnYAxis - 1);

        var month = 1;

        for (var row = 0; row < numCalendarsOnYAxis; ++row)
        {
            var top = Options.TMarginMillimeters + (row * (squareBoxHeightWidth + spacerHeight));

            for (var col = 0; col < numCalendarsOnXAxis; ++col)
            {
                var left = Options.LMarginMillimeters + (col * (squareBoxHeightWidth + spacerWidth));
                var box = CreateOutlineBox(month++, left, top, squareBoxHeightWidth);
                yield return box;
            }
        }
    }

    private SvgRectangle CreateOutlineBox(
        int monthNumber, 
        float xMillimeters,
        float yMillimeters,
        float widthHeightMillimeters)
    {
        var result = new SvgRectangle
        {
            ID = $"Month{monthNumber}",
            X = new SvgUnit(SvgUnitType.Millimeter, xMillimeters),
            Y = new SvgUnit(SvgUnitType.Millimeter, yMillimeters),
            Width = new SvgUnit(SvgUnitType.Millimeter, widthHeightMillimeters),
            Height = new SvgUnit(SvgUnitType.Millimeter, widthHeightMillimeters),
            Stroke = new SvgColourServer(Options.MonthHeaderBackgroundColor),
            StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 0.5F),
            Fill = new SvgColourServer(Color.White),
        };

        return result;
    }

}