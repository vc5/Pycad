from upopen import upopen

class dbtrans(object):
    '''事务简化类
    dbtrans(db: acdb.Database, commit: bool)'''
    
    _tables = (
        'BlockTable', 'LayerTable', 'TextStyleTable',
        'RegAppTable', 'DimStyleTable', 'LinetypeTable',
        'UcsTable', 'ViewTable', 'ViewportTable',
        'GroupDictionary', 'MLeaderStyleDictionary', 'MLStyleDictionary',
        'MaterialDictionary', 'TableStyleDictionary', 'VisualStyleDictionary',
        'ColorDictionary', 'PlotSettingsDictionary', 'PlotStyleNameDictionary',
        'LayoutDictionary')

    @staticmethod
    def isnullid(oid):
        return old == None or oid == acdb.ObjectId.Null

    def __init__(self, db = None, commit = False):
        if db:
            self.Database = db
        else:
            self.Document = acap.Application.DocumentManager.MdiActiveDocument
            self.Database = self.Document.Database
        self.Trans = self.Database.TransactionManager.StartTransaction()
        self.tables = {}
        self.commit = commit

    def __enter__(self):
        return self

    def __exit__(self, e_t, e_v, t_b):
        if t_b:
            self.Trans.Abort()
        elif self.commit:
            self.Trans.Commit()
        self.Trans.Dispose()

    def getobject(self, id, openMode = acdb.OpenMode.ForRead, openErased = False):
        return self.Trans.GetObject(id, openMode, openErased)

    def getobjectid(self, hstr):
        h = acdb.Handle(long(hstr, 16))
        return self.Database.GetObjectId(False, h, 0)

    def getrealobject(self, obj, type = acdb.DBObject):
        if isinstance(obj, acdb.ObjectId):
            obj = self.getobject(obj)
        if isinstance(obj, type):
            return obj

    def _gettable(self, name):
        if name in self._tables:
            if name not in self.tables:
                tid = getattr(self.Database, name + 'Id')
                self.tables[name] = self.getobject(tid)
            return self.tables[name]

    @property
    def BlockTable(self):
        return self._gettable('BlockTable')

    @property
    def DimStyleTable(self):
        return self._gettable('DimStyleTable')

    @property
    def LayerTable(self):
        return self._gettable('LayerTable')

    @property
    def TextStyleTable(self):
        return self._gettable('TextStyleTable')

    @property
    def RegAppTable(self):
        return self._gettable('RegAppTable')

    @property
    def LinetypeTable(self):
        return self._gettable('LinetypeTable')

    @property
    def UcsTable(self):
        return self._gettable('UcsTable')

    @property
    def ViewTable(self):
        return self._gettable('ViewTable')

    @property
    def ViewportTable(self):
        return self._gettable('ViewportTable')

    @property
    def GroupDictionary(self):
        return self._gettable('GroupDictionary')

    @property
    def MLeaderStyleDictionary(self):
        return self._gettable('MLeaderStyleDictionary')

    @property
    def MLStyleDictionary(self):
        return self._gettable('MLStyleDictionary')

    @property
    def MaterialDictionary(self):
        return self._gettable('MaterialDictionary')

    @property
    def TableStyleDictionary(self):
        return self._gettable('TableStyleDictionary')

    @property
    def VisualStyleDictionary(self):
        return self._gettable('VisualStyleDictionary')

    @property
    def ColorDictionary(self):
        return self._gettable('ColorDictionary')

    @property
    def PlotSettingsDictionary(self):
        return self._gettable('PlotSettingsDictionary')

    @property
    def PlotStyleNameDictionary(self):
        return self._gettable('PlotStyleNameDictionary')

    @property
    def LayoutDictionary(self):
        return self._gettable('LayoutDictionary')

    def getdict(self, dict, createsub, names):
        dict = self.getrealobject(dict, acdb.DBDictionary)
        if dict is None:
            return
        if createsub:
            with upopen(dict):
                dict.TreatElementsAsHard = True
            for name in names:
                if dict.Contains(name):
                    dict = self.getobject(dict.GetAt(name))
                else:
                    subDict = acdb.DBDictionary()
                    dict.SetAt(name, subDict)
                    dict = subDict
                    dict.TreatElementsAsHard = True
        else:
            for name in names:
                if dict.Contains(name):
                    dict = self.getobject(dict.GetAt(name))
                else:
                    return
        return dict;

    def getdictforobject(self, obj):
        if obj.ExtensionDictionary == acdb.ObjectId.Null:
            with upopen(obj):
                obj.CreateExtensionDictionary()
        return obj.ExtensionDictionary

    def getrootdict(self, names):
        if len(names) == 0:
            return
        elif isinstance(names[0], (acdb.DBObject, acdb.ObjectId)):
            if len(names) == 1:
                return
            id = self.getdictforobject(self.getrealobject(names[0]))
            del names[0]
        else:
            id = self.Database.NamedObjectsDictionaryId
        return id

    def settodict(self, value, *names):
        names = list(names)
        id = self.getrootdict(names)
        if id:
            key = names.pop()
            dict = self.getdict(id, True, names)
            dict.SetAt(key, value)
            self.Trans.AddNewlyCreatedDBObject(value, True)

    def getfromdict(self, *names):
        names = list(names)
        id = self.getrootdict(names)
        if id:
            key = names.pop()
            dict = self.getdict(id, False, names)
            id = dict.GetAt(key)
            return self.getobject(id)

    def setxrecord(self, values, *names):
        xr = acdb.Xrecord()
        xr.Data = conv.ToBuffer(values)
        self.settodict(xr, *names)

    def getxrecord(self, *names):
        xr = self.getfromdict(*names)
        if xr: return conv.ToList(xr.Data)

    def opencurrspace(self, openMode = acdb.OpenMode.ForRead, openErased = False):
        return self.getobject(self.Database.CurrentSpaceId, openMode, openErased)

    def openblockdef(self, name, openMode = acdb.OpenMode.ForRead, openErased = False):
        if isinstance(name, str):
            return self.getobject(self.BlockTable[name], openMode, openErased)
        elif isinstance(name, acdb.ObjectId):
            return self.getobject(name, openMode, openErased)
    
    def openpaperspace(self, openMode = acdb.OpenMode.ForRead, openErased = False):
        return self.getobject(self.BlockTable[acdb.BlockTableRecord.PaperSpace], openMode, openErased)

    def openmodelspace(self, openMode = acdb.OpenMode.ForRead, openErased = False):
        return self.getobject(self.BlockTable[acdb.BlockTableRecord.ModelSpace], openMode, openErased)

    def getrecordfrom(souce, target, over, *names):
        ids = acdb.ObjectIdCollection()
        for sid in (souce[name] for name in mames):
            if dbtrans.isnullid(sid): ids.Add(sid)
        idm = acdb.IdMapping()
        mode = over and acdb.DuplicateRecordCloning.Replace or acdb.DuplicateRecordCloning.Ignore
        souce.Database.WblockCloneObjects(ids, target, idm, mode, False)
        return idm

    def addentity(self, btr, *ents):
        ids = []
        with upopen(btr):
            for ent in ents:
                ids.append(btr.AppendEntity(ent))
                self.Trans.AddNewlyCreatedDBObject(ent, True)
        return ids

    def addrecord(self, table, record):
        with upopen(record):
            rid = table.Add(record)
            self.Trans.AddNewlyCreatedDBObject(record, True)
            return rid

    def addregapp(self, name):
        rat = self.RegAppTable
        if rat.Has(name): return rat[name]
        ratr = acdb.RegAppTableRecord()
        ratr.Name = name
        return self.addrecord(rat, ratr)

    def createlayer(name, color = None, linetypeid = None, lineweight = None):
        ltr = acdb.LayerTableRecord()
        ltr.Name = name
        if color: ltr.Color = color
        if dbtrans.isnullid(linetypeid):
            linetypeid = self.LinetypeTable['Continuous']
        ltr.LineWeight = lineweight or acdb.LineWeight.LineWeight000
        ltr.LinetypeObjectId = linetypeid
        return ltr

    def addlayer(self, name, color = None, linetypeid = None, lineweight = None):
        lt = self.LayerTable
        if lt.Has(name): return lt[name]
        ltr = createlayer(name, color, linetypeid, lineweight)
        return self.addrecord(lt, ltr)

    def createtextstyle(name, font, xscale = 1, bigfont = None):
        tstr = acdb.TextStyleTableRecord()
        tstr.Name = name
        tstr.FileName = font
        if bigfont: tstr.BigFontFileName = bigfont
        tstr.XScale = xscale
        return tstr

    def addtextstyle(self, name, font, xscale = 1, bigfont = None):
        tst = self.TextStyleTable
        if tst.Has(name): return tst[name]
        tstr = createtextstyle(name, font, xscale, bigfont)
        return self.addrecord(tst, tstr)
        