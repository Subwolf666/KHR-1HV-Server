using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public static class StaticUtilities
    {
        public const int LOGGING = 1; // 0 = no logging, 1 = screen, 2 = file, 3 = both
        public const int numberOfServos = 24;

        public const int numberOfMotions = 80;
        public const int numberOfDataTableItems = 5;

        public const int servoNull = 1500;
        public const int servoMin = 700;
        public const int servoMax = 2300;

        public const string SectionGraphicalEdit = "GraphicalEdit";
        public const string SectionItem = "Item";
        public const string SectionLink = "Link";

        public const string GraphicalEditType = "Type";
        public const string GraphicalEditWidth = "Width";
        public const string GraphicalEditHeight = "Height";
        public const string GraphicalEditItems = "Items";
        public const string GraphicalEditLinks = "Links";
        public const string GraphicalEditStart = "Start";
        public const string GraphicalEditName = "Name";
        public const string GraphicalEditCtrl = "Ctrl";

        public readonly static string[] GraphicalEdit = new string[]
        {
            GraphicalEditType,
            GraphicalEditWidth,
            GraphicalEditHeight,
            GraphicalEditItems,
            GraphicalEditLinks,
            GraphicalEditStart,
            GraphicalEditName,
            GraphicalEditCtrl
        };

        public const string ItemName = "Name";
        public const string ItemWidth = "Width";
        public const string ItemHeight = "Height";
        public const string ItemLeft = "Left";
        public const string ItemTop = "Top";
        public const string ItemColor = "Color";
        public const string ItemType = "Type";
        public const string ItemPrm = "Prm";

        public readonly static string[] Item = new string[]
        {
            ItemName,
            ItemWidth,
            ItemHeight,
            ItemLeft,
            ItemTop,
            ItemColor,
            ItemType,
            ItemPrm
        };

        public const string LinkMain = "Main";
        public const string LinkOrigin = "Origin";
        public const string LinkFinal = "Final";
        public const string LinkPoint = "Point";

        public readonly static string[] Link = new string[]
        {
            LinkMain,
            LinkOrigin,
            LinkFinal,
            LinkPoint
        };

        public const string DataTableMotion = "Motion";
        public const string DataTableName = "Name";
        public const string DataTableCount = "Count";
        public const string DataTableDate = "Date";
        public const string DataTableCtrl = "Control";

    }
}
