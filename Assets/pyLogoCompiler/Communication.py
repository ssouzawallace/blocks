# -*- coding: utf-8 -*-
#----------------------------BRGOGO----------------------------------------
#Project site: http://br-gogo.sourceforge.net
#
# Name: pyLogo
# Origiginal from: Marcelo Barbosa
# About: Esta é uma versão da linguagem Logo em python para a placa Gogo board
#        (This is a version of Logo in python for the card board Gogo)
#
# Modified by: Felipe Augusto Silva
# email: suportegogo@gmail.com
# compiler version: 0.23
#obs.: version to work with pyGogoMonitor
#
# Modified by: Derek O'Connell
# email: doconnel@gmail.com
#
#-----------------------------------------------------------------------------

import sys
#from time import sleep
import os
import serial
import glob

from gettext import gettext as _

from pyLogoCompiler import Exceptions
from pyLogoCompiler import pyYacc

global ERCP  #This variables stores compiler errors
ERCP = ['one']

# ============================================================

MODULE_DEBUG = False

serialPort = None


# ============================================================

def printFunctionName(caller=""):
    if caller <> "":
        print ">>>", caller
    print ">>> " + sys._getframe(1).f_code.co_name + "()"

def callerName():
    return sys._getframe(2).f_code.co_name + "()"



#================================================================================
#================================================================================
class SerialIF:
    
    # The coms port is set up when ComMgr is instantiated
    def __init__(self, p=None, debug=False):
        global serialPort
        
        serialPort = serial.Serial(p, 9600, bytesize = serial.EIGHTBITS, \
                                       parity=serial.PARITY_NONE, stopbits=serial.STOPBITS_ONE, \
                                       timeout=0.1, xonxoff=0, rtscts=0 )
        self.debug = debug
        
    def rxBytes(self, size=0):
        s = serialPort.read(size)
        r = [0] * len(s)
        for i in range(len(s)):
            r[i] = ord(s[i])
        return tuple(r)

    def rxOneByte(self):
        x = self.rxBytes(1)
        if len(x)>0:
            return x[0]
        return False

    def rx16BitValueLSBFirst(self):
        lowByte  = serialPortIF.rxOneByte() & 0xff          
        highByte = serialPortIF.rxOneByte() #& 0x3
        return (highByte << 8) + lowByte     

    def txByteRxEcho(self, byte):
        serialPort.write(chr(byte))
        self.rxOneByte() # Rx hw echo byte
        # Should check same byte returned here
        return True


    def txBytesNoEcho(self, bytes):
        s=""
        for i in bytes:
            s=s+chr(i)
        serialPort.write(s)

    def txCharsNoEcho(self, chars):
        serialPort.write(chars)


    def txWordMSBFirst(self, word):
        self.txByteRxEcho(word / 256)
        self.txByteRxEcho(word % 256)
        return True


    def flush(self):
        self.rxBytes(10)  # Flush does not work properly
        serialPort.flushInput()
        serialPort.flushOutput()


    def openPort(self):
        try:
            print "opening port..."
            serialPort.open()
            print "port opened"
            return True
        except serial.serialutil.SerialException:
            print "serial.serialutil.SerialException"
            return False

    def closePort(self):
        if serialPort.isOpen():
            serialPort.read(100) # empty buffer serial
            serialPort.close()
        return True
    
    def checkConnection(self):
        printFunctionName()
        if not serialPort.isOpen():
            return False
        self.flush()
        serialPort.write(chr(135))
        i = serialPort.read(1);
        if i == -1:
            print "< Cannot find serial interface"
            return False
        serialPort.write(chr(0))
        j = serialPort.read(1);
        k = serialPort.read(1)
        if j == '' or k == '':
            return False
        if ord(i) == 135 and ord(j) == 0 and ord(k) == 55:
            self.flush()
            return True
        return False

    def getPort(self):
        return serialPort.port

    def scanUnix(self):
        """scan for available ports (serial and USB). return a list"""
        return glob.glob('/dev/ttyUSB*') + glob.glob('/dev/ttyACM*') + glob.glob('/dev/ttyS*')

    def scanNT(self):
        """scan for available ports. return a list of tuples (num, name)"""
        available = []
        for i in range(256):
            try:
                s = serial.Serial(i)
                available.append(i)
                s.close()
            except serial.SerialException:
                pass
        return available


    def autoConnect(self):
        printFunctionName()
        if os.name == 'nt':
            available = self.scanNT()
        else:
            available = self.scanUnix()
            available.remove('/dev/ttyS0') # Internal to XO
        print "< Ports found:", available
        for i in available:
            print "< Trying port: ",i
            serialPort.port = i
            if self.openPort():
                if self.checkConnection():
                    print "< Connected on port:", i
                    return True
                else:
                    self.closePort()
                    print "< Failed to connect to: ", i
        
        return False

    def isUSBVersion(self):
        return str(self.getPort()).find("ACM") != -1
        


