import cmds

from pycad.runtime import *

@command()
@showtime
def mycir2():
    doc = acap.Application.DocumentManager.MdiActiveDocument
    db = doc.Database
    with db.TransactionManager.StartTransaction() as tr:
        btr = tr.GetObject(db.CurrentSpaceId, acdb.OpenMode.ForWrite)
        n = 100000
        for i in range(n):
            cir = acdb.Circle(acge.Point3d(), acge.Vector3d.ZAxis, 2)
            btr.AppendEntity(cir)
            tr.AddNewlyCreatedDBObject(cir, True)
        tr.Commit()

@command()
def mylayer():
    with dbtrans(commit=True) as tr:
        tr.addlayer('1')

@command()
@showtime
def mycir3():
    with dbtrans(commit=True) as tr:
        btr = tr.opencurrspace()
        cirs = (acdb.Circle(acge.Point3d(), acge.Vector3d.ZAxis, 2) 
            for i in range(100000))
        tr.addentity(btr, *cirs)