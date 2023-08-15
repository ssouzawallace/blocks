#!/usr/bin/env python
# -*- coding: utf-8 -*-
#-----------------------------------------------------------------------------
# Name: pyCricketLogo
# Author: Marcelo Barbosa
# Version: 0.1
# About: Esta é uma versão do Cricket Logo em python para a placa Gogo board
#
#-----------------------------------------------------------------------------

import pygtk
pygtk.require('2.0')
import gtk
import pyYacc

#from communication.Communication import Comm
from pyLogoCompiler import pyYacc
from pyLogoCompiler.Communication import GoGoComms

global ERCP
ERCP = ['one']

class TextMode(object):
    def showDialog(self,text,dialog_type,parent):
        dialog = gtk.MessageDialog(parent, gtk.DIALOG_MODAL, dialog_type, gtk.BUTTONS_OK, text)
        dialog.run()
        dialog.destroy()
    def showInfo(self,text,parent=None):
        self.showDialog(text,gtk.MESSAGE_INFO,parent)
    def showWarning(self,text,parent=None):
        self.showDialog(text,gtk.MESSAGE_WARNING,parent)
    def showError(self,text,parent=None):
        self.showDialog(text,gtk.MESSAGE_ERROR,parent)
        
    def quit(self, widget):
        self.window = None
        self.GoGo.closePort()
        del self

    def __init__(self):
        #Carrega a interface a partir do arquivo glade
        
        self.gui = gtk.glade.XML('gui/text.glade')
        self.window = self.gui.get_widget('mainWindow')  
        
        self.GoGo = GoGoComms()

        self.statusbar=self.gui.get_widget('statusbar')
        self.gui.get_widget('statusbarVersion').push(0,'Versão '+VERSION)
        self.notebookMain = self.gui.get_widget('notebookMain')    
        self.liststore=gtk.ListStore(str,str,str) # Name, Unit, #Description
        
        self.sensorsTab     = SensorsTab(self.gui, self.liststore)
        self.sensorTypes    = self.sensorsTab.sensorTypes        
        self.proceduresTab  = ProceduresTab(self.gui, self.GoGo)
        self.consoleTab     = ConsoleTab(self.gui, self.GoGo, self.statusbar, self.liststore, self.sensorTypes)
        self.uploadTab      = UploadTab(self.gui, self.GoGo, self.liststore, self.sensorTypes)     
        
        self.notebookMain.set_current_page(0)    
        self.notebookMain.set_show_tabs(True)    
        
        #Conecta Sinais aos Callbacks:        
        dic = {"gtk_main_quit" : gtk.main_quit}
        self.gui.signal_autoconnect(dic)    
        self.gui.signal_autoconnect(self)
        
        self.window.connect("destroy", self.quit)
        
        #Exibe toda interface:
        self.window.show_all() 
        
        # Tenta conectar-se à GogoBoard
        if self.GoGo.autoConnect():
            i = self.GoGo.getPort()
            if os.name == 'nt':
                self.statusbar.push(0,"Gogo connected on"+" COM "+str(i+1)+" "+"port")
            else:
                self.statusbar.push(0,"Gogo connected on"+" /dev/ttyS"+str(i)+" "+"port")
            
        else:
            print "Gogo not found"
            self.showWarning("There was a problem with the connection\nVerify that the board is properly connected with your computer")
            self.window.destroy()
            return
            
    def __init__(self):
        window = gtk.Window(gtk.WINDOW_TOPLEVEL)
        window.set_size_request(600, 600)
        window.connect("destroy", self.close_application)
        window.set_title("PyCricketLogo")
        window.set_border_width(10)

        box1 = gtk.VBox(False, 0)
        window.add(box1)
        box1.show()

        label = gtk.Label("Codigo")    
        label.set_alignment(0, 0)
        box1.pack_start(label, False, False, 0)
        label.show()

        box2 = gtk.VBox(False, 0) 
        box2.set_border_width(10)
        box1.pack_start(box2, True, True, 0)
        box2.show()

        sw = gtk.ScrolledWindow()
        sw.set_policy(gtk.POLICY_AUTOMATIC, gtk.POLICY_AUTOMATIC)
        textview = gtk.TextView()
        textbuffer = textview.get_buffer()
        sw.add(textview)
        sw.show()
        textview.show()

        box2.pack_start(sw)
        # Load the file textview-basic.py into the text window
        #infile = open("editor.py", "r")

        #if infile:
        #    string = infile.read()
        #    infile.close()
        #    textbuffer.set_text(string)

        separator2 = gtk.HSeparator()
        box1.pack_start(separator2, False, True, 0)
        separator2.show()

        label2 = gtk.Label("Resultado")    
        label2.set_alignment(0, 0)
        box1.pack_start(label2, False, False, 0)
        label2.show()

        box3 = gtk.HBox(False, 20)
        box3.set_border_width(10)
        box1.pack_start(box3, True, True, 0)
        box3.show()

        sw2 = gtk.ScrolledWindow()
        sw2.set_policy(gtk.POLICY_AUTOMATIC, gtk.POLICY_AUTOMATIC)
        textview2 = gtk.TextView()
        textbuffer2 = textview2.get_buffer()
        sw2.add(textview2)
        sw2.show()
        textview2.show()

        box3.pack_start(sw2)

        separator = gtk.HSeparator()
        box1.pack_start(separator, False, True, 0)
        separator.show()

        box2 = gtk.HBox(False, 10)
        box2.set_border_width(10)
        box1.pack_start(box2, False, True, 0)
        box2.show()

        button = gtk.Button("Compilar")
        button.connect("clicked", self.compilar, textbuffer, textbuffer2)
        box2.pack_start(button, True, True, 0)        
        button.show()
        button1 = gtk.Button("Enviar")
        button1.connect("clicked", self.enviarGogo, textbuffer, textbuffer2)
        box2.pack_start(button1, True, True, 0)        
        button1.show()
        button2 = gtk.Button("Fechar")
        button2.connect("clicked", self.close_application)
        box2.pack_start(button2, True, True, 0)
        button2.set_flags(gtk.CAN_DEFAULT)
        button2.grab_default()
        button2.show()
        
        window.show()            
    
    def toggle_editable(self, checkbutton, textview):
        textview.set_editable(checkbutton.get_active())

    def toggle_cursor_visible(self, checkbutton, textview):
        textview.set_cursor_visible(checkbutton.get_active())

    def toggle_left_margin(self, checkbutton, textview):
        if checkbutton.get_active():
            textview.set_left_margin(50)
        else:
            textview.set_left_margin(0)

    def toggle_right_margin(self, checkbutton, textview):
        if checkbutton.get_active():
            textview.set_right_margin(50)
        else:
            textview.set_right_margin(0)

    def new_wrap_mode(self, radiobutton, textview, val):
        if radiobutton.get_active():
            textview.set_wrap_mode(val)

    def new_justification(self, radiobutton, textview, val):
        if radiobutton.get_active():
            textview.set_justification(val)

    def close_application(self, widget):
        gtk.main_quit()

    def compilar(self, widget, textbuffer, textbuffer2):
        compilado = '%s ' % self.ler_token(widget, textbuffer, textbuffer2)
        if  len(compilado) <= 5 :
            global ERCP
            #print "CCCCCCCCCCCCCCCCOMPILOU COM ERRRO  ?"
            textbuffer2.set_text("ERROR->Look for a text containing one of the words: " + ERCP[0] )
            #print "????????erroCOMP"
            #print ERCP[0]           
            #print "????????ERCP"

            
        else :
            textbuffer2.set_text(compilado)

    def enviarGogo(self, widget, textbuffer, textbuffer2):
        codigo = self.ler_token(widget, textbuffer, textbuffer2)
        c = pyCricketComunic.Comunic(0)    
        result = c.enviar(codigo)
        c.fechaPorta()
        dialog = gtk.MessageDialog(None, gtk.DIALOG_MODAL, gtk.MESSAGE_INFO,
        gtk.BUTTONS_NONE, result)
        dialog.add_button(gtk.STOCK_CLOSE, gtk.RESPONSE_CLOSE)
        dialog.run()
        dialog.destroy()
        
    def ler_token(self, widget, textbuffer, textbuffer2):
        global ERCP
        start = textbuffer.get_start_iter()
        end = textbuffer.get_end_iter()
        texto = pyYacc.analisarCodigo(textbuffer.get_text(start, end),ERCP)
        print "ERCP", ERCP[0]
        return texto        
        #for i in texto:
        #    armazena = armazena + str(i) + '\n'