Friend Module Program
    Public ReadOnly mapEditor As New MapEditor
    Public ReadOnly mazeEditorInstr As New MazeEditorInstructions
    Public ReadOnly instructions As New Instructions

    <STAThread()>
    Friend Sub Main()
        Application.SetHighDpiMode(HighDpiMode.DpiUnaware)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New PacMan)
    End Sub

End Module
