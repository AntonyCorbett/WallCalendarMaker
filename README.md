# Wall Calendar Maker

A simple project to create an SVG image of a calendar month suitable for use in a wall calendar, for example. The resulting SVG file is very basic in its appearance and is intended to be imported into a publishing/drawing application for formatting, etc.

The code is in the form of a .NET library (WallCalendarMakerCore) and comes with a console application (WallCalendarMaker) for test purposes. MainApp.ExecuteAsync method is the starting point in the console application, and you would need to modify the MakerOptions in that method to change the year/month, fonts, etc.
