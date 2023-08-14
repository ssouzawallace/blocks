import Exceptions
import pyYacc

global ERCP  #This variables stores compiler errors
ERCP = ['one']

def compile():
    output = pyYacc.analisarCodigo("to start asdbeep end", ERCP)
    print output
    return output, ERCP