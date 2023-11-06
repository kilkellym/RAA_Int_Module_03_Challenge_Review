#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Media3D;

#endregion

namespace RAA_Int_Module_03_Challenge_Review
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 1. Get all grids
            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            collector.OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType();

            // 2. Create reference arrays and point lists
            ReferenceArray refArrayVert = new ReferenceArray();
            ReferenceArray refArrayHoriz = new ReferenceArray();

            List<XYZ> pointListVert = new List<XYZ>();
            List<XYZ> pointListHoriz = new List<XYZ>();

            // 3. Loop through grids and check if vertical or horizontal
            foreach (Grid curGrid in collector)
            {
                Line gridLine = curGrid.Curve as Line;

                if (IsLineVertical(gridLine))
                {
                    refArrayVert.Append(new Reference(curGrid));
                    XYZ point1 = gridLine.GetEndPoint(1);
                    pointListVert.Add(point1);
                }
                else
                {
                    refArrayHoriz.Append(new Reference(curGrid));
                    XYZ point1 = gridLine.GetEndPoint(1);
                    pointListHoriz.Add(point1);
                }
            }

            // 4. Order point lists
            List<XYZ> sortedPointListVert = pointListVert.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            XYZ p1 = sortedPointListVert.First();
            XYZ p2 = sortedPointListVert.Last();
            XYZ offset = new XYZ(0, 3, 0);

            List<XYZ> sortedPointListHoriz = pointListHoriz.OrderBy(p => p.Y).ThenBy(p => p.X).ToList();
            XYZ p1h = sortedPointListHoriz[0];
            XYZ p2h = sortedPointListHoriz[sortedPointListHoriz.Count - 1];
            XYZ offsetHoriz = new XYZ(-3, 0, 0);

            // 5. Create dimension strings
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create grid dimensions");

                Line line = Line.CreateBound(p1.Subtract(offset), p2.Subtract(offset));
                Line lineHoriz = Line.CreateBound(p1h.Subtract(offsetHoriz), p2h.Subtract(offsetHoriz));

                Dimension dim = doc.Create.NewDimension(doc.ActiveView, line, refArrayVert);
                Dimension dimHoriz = doc.Create.NewDimension(doc.ActiveView, lineHoriz, refArrayHoriz);

                t.Commit();
            }

            return Result.Succeeded;
        }

        private bool IsLineVertical(Line curLine)
        {
            XYZ p1 = curLine.GetEndPoint(0);
            XYZ p2 = curLine.GetEndPoint(1);

            if (Math.Abs(p1.X - p2.X) < Math.Abs(p1.Y - p2.Y))
                return true;

            return false;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
