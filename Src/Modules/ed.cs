using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using IronPython.Runtime;
using System;
using System.Linq;

namespace NFox.Pycad
{
    public class ed
    {
        public static Editor GetAcEditor()
        {
            return
                Autodesk.AutoCAD
                .ApplicationServices
                .Application
                .DocumentManager?
                .MdiActiveDocument?
                .Editor;
        }

        public static void WriteMessage(string message, params object[] parameter)
        {
            GetAcEditor()?.WriteMessage(message, parameter);
        }

        public static PromptResult GetKeywords(PromptKeywordOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetKeywords(opts);
        }

        public static PromptResult GetKeywords(string message, params string[] globalKeywords)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetKeywords(message, globalKeywords);
        }

        public static PromptResult GetString(PromptStringOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetString(opts);
        }

        public static PromptResult GetString(string message)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetString(message);
        }

        public static PromptIntegerResult GetInteger(PromptIntegerOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetInteger(opts);
        }

        public static PromptIntegerResult GetInteger(string message)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetInteger(message);
        }

        public static PromptDoubleResult GetDouble(PromptDoubleOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetDouble(opts);
        }

        public static PromptDoubleResult GetDouble(string message)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetDouble(message);
        }

        public static PromptDoubleResult GetAngle(PromptAngleOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetAngle(opts);
        }

        public static PromptDoubleResult GetAngle(string message)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetAngle(message);
        }

        public static PromptPointResult GetPoint(PromptPointOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetPoint(opts);
        }


        public static PromptPointResult GetPoint(string message)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetPoint(message);
        }

        public static PromptDoubleResult GetDistance(PromptDistanceOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetDistance(opts);
        }

        public static PromptDoubleResult GetDistance(string message)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetDistance(message);
        }

        public static PromptPointResult GetCorner(PromptCornerOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetCorner(opts);
        }

        public static PromptPointResult GetCorner(string message, Point3d basePoint)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetCorner(message, basePoint);
        }

        public static PromptEntityResult GetEntity(PromptEntityOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetEntity(opts);
        }

        public static PromptEntityResult GetEntity(string message)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetEntity(message);
        }

        public static PromptNestedEntityResult GetNestedEntity(PromptNestedEntityOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetNestedEntity(opts);
        }

        public static PromptNestedEntityResult GetNestedEntity(string message)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetNestedEntity(message);
        }

        public static PromptSelectionResult GetSelection()
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetSelection();
        }

        public static PromptSelectionResult GetSelection(PromptSelectionOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetSelection(opts);
        }

        public static PromptSelectionResult GetSelection(List filter)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetSelection(Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult GetSelection(PromptSelectionOptions opts, List filter)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.GetSelection(opts, Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult SelectAll()
        {
            return GetAcEditor()?.SelectAll();
        }

        public static PromptSelectionResult SelectAll(List filter)
        {
            return GetAcEditor()?.SelectAll(Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult SelectCrossingPolygon(Point3dCollection polygon, List filter)
        {
            return
                GetAcEditor()?.SelectCrossingPolygon(
                    polygon,
                    Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult SelectCrossingPolygon(Point3dCollection polygon)
        {
            return GetAcEditor()?.SelectCrossingPolygon(polygon);
        }

        public static PromptSelectionResult SelectCrossingPolygon(List polygon, List filter)
        {
            return
                GetAcEditor()?.SelectCrossingPolygon(
                    new Point3dCollection(polygon.Cast<Point3d>().ToArray()),
                    Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult SelectWindowPolygon(Point3dCollection polygon, List filter)
        {
            return
                GetAcEditor()?.SelectWindowPolygon(
                    polygon,
                    Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult SelectWindowPolygon(Point3dCollection polygon)
        {
            return GetAcEditor()?.SelectWindowPolygon(polygon);
        }

        public static PromptSelectionResult SelectWindowPolygon(List polygon, List filter)
        {
            return
                GetAcEditor()?.SelectWindowPolygon(
                    new Point3dCollection(polygon.Cast<Point3d>().ToArray()),
                    Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult SelectCrossingWindow(Point3d pt1, Point3d pt2)
        {
            return GetAcEditor()?.SelectCrossingWindow(pt1, pt2);
        }

        public static PromptSelectionResult SelectCrossingWindow(Point3d pt1, Point3d pt2, List filter)
        {
            return
                GetAcEditor()?.SelectCrossingWindow(
                    pt1, pt2,
                    Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult SelectCrossingWindow(Point3d pt1, Point3d pt2, List filter, bool forceSubEntitySelection)
        {
            return
                GetAcEditor()?.SelectCrossingWindow(
                    pt1, pt2,
                    Engine.Conv.ToFilter(filter),
                    forceSubEntitySelection);
        }

        public static PromptSelectionResult SelectWindow(Point3d pt1, Point3d pt2)
        {
            return GetAcEditor()?.SelectWindow(pt1, pt2);
        }

        public static PromptSelectionResult SelectWindow(Point3d pt1, Point3d pt2, List filter)
        {
            return
                GetAcEditor()?.SelectWindow(
                    pt1, pt2,
                    Engine.Conv.ToFilter(filter));
        }


        public static PromptSelectionResult SelectFence(Point3dCollection fence, List filter)
        {
            return
                GetAcEditor()?.SelectFence(
                    fence,
                    Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult SelectFence(Point3dCollection fence)
        {
            return GetAcEditor()?.SelectFence(fence);
        }

        public static PromptSelectionResult SelectFence(List polygon, List filter)
        {
            return
                GetAcEditor()?.SelectFence(
                    new Point3dCollection(polygon.Cast<Point3d>().ToArray()),
                    Engine.Conv.ToFilter(filter));
        }

        public static PromptSelectionResult SelectLast()
        {
            return GetAcEditor()?.SelectLast();
        }

        public static PromptSelectionResult SelectPrevious()
        {
            return GetAcEditor()?.SelectPrevious();
        }

        public static void SetImpliedSelection(SelectionSet ss)
        {
            GetAcEditor()?.SetImpliedSelection(ss);
        }

        public static void SetImpliedSelection(ObjectId[] ids)
        {
            GetAcEditor()?.SetImpliedSelection(ids);
        }

        public static PromptSelectionResult SelectImplied()
        {
            return GetAcEditor()?.SelectImplied();
        }

        public static PromptResult Drag(Jig jig)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.Drag(jig);
        }

        public static PromptResult Drag(PromptDragOptions opts)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.Drag(opts);
        }

        public static PromptResult Drag(SelectionSet ss, string message, DragCallback func)
        {
            using (new ConsoleFromManager())
                return GetAcEditor()?.Drag(ss, message, func);
        }

        private class ConsoleFromManager : IDisposable
        {

            public ConsoleFromManager()
            {
                ConsoleStream.HideForm();
            }


            public void Dispose()
            {
                ConsoleStream.ShowForm();
            }
        }

    }
}
