using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using IronPython.Runtime;
using System.Collections;

namespace NFox.Pycad
{

    public static class conv
    {

        public static List ToList(ResultBuffer rb)
        {
            List lst = new List();
            if (rb != null)
            {
                foreach (var value in rb)
                {
                    var slst = new List();
                    slst.append(value.TypeCode);
                    slst.append(value.Value);
                    lst.append(slst);
                }
            }
            return lst;
        }


        public static ResultBuffer ToBuffer(List lst)
        {
            ResultBuffer rb = new ResultBuffer();
            foreach (List slst in lst)
                rb.Add(new TypedValue((int)slst[0], slst[1]));
            return rb;
        }

        public static TypedValue[] ToTypedArray(List lst)
        {
            var arr = new TypedValue[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                var slst = lst[i] as List;
                arr[i] = new TypedValue((int)slst[0], slst[1]);
            }
            return arr;
        }

        public static SelectionFilter ToFilter(List lst)
        {
            return new SelectionFilter(ToTypedArray(lst));
        }

        #region LispData

        public static object FromLispData(ResultBuffer rb)
        {
            if (rb != null)
                return GetSubList(rb.GetEnumerator());
            return new List();
        }

        static object GetSubList(ResultBufferEnumerator it)
        {
            List lst = new List();
            while (it.MoveNext())
            {
                switch ((LispDataType)it.Current.TypeCode)
                {
                    case LispDataType.ListEnd:
                        return lst;
                    case LispDataType.DottedPair:
                        return new PythonTuple(lst);
                    case LispDataType.ListBegin:
                        lst.append(GetSubList(it));
                        break;
                    case LispDataType.T_atom:
                        lst.append(true);
                        break;
                    case LispDataType.Nil:
                        lst.append(false);
                        break;
                    default:
                        lst.append(it.Current.Value);
                        break;
                }
            }
            return lst;
        }

        public static object ToLispData(object obj)
        {

            ResultBuffer rb = new ResultBuffer();
            GetBuffer(obj, rb);
            var arr = rb.AsArray();
            switch (arr.Length)
            {
                case 0:
                    return CreateNewValue(LispDataType.Nil);
                case 1:
                    return arr[0];
                default:
                    return rb;
            }
        }

        static readonly TypedValue T = CreateNewValue(LispDataType.T_atom);
        static readonly TypedValue Nil = CreateNewValue(LispDataType.Nil);

        static TypedValue CreateNewValue(LispDataType code, object value)
        {
            return new TypedValue((int)code, value);
        }

        static TypedValue CreateNewValue(LispDataType code)
        {
            return new TypedValue((int)code);
        }

        static void Add(ResultBuffer rb, LispDataType code, object value)
        {
            rb.Add(CreateNewValue(code, value));
        }

        static void Add(ResultBuffer rb, LispDataType code)
        {
            rb.Add(CreateNewValue(code));
        }

        static void GetBuffer(object obj, ResultBuffer rb)
        {
            if (obj is TypedValue)
                rb.Add(obj);
            else if (obj is bool)
                Add(rb, (bool)obj ? LispDataType.T_atom : LispDataType.Nil);
            else if (obj is short)
                Add(rb, LispDataType.Int16, obj);
            else if (obj is int)
                Add(rb, LispDataType.Int32, obj);
            else if (obj is double)
                Add(rb, LispDataType.Double, obj);
            else if (obj is string)
                Add(rb, LispDataType.Text, obj);
            else if (obj is Point2d)
                Add(rb, LispDataType.Point2d, obj);
            else if (obj is Point3d)
                Add(rb, LispDataType.Point3d, obj);
            else if (obj is ObjectId)
                Add(rb, LispDataType.ObjectId, obj);
            else if (obj is SelectionSet)
                Add(rb, LispDataType.SelectionSet, obj);
            else if (obj is PythonTuple)
                GetBuffer((PythonTuple)obj, rb);
            else if (obj is IEnumerable)
                GetBuffer((IEnumerable)obj, rb);
        }

        static void GetBuffer(IEnumerable lst, ResultBuffer rb)
        {
            Add(rb, LispDataType.ListBegin);
            foreach (var obj in lst)
                GetBuffer(obj, rb);
            Add(rb, LispDataType.ListEnd);
        }

        static void GetBuffer(PythonTuple lst, ResultBuffer rb)
        {
            Add(rb, LispDataType.ListBegin);
            foreach (var obj in lst)
                GetBuffer(obj, rb);
            Add(rb, LispDataType.DottedPair);
        }

        #endregion

    }
}