#================================================================================
#================================================================================
class CricketIF():

    # Memory map:
    ADDR_CODE_START     = 0       # Code memory start (1280 bytes)
    ADDR_RECDATA_START  = 0x0500  # Recorded data start (2500 bytes)
    ADDR_RECDATA_END    = 0x0EC3  # Recorded data end
    ADDR_CODE_END       = 0x0FEF  # Code memory end (4080 bytes. Note overlap!) 
    # Button Code Vector Addresses: Address of pointer to code to run when button pressed 
    ADDR_BUTTON1_VTR    = 0x0FF0  # Address of pointer to code to run for button 1
    #ADDR_BUTTON2_VTR    = 0x0FF2  # Address of pointer to code to run for button 2
    #ADDR_CRICKET_NAME   = 0x0FF4   # 11 bytes (to 0xFFE)
    #ADDR_AUTOSTART_FLAG = 0x0FFF   # 1 byte
    
    # WIP: Preparation for preserving area of memory used for data recording
    # (- check exactly what compiler spits out, ie, range of function addresses)
    # (- using own variables for now...) 
    ADDR_CODE1_START = ADDR_CODE_START
    ADDR_CODE1_SIZE  = ADDR_RECDATA_START - ADDR_CODE1_START
    ADDR_DATA_START  = ADDR_RECDATA_START
    ADDR_DATA_SIZE   = ADDR_RECDATA_END - ADDR_RECDATA_START
    ADDR_CODE2_START = ADDR_RECDATA_END
    ADDR_CODE2_SIZE  = ADDR_CODE_END - ADDR_RECDATA_END
    
    
    CMD_SET_PTR_HI_BYTE       = 0x80 # 128
    CMD_SET_PTR_LOW_BYTE      = 0x81 # 129
    
    CMD_READ_BYTES_COUNT_HI   = 0x82 # 130
    CMD_READ_BYTES_COUNT_LOW  = 0x83 # 131
    
    CMD_WRITE_BYTES_COUNT_HI  = 0x84 # 132
    CMD_WRITE_BYTES_COUNT_LOW = 0x85 # 133
    CMD_WRITE_BYTES_SENDING   = 0x86 # 134
    
    
    CMD_RUN     = 0x86 # 134
    CMD_CHECK   = 0x87 # 135
    CMD_NAME    = 0x87 # 135
    
    RES_FOUND   = 0x37 # 55, Response to CMD_CHECK if Cricket found


    def __init__(self, debug=False):
        self.byteCount = None
        pass
    
    #def sendMemoryPointer(self, ptr):
    #    self.cmdReadByteCount.(ptr)
    
    def cmdReadByteCount(self, byteCount):
        serialPortIF.txByteRxEcho(CricketIF.CMD_READ_BYTES_COUNT_LOW)
        serialPortIF.txWordMSBFirst(byteCount)
        return True

    def cmdWriteByteCount(self, byteCount):
        serialPortIF.txByteRxEcho(CricketIF.CMD_WRITE_BYTES_COUNT_LOW)
        serialPortIF.txWordMSBFirst(byteCount)
        return True

    def cmdRun(self):
        #serialPort.write(chr(RUN))
        serialPortIF.txByteRxEcho(CricketIF.CMD_RUN)
        return True
    
    def cmdCheckAllCrickets(self):
        serialPortIF.txByteRxEcho(CricketIF.CMD_CHECK)
        serialPortIF.txByteRxEcho(0)
        return serialPortIF.rxOneByte() == CricketIF.RES_FOUND


