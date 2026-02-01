Friend Module Program
    Friend ReadOnly bgmGetReady As New AudioPlayer("Assets/get_ready.mp3")
    Friend ReadOnly bgmMainTheme As New AudioPlayer("Assets/main_theme.mp3")
    Friend ReadOnly sndLifeLost As New AudioPlayer("Assets/life_lost.wav")
    Friend ReadOnly sndFruitEaten As New AudioPlayer("Assets/fruit_eaten.wav")
    Friend ReadOnly sndDotEaten As New AudioPlayer("Assets/dot_eaten.wav")
    Friend ReadOnly sndGhostEaten As New AudioPlayer("Assets/ghost_eaten.wav")
    Friend ReadOnly sndLifeGained As New AudioPlayer("Assets/life_gained.wav")
    Friend ReadOnly sndPowerEaten As New AudioPlayer("Assets/power_eaten.wav")

    Public ReadOnly Property MapEditor As New MapEditor
    Public ReadOnly Property MazeEditorInstr As New MazeEditorInstructions
    Public ReadOnly Property Instructions As New Instructions

    <STAThread()>
    Friend Sub Main()
        Application.SetHighDpiMode(HighDpiMode.DpiUnaware)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New PacMan)
    End Sub
End Module
