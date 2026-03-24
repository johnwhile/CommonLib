Inputs Namespace provide some base functions to read mouse and keyboard data, need to implement a complete input manager class.

* HOOKS : implement a low level mouse and keyboard reader, indipendent from application but work in same thread.
          The change of state is made by window when it wants.

* MESSAGESFILTER: implement a application's level mouse and keyboard reader, work only inside application area so not very usefull for
                  fullscreen game. The change of state is made by window when it wants.

* UPDATING: Get states only when require, work only at application's level, for fullscreen or lowlevel use DirectInput.


"Hook" can be used to implement a low level version of "Updating" method.