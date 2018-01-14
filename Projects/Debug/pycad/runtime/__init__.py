__all__ = ['dbtrans', 'upopen', 'getvalue', 'setvalue']

from dbtrans import dbtrans
from upopen import upopen

__dict = {}

def getvalue(key):
    global __dict
    if __dict.has_key(key):
        return __dict[key]

def setvalue(key, value = None):
    global __dict
    __dict[key] = value