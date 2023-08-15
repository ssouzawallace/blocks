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

import os
import ply.lex as lex
from ply.lex import TOKEN

# Lista dos nomes dos tokens.
  #Palavras reservadas
reserved = {
       'to'         : 'TO',
       'end'        : 'END',
       'output'     : 'OUTPUT',
       'repeat'     : 'REPEAT',
       'if'         : 'IF',
       'ifelse'     : 'IFELSE',
       'beep'       : 'BEEP',
       'while'      : 'WHILE',
       'waituntil'  : 'WAITUNTIL',
       'loop'       : 'LOOP',
       'forever'    : 'FOREVER',
       'wait'       : 'WAIT',
       'stop'       : 'STOP',
       'reset'      : 'RESET',
       'send'       : 'SEND',
       'make'       : 'MAKE',
       'resetdp'    : 'RESETDP',
       'record'     : 'RECORD',
       'erase'      : 'ERASE',
       'on'         : 'ON',
       'onfor'      : 'ONFOR',
       'off'        : 'OFF',
       'thisway'    : 'THISWAY',
       'thatway'    : 'THATWAY',
       'rd'         : 'RD',
       'brake'      : 'BRAKE',
       'setsvh'     : 'SETSVH',
       'svr'        : 'SVR',
       'svl'        : 'SVL',
       'setpower'   : 'SETPOWER',
       'ledon'      : 'LEDON',
       'ledoff'     : 'LEDOFF',
       'i2c_start'  : 'I2C_START',
       'i2c_stop'   : 'I2C_STOP',
       'i2c_read'   : 'I2C_READ',
       'i2c_write'  : 'I2C_WRITE',
       'show'       : 'SHOW',
       'and'        : 'AND',
       'or'         : 'OR',
       'xor'        : 'XOR',
       'not'        : 'NOT',
       'timer'      : 'TIMER',
       'timerms'    : 'TIMERMS',
       'serial'     : 'SERIAL',
       'newir?'     : 'NEWIRQ',
       'random'     : 'RANDOM',
       'recall'     : 'RECALL',
       'sensor1'    : 'SENSOR1',
       'sensor2'    : 'SENSOR2',
       'sensor3'    : 'SENSOR3',
       'sensor4'    : 'SENSOR4',
       'sensor5'    : 'SENSOR5',
       'sensor6'    : 'SENSOR6',
       'sensor7'    : 'SENSOR7',
       'sensor8'    : 'SENSOR8',
       'switch1'    : 'SWITCH1', 
       'switch2'    : 'SWITCH2',
       'switch3'    : 'SWITCH3',
       'switch4'    : 'SWITCH4', 
       'switch5'    : 'SWITCH5',
       'switch6'    : 'SWITCH6',
       'switch7'    : 'SWITCH7', 
       'switch8'    : 'SWITCH8',
       'highbyte'   : 'HIGHBYTE',
       'lowbyte'    : 'LOWBYTE',
       'bsend'      : 'BSEND',
       'bsr'        : 'BSR',
       'when'       : 'WHEN',
       'whenoff'    : 'WHENOFF',
       'setdp'      : 'SETDP',
       'fastsend'   : 'FASTSEND',
    }
  #lista de tokens
tokens = ['MINUS', 'PERCENT', 'LPAREN', 'RPAREN', 'TIMES', 'DIVIDE', 'BYTES', 
        'LBRACKET', 'RBRACKET', 'PLUS', 'LESSTHAN', 'EQUALS', 'GREATERTHAN', 'MOTORATTENTION',
        'NUMBERLITERAL', 'PROCEDURENAME', 'RECEIVER', 'REPORTER'] + reserved.values()

#teste = ['uma'] + reserved.values()

#------------------------------------Especificação dos tokens---------------------------------

##states = (
##   ('procedure','exclusive'),
##   ('global','inclusive'),
##)

