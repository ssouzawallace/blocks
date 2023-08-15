import sys
sys.path.insert(1, './py-yacc-3.0.1')

import Exceptions
import pyYacc

global ERCP  #This variables stores compiler errors
ERCP = ['one']

def compile():
    code = "to start beep end"
    code = """
        to dobeep
            repeat 10 [ beep wait 1]
        end
    """
    output = pyYacc.analisarCodigo(code, ERCP)
    
    return output
#    return ERCP