#==================================== 

    def writeBytesToCricketMemory(self, ptr, bytes):
        printFunctionName()
        print "< Bytes: ", bytes
        try:
            # Shouldn't this be "(ptr + len(bytes) < ADDR_CODE_END" ???
            if len(bytes) < CricketIF.ADDR_CODE_END:
                self.cmdReadByteCount(ptr) # To set memory ptr?
                self.cmdWriteByteCount(len(bytes))
                #serialPortIF.writeBytes(bytes)
                for i in range(len(bytes)):
                    serialPortIF.txByteRxEcho(bytes[i] % 256)
                    serialPortIF.rxOneByte() # Extra echo byte!
        except:
            print "< Problem sending data"
            raise Exceptions.CommunicationProblem


    def cmdSetButton1Pointer(self, vector):
        self.writeBytesToCricketMemory(CricketIF.ADDR_BUTTON1_VTR, [vector/256, vector%256])

#========================================

    def txCompiledCodeEnd(self):
        start = [128,0,0] # 0xF00000 ???
        end = [0]
        
        overhead = len(start) + len(end)
        
        if self.byteCount + overhead < CricketIF.ADDR_CODE_END:
            self.writeBytesToCricketMemory(self.byteCount, start)
            self.cmdSetButton1Pointer(self.byteCount)
            self.byteCount = self.byteCount + overhead
        else:
            self.byteCount = self.byteCount + overhead
            raise serial.serialutil.SerialException
        return overhead


    def txCompiledCode(self, byteCode, byteCodeCount):
        printFunctionName()
        #try:
        self.byteCode  = byteCode
        self.byteCount = byteCodeCount
        self.writeBytesToCricketMemory(CricketIF.ADDR_CODE_START, self.byteCode)
        self.txCompiledCodeEnd()
            #return "< Send success"
        #except serial.serialutil.SerialException:
            #return "< Problem sending, check communication"
            

    def compile(self, logoCode):
        printFunctionName()
        self.byteCode = pyYacc.analisarCodigo(logoCode, ERCP)
        
        if self.byteCode is None:
            print _("ERROR->Look for a text containing one of the words: "),  ERCP[0]
            return False
        else:
            self.byteCount = len(self.byteCode)
            print "< code:", self.byteCode
            return True

    def download(self):
        printFunctionName()
        if not serialPortIF.checkConnection():
            print "< Gogo disconnected"
            #self.showInfo(_("Gogo disconnected"))
            #return False
            raise Exceptions.ConnectionProblem
        
        self.txCompiledCode(self.byteCode, self.byteCount)

    def returnByteCode(self):
        return self.byteCode

    def downloadFirmware(self, HEXFilePath, refreshMethod, completeMethod):
        printFunctionName()

        # iterating on a file line by line 
        with open(HEXFilePath, 'rU') as f:
            # send first string
            # I don't know if this will work for any firmware length - Wallace Souza
            firstString = ':0400000081EF3EF05E'
            serialPortIF.txCharsNoEcho(firstString)

            # overwrite not recognized
            if serialPortIF.rxOneByte() is not 0x80:
                return False

            # get the number of lines on file, excluding the last 3
            num_lines = sum(1 for line in open(HEXFilePath)) - 3

            i = 0
            for line in f:
                serialPortIF.txCharsNoEcho(line)
                
                byteReceived = serialPortIF.rxOneByte()

                #Ready for next
                if byteReceived == 0x11 and refreshMethod is not None:
                    refreshMethod(i, num_lines, line)

                # Finish flag received
                elif byteReceived == 0x55:
                    # send message to clear the progressbar
                    if completeMethod is not None:
                        completeMethod();

                    serialPortIF.closePort()
                    return True
                i = i + 1

        return False


