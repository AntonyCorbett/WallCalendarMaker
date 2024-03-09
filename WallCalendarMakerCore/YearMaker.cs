using System.Drawing;
using System.Security.Cryptography;
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

            var x = ((col + 1) * colWidth) + box.X.Value;
            var y = (row * rowHeight) + box.Y.Value + yOffset;
            var number = GenerateDayNumber(theDate, x, y);

            box.Children.Add(number);

            theDate = theDate.AddDays(1);
        }
    }

    private SvgText GenerateDayNumber(DateTime theDate, float x, float y)
    {
        var numberText = new SvgText(theDate.Day.ToString())
        {
            Font = Options.NumbersFont.Name,
            FontSize = new SvgUnit(SvgUnitType.Point, Options.NumbersFont.PointSize),
            FontStyle = Options.NumbersFont.Italic ? SvgFontStyle.Italic : SvgFontStyle.Normal,
            FontWeight = Options.NumbersFont.Bold ? SvgFontWeight.Bold : SvgFontWeight.Normal,
            TextAnchor = SvgTextAnchor.Middle,
            Fill = new SvgColourServer(Options.NumbersFont.Color),
        };

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
        return dow switch
        {
            DayOfWeek.Monday => 0,
            DayOfWeek.Tuesday => 1,
            DayOfWeek.Wednesday => 2,
            DayOfWeek.Thursday => 3,
            DayOfWeek.Friday => 4,
            DayOfWeek.Saturday => 5,
            DayOfWeek.Sunday => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(dow), dow, null)
        };
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
            Fill = new SvgColourServer(Color.DeepSkyBlue),
        };

        // result.Display = "none";
        
        return result;
    }

}