#Expressoes regulares para tokens simples
t_LPAREN        = r'\('
t_RPAREN        = r'\)'
t_TIMES         = r'\*'
t_DIVIDE        = r'/'
t_PLUS          = r'\+'
t_MINUS         = r'-'
t_PERCENT       = r'\%'
t_LBRACKET      = r'\['
t_RBRACKET      = r'\]'
t_LESSTHAN      = r'\<'
t_EQUALS        = r'\='
t_GREATERTHAN   = r'\>'

#Expressoes regulares com código de ação
    #Definindo dos identificadores
digit        = r'([0-9])'
letter       = r'([a-zA-Z_])'
alphanumeric = r'([a-zA-Z0-9_])'

procname     = r'(' + letter + r'(' + alphanumeric + r')*)'
reporter     = r':(' + letter + r'(' + alphanumeric + r')*)'
receiver     = r'"(' + letter + r'(' + alphanumeric + r')*)'

bytes        = r'0x('+ digit + r')+'
nliteral     = r'(' + digit + r')+'
names        = r'([a-d])+'
motor        = r'(' + names + r'),'


@TOKEN(motor)
def t_MOTORATTENTION(t):
    print " # pyLex -> t_MOTORATTENTION '%s'" % t
    t.type = reserved.get(t.value,'MOTORATTENTION')
    return t

@TOKEN(procname)
def t_PROCEDURENAME(t):
    print " # pyLex -> t_PROCEDURENAME '%s'" % t
    #try:
    t.type = reserved.get(t.value,'PROCEDURENAME') # Checa se é uma palavra reservada
    #except ValueError:
        #print "[Linha: %d Coluna: %d] - Nome %s não é determinado!" % (t.lineno, t.linepos, t.value)
        #t.value = 0
    return t

@TOKEN(reporter)
def t_REPORTER(t):
    print " # pyLex -> t_REPORTER '%s'" % t
    t.type = reserved.get(t.value,'REPORTER') # Checa se é uma palavra reservada
    return t

@TOKEN(receiver)
def t_RECEIVER(t):
    print " # pyLex -> t_RECEIVER '%s'" % t
    t.type = reserved.get(t.value,'RECEIVER') # Checa se é uma palavra reservada
    print "t.type= '%s'" % t.type
    return t

@TOKEN(bytes)
def t_BYTES(t):
    print " # pyLex -> t_BYTES '%s'" % t
    t.type = reserved.get(t.value, 'BYTES')
    return t 

@TOKEN(nliteral)
def t_NUMBERLITERAL(t):
    print " # pyLex -> t_NUMBERLITERAL '%s'" % t
    try:
        t.value = int(t.value)
    except ValueError:
        print "Número %s não é válido!" % t.value
        t.value = 0        
    return t

def t_ID(t):
    r'[a-zA-Z_][a-zA-Z0-9_]*'
    t.type = reserved.get(t.value,'ID')    # Checa se é uma palavra reservada
    return t

def t_COMMENT(t):
    r'(\;*(.|\n)*?\*;)|(\;.*)' # ignora todo um trecho de ;* até *
     #r'(/\*(.|\n)*?*/)|(//.*)'
    pass

#ignora comentarios
t_ignore_COMMENTLINE   = r'\;.*' # comenta apenas a linha

#ignora espaços e quebra de linhas
t_ignore = " \t\n"

#Guarda o valor da linha 
def t_newline(t):
    r'\n+'
    t.lexer.lineno += t.value.count("\n")

def t_error(t):
    print "[Linha: %d Coluna: %d] - Token ilegal '%s'. Não foi encontrado referencia para esse token." % (t.lineno, t.linepos, t.value[0])
    t.lexer.skip(1)

#Executa o analisador léxico
#realiza a otimização do analisador lexico solicitando que crie uma tabela para as
#expressões regulares e as tabelas utilizadas
# Build the lexer
import sys
def build(**kwargs):
    if os.name=='nt':
        return lex.lex(module=sys.modules[__name__], optimize=1,outputdir="", **kwargs) #windows?
    else:
        return lex.lex(module=sys.modules[__name__], outputdir="", **kwargs)
if __name__ == '__main__':
    lex.runmain()


