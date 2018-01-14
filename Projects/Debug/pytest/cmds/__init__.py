#coding:utf-8
import System
clr.ImportExtensions(System.Linq)

from pycad.runtime import *

@command(flags = acrx.CommandFlags.Redraw)
def mycir():
    with dbtrans(commit = True) as tr:
        btr = tr.opencurrspace(acdb.OpenMode.ForWrite)
        res = ed.GetPoint('\n输入一个点')
        if res.Status == aced.PromptStatus.OK:
            cir = acdb.Circle(res.Value, acge.Vector3d.ZAxis, 2)
            tr.addentity(btr, cir)
            tr.setxrecord([[1000, 'abc']], cir, 'NFox.Cad')
            print '\n在对象字典中设置了值为%s的扩展数据' % tr.getxrecord(cir, 'NFox.Cad')

@command()
def mydict():
    with dbtrans(commit = True) as tr:
        tr.setxrecord([[1000, 'abc']], 'NFox.Cad')
        print '\n在命名字典中设置了值为%s的扩展数据' % tr.getxrecord('NFox.Cad')

@command()
def mymsg():
    from ctypes import windll
    windll.user32.MessageBoxW(0, "Great", "Hello World", 0)

@command()
def showwpf():
    from pytest.WpfTestForm import WpfTestForm
    acap.Application.ShowModalDialog(WpfTestForm())

@command()
def showfrm():
    from pytest.TForm import TForm
    acap.Application.ShowModalDialog(TForm())

@lisp("lisptest")
@showtime
def mylisp(lst):
    from System.Diagnostics import Process
    return [x.ProcessName for x in Process.GetProcesses()]

@lisp()
def mylisp0(lst):
    return lst[0]

@lisp()
def mylisp1(lst):
    return sorted(lst[0])

@lisp()
def mylisp2(lst):
    return lst[0].OrderBy(lambda x: x)

@lisp()
def mylisp3(lst):
    showwpf()
    return getvalue('WpfTestForm')

@lisp()
def mylisp4(list):
    return faltten([1,[2,3],4,[5,6,[7,8,[9]]]])

@command()
def mysstest():
    respt = ed.GetPoint("选择起点")
    if respt.Status != aced.PromptStatus.OK:
        return
    rescor = ed.GetCorner("选择终点", respt.Value)
    if rescor.Status != aced.PromptStatus.OK:
        return
    pt1, pt2 = respt.Value, rescor.Value
    res = ed.SelectCrossingWindow(pt1, pt2)
    if res.Status != aced.PromptStatus.OK:
        return


@command()
def myswitch():
    case = switcher(0)
    if case[int]:
        print 'int'
    elif case[str]:
        print 'str'

@panel('575123da-2ca8-44a8-b57f-61eb64f5d050', 'testpanel')
class myshowpanel(object):
    text = '测试面板'
    dock = acws.DockSides.Right
    def __init__(self, ps):
        from pytest.TPanel import TPanel
        from System.Drawing import Size
        ps.Style = \
            acws.PaletteSetStyles.NameEditable | \
            acws.PaletteSetStyles.ShowCloseButton | \
            acws.PaletteSetStyles.Notify
        ps.DockEnabled = acws.DockSides.Left | acws.DockSides.Right
        ps.MinimumSize = Size(320, 320)
        ps.Size = Size(320, 320)
        ps.Add("P1", TPanel())
    def show(self, ps):
        #self.dock = acws.DockSides.None
        pass
