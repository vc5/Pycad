
def ToList(rb):
    try: return [[v.TypeCode, v.Value] for v in rb]
    except: return []

def ToBuffer(lst):
    try:
        t = tuple((_newvalue(*v) for v in lst))
        return acdb.ResultBuffer(t)
    except:
        pass

def ToTypedArray(lst):
    from System import Array
    return Array[acdb.TypedValue]([_newvalue(*v) for v in lst])

def ToFilter(lst):
    return aced.SelectionFilter(ToTypedArray(lst))

def FromLispData(rb):
    if rb: return _getlist(rb.GetEnumerator())
    return []

def _getlist(it):
    lst = []
    while it.MoveNext():
        from System import Enum
        code = Enum.ToObject(acrx.LispDataType, it.Current.TypeCode)
        if code == acrx.LispDataType.ListEnd:
            return lst
        elif code == acrx.LispDataType.DottedPair:
            return tuple(lst)
        elif code == acrx.LispDataType.ListBegin:
            lst.append(_getlist(it))
        elif code == acrx.LispDataType.T_atom:
            lst.append(True)
        elif code == acrx.LispDataType.Nil:
            lst.append(False)
        else:
            lst.append(it.Current.Value)
    return lst

def ToLispData(obj):
    lst = []
    _getbuffer(obj, lst)
    n = len(lst)
    if n == 0:
        return _newvalue(acrx.LispDataType.Nil)
    elif n == 1:
        return lst[0]
    return acdb.ResultBuffer(tuple(lst))

def _getbuffer(obj, lst):
    from System import Int16, Int32
    if isinstance(obj, acdb.TypedValue):
        lst.append(obj)
    elif isinstance(obj, (Int16, Int32, int)):
        lst.append(_newvalue(acrx.LispDataType.Int32, obj))
    elif obj == True:
        lst.append(_newvalue(acrx.LispDataType.T_atom))
    elif obj in (False, None):
        lst.append(_newvalue(crx.LispDataType.Nil))
    elif isinstance(obj, float):
        lst.append(_newvalue(acrx.LispDataType.Double, obj))
    elif isinstance(obj, str):
        lst.append(_newvalue(acrx.LispDataType.Text, obj))
    elif isinstance(obj, acge.Point2d):
        lst.append(_newvalue(acrx.LispDataType.Point2d, obj))
    elif isinstance(obj, acge.Point3d):
        lst.append(_newvalue(acrx.LispDataType.Point3d, obj))
    elif isinstance(obj, acdb.ObjectId):
        lst.append(_newvalue(acrx.LispDataType.ObjectId, obj))
    elif isinstance(obj, aced.SelectionSet):
        lst.append(_newvalue(acrx.LispDataType.SelectionSet, obj))
    else:
        from collections import Iterable
        if isinstance(obj, Iterable):
            lst.append(_newvalue(acrx.LispDataType.ListBegin))
            for o in obj:
                _getbuffer(o, lst)
            if isinstance(obj, tuple):
                lst.append(_newvalue(acrx.LispDataType.DottedPair))
            else:
                lst.append(_newvalue(acrx.LispDataType.ListEnd))

def _newvalue(code, value = -1):
    try:
        return acdb.TypedValue(code, value)
    except TypeError:
        return acdb.TypedValue(code.value__, value)

def _getlist2(it):
    lst = []
    while it.MoveNext():
        from System import Enum
        case = switcher(Enum.ToObject(acrx.LispDataType, it.Current.TypeCode))
        if case(acrx.LispDataType.ListEnd): return lst
        elif case(acrx.LispDataType.DottedPair): return tuple(lst)
        elif case(acrx.LispDataType.ListBegin): lst.append(_getlist2(it))
        elif case(acrx.LispDataType.T_atom): lst.append(True)
        elif case(acrx.LispDataType.Nil): lst.append(False)
        else: lst.append(it.Current.Value)
    return lst

def _getbuffer2(obj, lst):
    from System import Int16, Int32
    case = switcher(obj)
    if case[acdb.TypedValue]:
        lst.append(obj)
    elif case[Int16, Int32, int]:
        lst.append(_newvalue(acrx.LispDataType.Int32, obj))
    elif case(True):
        lst.append(_newvalue(acrx.LispDataType.T_atom))
    elif case(False, None):
        lst.append(_newvalue(crx.LispDataType.Nil))
    elif case[float]:
        lst.append(_newvalue(acrx.LispDataType.Double, obj))
    elif case[str]:
        lst.append(_newvalue(acrx.LispDataType.Text, obj))
    elif case[acge.Point2d]:
        lst.append(_newvalue(acrx.LispDataType.Point2d, obj))
    elif case[acge.Point3d]:
        lst.append(_newvalue(acrx.LispDataType.Point3d, obj))
    elif case[acdb.ObjectId]:
        lst.append(_newvalue(acrx.LispDataType.ObjectId, obj))
    elif case[aced.SelectionSet]:
        lst.append(_newvalue(acrx.LispDataType.SelectionSet, obj))
    else:
        from collections import Iterable
        if case[Iterable]:
            lst.append(_newvalue(acrx.LispDataType.ListBegin))
            for o in obj:
                _getbuffer2(o, lst)
            if case[tuple]:
                lst.append(_newvalue(acrx.LispDataType.DottedPair))
            else:
                lst.append(_newvalue(acrx.LispDataType.ListEnd))