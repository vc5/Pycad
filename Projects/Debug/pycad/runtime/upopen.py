class upopen(object):
    def __init__(self, obj):
        self._obj = obj
        self._isNotifyEnabled = obj.IsNotifyEnabled
        self._isWriteEnabled = obj.IsWriteEnabled
        if self._isNotifyEnabled:
            self._obj.UpgradeFromNotify()
        elif not self._isWriteEnabled:
            self._obj.UpgradeOpen()

    def __enter__(self):
        return self

    def __exit__(self, t, v, b):
        if b is None:
            if self._isNotifyEnabled:
                self._obj.DowngradeToNotify()
            elif not self._isWriteEnabled:
                self._obj.DowngradeOpen()