#================================================================================
#================================================================================

class GoGoIF:

    NUMBER_OF_SENSORS = 8
    
    # COMMAND/RESPONSE HEADERS:
    
    HDR_SEND = 0x54,0xFE
    HDR_ACK  = 0X55,0xFF,0xAA
    

    # SINGLE BYTE GOGO COMMANDS:
    
    CMD_PING            = 0x00  # 00  Add Board ID
    CMD_READ_SENSOR     = 0x20  # 32  Add sensor number*4
    CMD_MOTOR_ON        = 0x40, # 64,
    CMD_MOTOR_OFF       = 0x44, # 68,
    CMD_MOTOR_REVERSE   = 0x48, # 72,
    CMD_MOTOR_THISWAY   = 0x4C, # 78, !!!
    CMD_MOTOR_THATWAY   = 0x50, # 80,
    CMD_MOTOR_COAST     = 0x54, # 84,
    CMD_SET_MOTOR_POWER = 0x60  # 96  Add motor power * 4
    
    
    # DOUBLE BYTE GOGO COMMANDS:
    
    BURST_MODE_NORMAL = 0
    BURST_MODE_SLOW   = 1
    
    CMD_TALK_TO_MOTOR   = 0x80,  # 128,   # Byte 2: motors
    CMD_SET_BURST       = 0xA0   # 160    # Add burst mode
    CMD_LED_ON          = 0xC0,0 # 192,0
    CMD_LED_OFF         = 0xC1,0 # 193,0
    CMD_BEEP            = 0xC4,0 # 196,0
    CMD_SET_PWM_DUTY    = 0xC8,  # 200,   # Byte 2: duty cycle
    CMD_UPLOAD_EEPROM   = 0xCC,  # 204,   # >>>>>>>>
    # Add upper 2 bits of the number of bytes to upload. Byte 2: lower 8 bits

    
    # UPLOADING RECORDED DATA
    
    CMD_AUTO_UPLOAD     = 0xCC, 0x00
    HDR_AUTO_UPLOAD_LEN = 0xEE, 0x11
        


    
    def __init__(self, debug=False):
        pass
    
    def sendCmd(self, code, flush=True):
        printFunctionName(callerName())

        #if self.checkConnection():
        #        print "Gogo conectada"
        #else:
        #        print "Gogo desconectada!"
        #       self.showInfo("Gogo desconectada")
        #       return False
        
        serialPortIF.flush()
        
        command = GoGoIF.HDR_SEND + code          
        serialPortIF.txBytesNoEcho(command)
        
        cmd_response = serialPortIF.rxBytes(len(command))
        if cmd_response == '':
            print "< No response to command"
            return False
        ack_response = serialPortIF.rxBytes(3)
        
        print cmd_response, command
        print ack_response, GoGoIF.HDR_ACK
        
        if flush:
            serialPortIF.flush()
        
        #print cmd_response,ack_response
        if cmd_response == command and ack_response == GoGoIF.HDR_ACK:
            print "< Command successfully sent"
            return True

        print "< Error: cmd_response or ack_response"
        return False


    def readSensor(self, sensorNumber=0):
        if sensorNumber >= GoGoIF.NUMBER_OF_SENSORS:
            print "readSensor(): Sensor does not exist:",sensorNumber
            return -1
        
        command = GoGoIF.HDR_SEND + tuple([GoGoIF.CMD_READ_SENSOR + (sensorNumber << 2)])
        serialPortIF.txBytesNoEcho(command)
        
        cmd_response = serialPortIF.rxBytes(3)
        ack = serialPortIF.rxBytes(2)
        
        if not (cmd_response == command and ack == tuple(GoGoIF.HDR_ACK[:2])):
            print "readSensor(): Error reading sensor:",sensorNumber
            #print command,"!=",cmd_response
            #print ack,"!=",tuple(GoGoIF.HDR_ACK[:2])                     
            serialPortIF.flush()
            if serialPortIF.checkConnection():
                return -1
            else:
                raise Exceptions.ConnectionProblem
        
        highByte = serialPortIF.rxOneByte() & 0x3
        lowByte  = serialPortIF.rxOneByte() & 0xff          
        value = (highByte << 8) + lowByte     
        return value


    def motorOn(self):
        return self.sendCmd(GoGoIF.CMD_MOTOR_ON)
    def motorOff(self):
        return self.sendCmd(GoGoIF.CMD_MOTOR_OFF)

    #TODO
    def motorBreak(self):
        self.motorCoast()
    def motorReverse(self):
        return self.sendCmd(GoGoIF.CMD_MOTOR_REVERSE)
    def motorThisway(self):
        return self.sendCmd(GoGoIF.CMD_MOTOR_THISWAY)
    def motorThatway(self):
        return self.sendCmd(GoGoIF.CMD_MOTOR_THATWAY)
    def motorCoast(self):
        return self.sendCmd(GoGoIF.CMD_MOTOR_COAST)


    def talkToMotor(self, motors=""):
        byte = 0
        if 'a' in motors.lower():
            byte = byte + 1
        if 'b' in motors.lower():
            byte = byte + (1 << 1)        
        if 'c' in motors.lower():
            byte = byte + (1 << 2)
        if 'd' in motors.lower():
            byte = byte + (1 << 3)                
        return self.sendCmd(GoGoIF.CMD_TALK_TO_MOTOR + (byte,))

    def setMotorPower(self, power=0):
        if power < 8:
            command = (tuple([GoGoIF.CMD_SET_MOTOR_POWER + (power << 2)]))
        else:
            command = (tuple([GoGoIF.CMD_SET_MOTOR_POWER + (7 << 2)]))
        return self.sendCmd(command)

    #TODO
    def setBurstMode(self, burstBits=0, burstMode=0):
        if burstMode < 0 or burstMode > 1:
            return False
                
        command = tuple([GoGoIF.CMD_SET_BURST + burstMode, burstBits])
        return self.sendCmd(command)

    def ledOn(self):
        return self.sendCmd(GoGoIF.CMD_LED_ON)

    def ledOff(self):
        return self.sendCmd(GoGoIF.CMD_LED_OFF)

    def beep(self):
        return self.sendCmd(GoGoIF.CMD_BEEP)

    #TODO
    def setPwmDuty(self, duty=0):
        if duty >= 0 and duty <= 255:      
            return self.sendCmd(GoGoIF.CMD_SET_PWM_DUTY + (duty,))
        return False
    
    
    def autoUpload(self, uploadCount, progress_cb = None):
        printFunctionName()
        if not serialPortIF.checkConnection():
            print "< Gogo disconnected"
            #self.showInfo(_("Gogo disconnected"))
            #return False
            raise Exceptions.ConnectionProblem
        
        recData = []
        if not uploadCount:
            if not self.sendCmd(GoGoIF.CMD_AUTO_UPLOAD, flush=False):
                print "autoUpload: 1"
                return recData
        else:
            CMD = 0xCC, uploadCount
            if not self.sendCmd(CMD, flush=False):
                print "uploadCount: 1"
                return recData        
        #    3. The Gogo will send four bytes. 0xEE, 0x11, uploadLen (low byte),
        #    uploadLen (high byte). The first two bytes are just headers. The
        #    latter two are in bytes.
        
        # Rx upload-length header bytes (2)
        hdr = serialPortIF.rxBytes(2)
        print "hdr",hdr
        dataBytes = serialPortIF.rx16BitValueLSBFirst()
        dataWords = dataBytes
        
        if dataWords > 0:
            for i in range(dataWords):
                recData.append(serialPortIF.rx16BitValueLSBFirst())
                if progress_cb:
                    progress_cb(i, dataWords)
            if progress_cb:
                progress_cb(dataWords, dataWords)
        return recData
        
        #    Here's an example session:
        #    
        #    Computer->GoGo  :  0x54, 0xFE, 0xCC, 0x00   // command
        #    GoGo->Computer  :  0x54, 0xFE, 0xCC, 0x00   // echo
        #    GoGo->Computer  :  0x55, 0xFF, 0xAA   // ack
        #    GoGo->Computer  :  0xEE, 0x11, 0x06, 0x00   // upload header
        #    GoGo->Computer  :  0x00, 0x00, 0x01, 0x00, 0x02, 0x00  // upload data
        #    
        #    In this example the gogoboard uploads 3 16-bit values: 0, 1, 2 respectively.
        #    
        #    Note. You may find that the gogo seems to be excessively sending stuff
        #    back to the computer with all the echos and acks. This was done to
        #    provide compatibility with the Cricket download protocol. In practice,
        #    once you send the command bytes to the gogo you can just wait for the
        #    upload headers (0xEE, 0x11) while ignoring the others.



