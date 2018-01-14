#coding:utf-8

'''初始化模块'''

builtins = [
'conv', 'ed', 'lisp', 'command', 'panel', 'showtime',
'help', 'guid', 'switcher', 'faltten']

import conv
from decorators import lisp, command, panel, showtime
from wraps import ed

ed.init()

def help(o):
    import pydoc
    pydoc.help(o)

def guid():
    from System import Guid
    return Guid.NewGuid()

class switcher(object):
    '''
case = switcher(value)
if case(somevalue) or case[sometype]:
    dosomething'''
    def __init__(self, value):
        self.value = value
    def __call__(self, *args):
        return self.value in args
    def __getitem__(self, *args):
        return isinstance(self.value, args)
    def __lt__(self, other):
        return self.value < other
    def __gt__(self, other):
        return self.value > other
    def __le__(self, other):
        return self.value <= other
    def __ge__(self, other):
        return self.value >= other
    def match(self, func):
        return func(self.value)

def faltten(nested):
    '''展开生成器'''
    try:
        for sublist in nested:
            for element in faltten(sublist):
                yield element
    except TypeError:
        yield nested

