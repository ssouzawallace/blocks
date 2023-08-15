# -*- coding: utf-8 -*-
#----------------------------BRGOGO----------------------------------------
#Project site: http://br-gogo.sourceforge.net
#
# Name: pyLogo
# Origiginal from: Marcelo Barbosa
# About: Esta é uma versão da linguagem Logo em python para a placa Gogo board
#
# Modified by: Felipe Augusto Silva
# email: suportegogo@gmail.com
# compiler version: 0.23
#-----------------------------------------------------------------------------

class UnknowSymbol(Exception):
    pass
class BracketError(Exception):
    pass    
class BlockTooLong(Exception):
    pass
class CodeTooLong(Exception):
    pass    
class TooManyGlobals(Exception):
    pass
class DuplicatedSymbol(Exception):
    pass
class ParentesisError(Exception):
    pass
    
    
    
class CommunicationProblem(Exception):
    pass
    
class ConnectionProblem(Exception):
    pass
