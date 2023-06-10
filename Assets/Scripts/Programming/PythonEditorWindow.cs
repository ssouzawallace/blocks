using UnityEngine;  
using UnityEditor;  
using IronPython;  
using IronPython.Modules;  
using System.Text;  
using System.Collections.Generic;

#if UNITY_EDITOR

using Microsoft.Scripting.Hosting; 

// derive from EditorWindow for convenience, but this is just a fire-n-forget script
public class PythonEditorWindow : EditorWindow  
{  
	// class member properties
	Vector2 _historyScroll;  
	Vector2 _scriptScroll;  
	bool _showHistory = true;  
	int _historyPaneHeight = 192;  
	string _historyText = "history";  
	string _scriptText = "script";  
	string _lastResult = "";  
	TextEditor _TEditor;  
	GUIStyle consoleStyle = new GUIStyle ();  
	GUIStyle historyStyle = new GUIStyle ();  
	Microsoft.Scripting.Hosting.ScriptEngine _ScriptEngine;  
	Microsoft.Scripting.Hosting.ScriptScope _ScriptScope;  

	[MenuItem ("Python/LogoCompileTest")]
	public static void LogoCompileTest () {
		// create the engine  
		var ScriptEngine = IronPython.Hosting.Python.CreateEngine();  
		// and the scope (ie, the python namespace)  
		var ScriptScope = ScriptEngine.CreateScope();  

		// execute a string in the interpreter and grab the variable  
		string dllpath = System.IO.Path.GetDirectoryName (  
		                                                  (typeof(ScriptEngine)).Assembly.Location).Replace (  
		                                                   "\\", "/");  
		StringBuilder example = new StringBuilder();  
		example.AppendLine ("import sys");  
		example.AppendFormat ("sys.path.append(\"{0}\")\n", dllpath + "/Lib");  
		example.AppendFormat ("sys.path.append(\"{0}\")\n", dllpath + "/DLLs");  
		example.AppendFormat ("sys.path.append(\"{0}\")\n", dllpath + "/pyLogoCompiler");
		example.AppendLine("import Compiler");  
		example.AppendLine("output, ERCP = Compiler.compile()");  

		var ScriptSource = ScriptEngine.CreateScriptSourceFromString(example.ToString());  
		ScriptSource.Execute(ScriptScope);  

		IronPython.Runtime.List came_from_script = ScriptScope.GetVariable<IronPython.Runtime.List>("output");  

		IronPython.Runtime.List ERCP = ScriptScope.GetVariable<IronPython.Runtime.List>("ERCP");  

		Debug.Log("Saida: ");
		if (came_from_script != null) {
			string toLog = "[";
			foreach (var elem in came_from_script) {
				toLog += elem.ToString() + " ";
			}
			toLog += "]";
			Debug.Log(toLog);
		}
		if (ERCP != null) {
			string toLog = "[";
			foreach (var elem in ERCP) {
				toLog += elem.ToString() + " ";
			}
			toLog += "]";
			Debug.Log(toLog);
		}

	}

	[MenuItem ("Python/PythonEditorWindow")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		PythonEditorWindow window = (PythonEditorWindow)EditorWindow.GetWindow (typeof (PythonEditorWindow));
		window.Show();
	}

	// initialization logic (it's Unity, so we don't do this in the constructor!
	public void OnEnable ()  
	{     
		// pure gui stuff
		consoleStyle.normal.textColor = Color.yellow;  
		consoleStyle.margin = new RectOffset (20, 10, 10, 10);  
		historyStyle.normal.textColor = Color.white;  
		historyStyle.margin = new RectOffset (20, 10, 10, 10);  
		
		// load up the hosting environment  
		var options = new Dictionary<string, object>();
		options["Frames"] = true;
		options["FullFrames"] = true;
		_ScriptEngine = IronPython.Hosting.Python.CreateEngine (options);  
		_ScriptScope = _ScriptEngine.CreateScope ();  
		
		// load the assemblies for unity, using types  
		// to resolve assemblies so we don't need to hard code paths  
		//_ScriptEngine.Runtime.LoadAssembly (typeof(PythonFileIOModule).Assembly);  
		_ScriptEngine.Runtime.LoadAssembly (typeof(GameObject).Assembly);  
		_ScriptEngine.Runtime.LoadAssembly (typeof(Editor).Assembly);  
		string dllpath = System.IO.Path.GetDirectoryName (  
		                                                  (typeof(ScriptEngine)).Assembly.Location).Replace (  
		                                                   "\\", "/");  
		// load needed modules and paths  
		StringBuilder init = new StringBuilder ();  
		init.AppendLine ("import sys");  
		init.AppendFormat ("sys.path.append(\"{0}\")\n", dllpath + "/Lib");  
		init.AppendFormat ("sys.path.append(\"{0}\")\n", dllpath + "/DLLs");  
		init.AppendFormat ("sys.path.append(\"{0}\")\n", dllpath + "/pyLogoCompiler");
		init.AppendLine ("import UnityEngine as unity");  
		init.AppendLine ("import UnityEditor as editor");  
		init.AppendLine ("import StringIO");  
		init.AppendLine ("unity.Debug.Log(\"Python console initialized\")");  
		init.AppendLine ("__print_buffer = sys.stdout = StringIO.StringIO()");  
		var ScriptSource = _ScriptEngine.CreateScriptSourceFromString (init.ToString ());  
		ScriptSource.Execute (_ScriptScope);  
	}   
	
