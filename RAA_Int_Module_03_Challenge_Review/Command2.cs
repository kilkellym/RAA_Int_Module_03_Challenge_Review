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

#endregion

namespace RAA_Int_Module_03_Challenge_Review
{
    [Transaction(TransactionMode.Manual)]
    public class Command2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // 1. Get all rooms
            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id)
               .OfCategory(BuiltInCategory.OST_Rooms);

            int counter = 0;

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Dimension rooms");

                // 2. Loop through rooms
                foreach (SpatialElement curRoom in collector)
                {
                    // 3. get room boundaries
                    SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
                    options.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;

                    // 4. create ref array and point list
                    ReferenceArray referenceArray = new ReferenceArray();
                    ReferenceArray referenceArrayHoriz = new ReferenceArray();
                    List<XYZ> pointList = new List<XYZ>();
                    List<XYZ> pointListHoriz = new List<XYZ>();

                    // 5. get room boundaries
                    List<BoundarySegment> boundSegList = curRoom.GetBoundarySegments(options).First().ToList();

                    // 6. loop through segements 
                    foreach (BoundarySegment curSeg in boundSegList)
                    {
                        // 6a. Get boundary geometry
                        Curve boundCurve = curSeg.GetCurve();
                        XYZ midPoint = boundCurve.Evaluate(0.5, true);

                        // 6c. Get boundary wall
                        Element curElem = doc.GetElement(curSeg.ElementId);

                        if(curElem != null)
                        {
                            // 6b. Check if line is vertical
                            if (IsLineVertical(boundCurve))
                            {
                                // 7. Add to ref and point array
                                referenceArray.Append(new Reference(curElem));
                                pointList.Add(midPoint);
                            }
                            else
                            {

                                // 7. Add to ref and point array
                                referenceArrayHoriz.Append(new Reference(curElem));
                                pointListHoriz.Add(midPoint);
                                
                            }

                        }
                    }

                    // 8. Create line for dimension
                    XYZ offset = new XYZ(3, 0, 0);
                    XYZ offsetHoriz = new XYZ(0, 3, 0);

                    XYZ p1 = pointList.First().Add(offset);
                    XYZ p2 = new XYZ(pointList.Last().X, pointList.First().Y, 0).Add(offset);

                    XYZ p1h = pointListHoriz.First().Add(offsetHoriz);
                    XYZ p2h = new XYZ(pointListHoriz.First().X, pointListHoriz.Last().Y, 0).Add(offsetHoriz);

                    Line dimLine = Line.CreateBound(p1, p2);
                    Line dimLineHoriz = Line.CreateBound(p1h.Add(offsetHoriz), p2h.Add(offsetHoriz));
                    
                    Dimension newDim = doc.Create.NewDimension(doc.ActiveView, dimLine, referenceArray);
                    Dimension newDimHoriz = doc.Create.NewDimension(doc.ActiveView, dimLineHoriz, referenceArrayHoriz);

                    counter++;
                }
                t.Commit();
            }

            TaskDialog.Show("Complete", $"Created {counter} dimension strings.");

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 2");

            return myButtonData1.Data;
        }
        private bool IsLineVertical(Curve curLine)
        {
            XYZ p1 = curLine.GetEndPoint(0);
            XYZ p2 = curLine.GetEndPoint(1);

            if (Math.Abs(p1.X - p2.X) < Math.Abs(p1.Y - p2.Y))
                return true;

            return false;
        }
    }
}