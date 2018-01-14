#预加载程序集集合
mgds = [
['acmgd.dll', 'acdbmgd.dll', 'accoremgd.dll'], 
['acdbmgdbrep.dll', 'acmgdinternal.dll', 'accui.dll'], 
['System', 'System.Core']]

#命名空间集合
namespaces = {
"acap" : "Autodesk.AutoCAD.ApplicationServices",
"acdb" : "Autodesk.AutoCAD.DatabaseServices",
"aced" : "Autodesk.AutoCAD.EditorInput",
"acge" : "Autodesk.AutoCAD.Geometry",
"acrx" : "Autodesk.AutoCAD.Runtime",
"acws" : "Autodesk.AutoCAD.Windows",
"acgi" : "Autodesk.AutoCAD.GraphicsInterface",
"acgs" : "Autodesk.AutoCAD.GraphicsSystem",
"acin" : "Autodesk.AutoCAD.Internal",
"acps" : "Autodesk.AutoCAD.PlottingServices"}

#设置中文支持
import sys
reload(sys)
sys.setdefaultencoding("utf-8")

#加载程序集
import clr
for keys in mgds:
    for key in keys:
        try: clr.AddReference(key)
        except: pass

import __builtin__

#设置内建模块
def _builtin(key, value):
    __builtin__.__dict__[key] = value

_builtin('clr', clr)

for key, value in namespaces.items():
    try:
        exec "import %s" % value
        _builtin(key, eval(value))
    except:
        pass

import pycad.sys
for key in pycad.sys.builtins:
    _builtin(key, eval('pycad.sys.' + key))

import keyword

"""
命令函数定义格式
@command(name = None, flags = -1)
def func()
name:命令名，默认为函数名
flags:命令格式

Lisp函数定义格式
@lisp(name = None)
def func(lst)
name:Lisp函数名，默认为函数名
参数name:由lisp数据转换成的表
返回值:一个值、列表或元组，分别对应Lisp中的值、表和点对
"""