	public void OnGUI ()  
	{  
		HackyTabSubstitute ();  // this is explained below...
		
		// top pane with history  
		_showHistory = EditorGUILayout.Foldout (_showHistory, "History");  
		if (_showHistory) {  
			EditorGUILayout.BeginVertical (GUILayout.ExpandWidth (true),   
			                               GUILayout.Height (_historyPaneHeight));  
			if (GUILayout.Button ("Clear history")) {  
				_historyText = "";  
			}  
			_historyScroll = EditorGUILayout.BeginScrollView (_historyScroll);  
			EditorGUILayout.TextArea (_historyText,   
			                          historyStyle,   
			                          GUILayout.ExpandWidth (true),   
			                          GUILayout.ExpandHeight (true));          
			EditorGUILayout.EndScrollView ();  
			EditorGUILayout.EndVertical ();  
		}  
		// draggable splitter  
		GUILayout.Box ("", GUILayout.Height (8), GUILayout.ExpandWidth (true));  
		//Lower pane for script editing  
		EditorGUILayout.BeginVertical (GUILayout.ExpandWidth (true),   
		                               GUILayout.ExpandHeight (true));  
		_scriptScroll = EditorGUILayout.BeginScrollView (_scriptScroll);  
		GUI.SetNextControlName ("script_pane");  
		// note use of GUILayout NOT EditorGUILayout.  
		// TextEditor is not accessible for EditorGUILayout!  
		_scriptText = GUILayout.TextArea (_scriptText,   
		                                  consoleStyle,  
		                                  GUILayout.ExpandWidth (true),   
		                                  GUILayout.ExpandHeight (true));          
		_TEditor = (TextEditor)GUIUtility.GetStateObject (typeof(TextEditor), GUIUtility.keyboardControl);  
		EditorGUILayout.EndScrollView ();  
		EditorGUILayout.BeginHorizontal ();  
		if (GUILayout.Button("Clear", GUILayout.ExpandWidth(true)))  
		{  
			_scriptText = "";  
			GUI.FocusControl("script_pane");  
		}  
		if (GUILayout.Button ("Execute and clear", GUILayout.ExpandWidth (true))) {  
			Intepret (_scriptText);  
			_scriptText = "";  
			GUI.FocusControl("script_pane");  
		}  
		if (GUILayout.Button ("Execute", GUILayout.ExpandWidth (true))) {  
			Intepret (_scriptText);  
		}  
		EditorGUILayout.EndHorizontal ();  
		EditorGUILayout.EndVertical ();      
		// mimic maya Ctrl+enter = execute  
		if (Event.current.isKey &&  
		    Event.current.keyCode == KeyCode.Return &&  
		    Event.current.type == EventType.KeyUp &&  
		    Event.current.control) {  
			Intepret (_scriptText);  
		}  
		// drag the splitter  
		if (Event.current.isMouse & Event.current.type == EventType.MouseDrag)  
		{_historyPaneHeight = (int) Event.current.mousePosition.y - 28;  
			Repaint();  
		}  
	}  

	private void HackyTabSubstitute ()  
	{  
		string _t = _scriptText;  
		string[] lines = _scriptText.Split ('\n');  
		for (int i = 0; i< lines.Length; i++) {  
			if (lines [i].IndexOf ('`') >= 0) {  
				lines [i] = "  " + lines [i];  
//				_TEditor.select.selectPos = _TEditor.pos = _TEditor.pos + 3;  
			}  
			if (lines [i].IndexOf ("  ") >= 0 && lines [i].IndexOf ("~") >= 0) {  
				if (lines [i].StartsWith ("  "))  
					lines [i] = lines [i].Substring (4);  
//				_TEditor.selectPos = _TEditor.pos = _TEditor.pos - 4;  
			}  
			lines [i] = lines [i].Replace ("~", "");  
			lines [i] = lines [i].Replace ("`", "");  
		}  
		_scriptText = string.Join ("\n", lines);  
		if (_scriptText != _t)  
			Repaint ();  
	}  

	// Pass the script text to the interpreter and display results  
	private void Intepret (string text_to_interpret)  
	{  
		object result = null;  
		try {  
			Undo.RegisterSceneUndo ("script");  
			var scriptSrc = _ScriptEngine.CreateScriptSourceFromString (text_to_interpret);  
			_historyText += "\n";  
			_historyText += text_to_interpret;  
			_historyText += "\n";  
			result = scriptSrc.Execute (_ScriptScope);  
		}   
		// Log exceptions to the console too  
		catch (System.Exception e) {  
			Debug.LogException (e);  
			_historyText += "\n";  
			_historyText += "#  " + e.Message + "\n";  
		}   
		finally {  
			// grab the __print_buffer stringIO and get its contents  
			var print_buffer = _ScriptScope.GetVariable ("__print_buffer");  
			var gv = _ScriptEngine.Operations.GetMember (print_buffer, "getvalue");  
			var st = _ScriptEngine.Operations.Invoke (gv);  
			var src = _ScriptEngine.CreateScriptSourceFromString ("__print_buffer = sys.stdout = StringIO.StringIO()");  
			src.Execute (_ScriptScope);  
			if (st.ToString ().Length > 0) {  
				_historyText += "";  
				foreach (string l in st.ToString().Split('\n'))  
				{  
					_historyText += "  " + l + "\n";  
				}  
				_historyText += "\n";  
			}  
			// and print the last value for single-statement evals  
			if (result != null) {  
				_historyText += "#  " + result.ToString () + "\n";  
			}  
			int lines = _historyText.Split ('\n').Length;  
			_historyScroll.y += (lines * 19);                  
			Repaint ();  
		}  
	}  
}  
#endif