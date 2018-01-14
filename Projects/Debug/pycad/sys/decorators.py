class lisp(object):
    '''
Lisp函数定义格式
@lisp(name = None)
def func(lst)
参数：
name Lisp函数名，默认为函数名

def func(lst):
参数：
name 由lisp数据转换成的表
返回值：
一个值、列表或元组，分别对应Lisp中的值、表和点对
'''
    def __init__(self, name = None):
        self.name = name
    def __call__(self, args):
        if callable(args):
            if not self.name:
                self.name = args.__name__
            self.func = args
            pye.addlisp(self)
            return self
        else:
            ed.getcurr()
            if isinstance(args, list):
                import pdb
                pdb.set_trace()
                res = self.func(args)
                return res
            else:
                return conv.ToLispData(self.func(conv.FromLispData(args)))

class command(object):
    '''
命令函数定义格式
@command(name = None, flags = -1)
def func()
参数：
name 命令名，默认为函数名
flags 命令格式，acrx.CommandFlags枚举
'''
    def __init__(self, name = None, flags = -1):
        self.name = name
        self.flags = flags
    def __call__(self, args = None):
        if callable(args):
            if not self.name:
                self.name = args.__name__
            self.func = args
            pye.addcommand(self)
            return self
        else:
            ed.getcurr()
            if args:
                import pdb
                pdb.set_trace()
                self.func()
            else:
                self.func()

class panel(object):
    def __init__(self, tid, name = None, flags = -1):
        from System import Guid
        self.guid = Guid(tid)
        self.name = name
        self.flags = flags
    def __call__(self, cls = None):
        if cls:
            self.cls = cls
            if not self.name:
                self.name = cls.__name__
            if hasattr(cls, 'text'):
                self.text = cls.text
            else:
                self.text = cls.__name__
            if not hasattr(cls, 'dock'):
                setattr(cls, 'dock', acws.DockSides.None)
            self.obj = None
            self.ps = None
            pye.addcommand(self)
            return self
        else:
            if not self.obj:
                self.ps = acws.PaletteSet(self.text, self.name, self.guid)
                self.obj = self.cls(self.ps)
            self.ps.Visible = True
            if hasattr(self.obj, 'show'):
                try: self.obj.show(self.ps)
                except: pass
            self.ps.Dock = self.obj.dock

def showtime(func):
    import functools
    @functools.wraps(func)
    def _func(*args, **kwargs):
        from datetime import datetime
        oldtime = datetime.now()
        res = func(*args, **kwargs)
        t = datetime.now() - oldtime
        print "函数名称:%s.%s" % (func.__module__, func.__name__)
        print "花费时间:%f" % t.total_seconds()
        return res
    return _func