#================================================================================
#================================================================================

class GoGoComms:
    
    def __init__(self, portNum=None, debug=False):
        global serialPortIF
        
        serialPortIF = SerialIF(portNum, debug)
        
        self.cricketIF = CricketIF(debug)
        self.gogoIF    = GoGoIF(debug)

    # Public Serial Interface:
    
    def autoConnect(self):
        return serialPortIF.autoConnect()
    
    def getPort(self):
        return serialPortIF.getPort()
        
    def checkConnection(self):
        return serialPortIF.checkConnection()

    def closePort(self):
        return serialPortIF.closePort()
    
    def isUSBVersion(self):
        return serialPortIF.isUSBVersion()
        
    def rxOneByte(self):
        return serialPortIF.rxOneByte()            
        
    # Public Cricket Interface:
    
    def compile(self, text):
        return self.cricketIF.compile(text)
    
    def download(self):
        return self.cricketIF.download()

    def returnByteCode(self):
        return self.cricketIF.returnByteCode() 

    def downloadFirmware(self, HEXFilePath, refreshMethod = None, completeMethod = None):
        return self.cricketIF.downloadFirmware(HEXFilePath, refreshMethod, completeMethod)
    

    # Public GoGo Interface:   
    
    def beep(self):
        return self.gogoIF.beep()
    
    def ledOn(self):
        return self.gogoIF.ledOn()
    
    def ledOff(self):
        return self.gogoIF.ledOff()
    def run(self):
        return self.cricketIF.cmdRun()
    
    def talkToMotor(self, m):
        return self.gogoIF.talkToMotor(m)
    
    def motorOn(self):
        return self.gogoIF.motorOn()
    
    def motorOff(self):
        return self.gogoIF.motorOff()
    
    def motorBreak(self):
        return self.gogoIF.motorBreak()
    
    def motorCoast(self):
        return self.gogoIF.motorCoast()
    
    def setMotorPower(self, power):
        return self.gogoIF.setMotorPower(power)
    
    def motorThisway(self):
        return self.gogoIF.motorThisway()
    
    def motorThatway(self):
        return self.gogoIF.motorThatway()
    
    def motorReverse(self):
        return self.gogoIF.motorReverse()
    
    def setPwmDuty(self, duty):
        return self.gogoIF.setPwmDuty(duty)
    
    def readSensor(self, sensorNumber):
        return self.gogoIF.readSensor(sensorNumber)

    def autoUpload(self, uploadCount, progress_cb = None):
        return self.gogoIF.autoUpload(uploadCount, progress_cb)
    
#========================================================================================================#


if __name__=='__main__':
    GoGoComms(0, False)

