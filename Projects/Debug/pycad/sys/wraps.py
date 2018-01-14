class ed(object):
    '''
文档控制台封装类'''
    _ed = None
    @classmethod
    def init(cls):
        '''
封装aced.Editor类的属性到ed
对函数进行特殊封装'''
        for key in aced.Editor.__dict__:
            if key != '__doc__':
                t = type(aced.Editor.__dict__[key])
                if t.__name__ == 'method_descriptor':
                    setattr(cls, key, _preperty(key, True))
                else:
                    setattr(cls, key, _preperty(key))
    @classmethod
    def getcurr(cls):
        cls._ed = acap.Application.DocumentManager.MdiActiveDocument.Editor

class _preperty(object):
    def __init__(self, name, isfunc = None):
        self.__name__ = name
        self._isfunc = isfunc
        self._func = None

    def __get__(self, instance, owner):
        if not owner._ed:
            owner.getcurr()
        #获取当前实例属性
        attr = getattr(owner._ed, self.__name__)
        if self._isfunc:
            self._func = attr
            self.__doc__ = self._func.__doc__
            return self
        return attr
    def __call__(self, *args, **kargs):
        try:
            pye.hide()
            return self._func(*args, **kargs)
        finally:
            pye.show()
    def __repr__(self):
        return '<built-in method %s of Editor object at 0x%016X>' % (
            self.__name__, id(self))

