# Pycad
PyIDE For AutoCad2014 - 2016

1、打开AutoCad，用`netload`命令加载NFox.Python.dll

2、加载成功后，在命令行键入`pye`命令可打开编辑器

3、pytest项目有创建命令、lisp函数、面板的例程

```python
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

@lisp()  
def mylisp1(lst):  
    return sorted(lst[0])  

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
```

![Image text](https://github.com/xsfhlzh/Pycad/blob/master/Src/0.png)
