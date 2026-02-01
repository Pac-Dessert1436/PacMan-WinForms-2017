Public Class MapEditor

    ' Declare constants for dimensions of the GUI, scale and FPS.
    Public Const CLIENT_WIDTH = 224
    Public Const CLIENT_HEIGHT = 288
    Public Const CLIENT_SCALE = 2
    Public Const FPS = 60

    ' Declare enumerator 'mode' to hold the possible actions in the map editor.
    Public Enum Mode As Integer
        addBlock = 0
        deleteBlock = 1
        addDot = 2
        deleteDot = 3
        addEnergizer = 4
        deleteEnergizer = 5
    End Enum

    ' Declare global variables for the map editor.
    ' Create, with its events, a variable to hold a new instance of the game engine.
    Dim WithEvents MapEngine As GameEngine
    Dim map As GameEngine.Map
    Dim debugMap As GameEngine.Map          ' DEBUG
    Dim mapCursor As GameEngine.Sprite
    Dim mapPacman As GameEngine.Sprite

    ' New maze object
    Private ReadOnly mapMaze As New Maze

    ' New temporary maze object (used to determine whether a block deletion is valid)
    Private ReadOnly mapMazeDelete As New Maze

    ' Block placement mode (default to addBlock)
    Dim blockMode As Integer = Mode.addBlock
    
    ' Flag to track if icons have been initialized
    Dim initializedIcons As Boolean = False

    Private Sub MapEditor_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' On Form Load, initialize the map editor
        Initialize()

    End Sub

    Public Sub MapEngine_geGameLogic() Handles MapEngine.GE_GameLogic

        ' This is the geGameLogic event.
        ' This is triggered once per frame by the gameEngine.

        ' There is no game logic

    End Sub

    Public Sub MapEngine_geRenderScene() Handles MapEngine.GE_RenderScene

        ' This is the geRenderScene event.
        ' This is triggered once per frame by the gameEngine.

        ' If there is mouse data (i.e. the mouse if over the gameSurface)...
        If MapEngine.GetMouse IsNot Nothing Then

            ' Set the mapCursor point (measured in 8x8 tile units)
            mapCursor.Point = New Point(Int(MapEngine.GetMouse.Location.X / (8 * CLIENT_SCALE)) * 8, Int(MapEngine.GetMouse.Location.Y / (8 * CLIENT_SCALE)) * 8)

        End If

    End Sub

    Public Sub MapEngine_geMouseMove(sender As Object, e As MouseEventArgs) Handles MapEngine.GE_MouseMove

        ' This is the geMouseMove event.
        ' This is triggered once per frame by the gameEngine and reports mouse movement

        ' Get the current mouse position
        Dim pos As New Point(Int(e.Location.X / (8 * CLIENT_SCALE)), Int(e.Location.Y / (8 * CLIENT_SCALE)))

        ' Set the mapCursor point (measured in 8x8 tile units)
        mapCursor.Point = New Point(pos.X * 8, pos.Y * 8)

        ' As long as the mouse cursor is within the gameSurface area...
        If pos.X < 27 And pos.Y < 30 Then

            ' Depending upon the mode indicated by the blockMode variable, a different outlining box is shown.
            Select Case blockMode

                Case Mode.addBlock

                    ' If addBlock mode is active then check whether the tiles under the cursor are fixed
                    ' If they are then the cursor is red, otherwise it's green
                    If mapMaze.MazeBlockFixed(pos.X, pos.Y) Or mapMaze.MazeBlockFixed(pos.X + 1, pos.Y) Or mapMaze.MazeBlockFixed(pos.X, pos.Y + 1) Or mapMaze.MazeBlockFixed(pos.X + 1, pos.Y + 1) Then
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(0, 0)
                    Else
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(1, 1)
                    End If

                Case Mode.deleteBlock

                    ' If deleteBlock mode is active then check whether deletion is allowed at this position
                    ' If it is not then the cursor is red, otherwise it's green
                    If CheckDelete(pos.X, pos.Y) = False Then
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(0, 0)
                    Else
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(1, 1)
                    End If

                Case Mode.addEnergizer

                    ' If addEnergizer mode is active then check whether the tile under the cursor is fixed or a block
                    ' If is is then the cursor is red, otherwise it's green
                    If mapMaze.MazePathType(pos.X, pos.Y) <> Maze.PathType.block And mapMaze.MazeBlockFixed(pos.X, pos.Y) = False Then
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(3, 3)
                    Else
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(2, 2)
                    End If

                Case Mode.addDot

                    ' If addDot mode is active then check whether the tile under the cursor is fixed or a block
                    ' If is is then the cursor is red, otherwise it's green
                    If mapMaze.MazePathType(pos.X, pos.Y) <> Maze.PathType.block And mapMaze.MazeBlockFixed(pos.X, pos.Y) = False Then
                        ' If so the outline box is set to green, to indicate that a dot can be placed in that position.
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(3, 3)
                    Else
                        ' Otherwise, the outline box is set to red, to indicate that a dot cannot be placed in that position.
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(2, 2)
                    End If

                Case Mode.deleteDot

                    ' If deleteDot mode is active then check whether the tile under the cursor is fixed or a not a dot
                    ' If is is then the cursor is red, otherwise it's green
                    If mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.dot And mapMaze.MazeBlockFixed(pos.X, pos.Y) = False Then
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(3, 3)
                    Else
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(2, 2)
                    End If

                Case Mode.deleteEnergizer

                    ' If deleteEnergizer mode is active then check whether the tile under the cursor is fixed or a not an energizer
                    ' If is is then the cursor is red, otherwise it's green
                    If mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.energizer And mapMaze.MazeBlockFixed(pos.X, pos.Y) = False Then
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(3, 3)
                    Else
                        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(2, 2)
                    End If

            End Select
        End If

        ' If debugging is enabled...
        If ToolStrip_Debug.Checked = True Then

            ' Draw a representation of the 6x6 blocks that surround the cursor
            ' This shows fixed blocks, fixed non-blocks, blocks, and non-blocks.
            Dim off As Integer
            Dim size As Integer
            For y = -2 To 3
                For x = -2 To 3
                    If mapMaze.MazeBlockFixed(pos.X + x, pos.Y + y) = True Then
                        off = 12
                    Else
                        off = 0
                    End If
                    If blockMode = Mode.addBlock Or blockMode = Mode.deleteBlock Then
                        size = 1
                    Else
                        size = 0
                    End If
                    If mapMaze.Path(New Point(pos.X + x, pos.Y + y)).pathType = Maze.PathType.block Then
                        If x < 0 Or x > size Or y < 0 Or y > size Then
                            debugMap.Value(New Point(2 + x, 2 + y)) = 51 + off
                        Else
                            If mapCursor.AnimationRange.Min = 0 Then
                                debugMap.Value(New Point(2 + x, 2 + y)) = 55 + off
                            Else
                                debugMap.Value(New Point(2 + x, 2 + y)) = 53 + off
                            End If
                        End If
                    Else
                        If x < 0 Or x > size Or y < 0 Or y > size Then
                            debugMap.Value(New Point(2 + x, 2 + y)) = 52 + off
                        Else
                            If mapCursor.AnimationRange.Min = 0 Then
                                debugMap.Value(New Point(2 + x, 2 + y)) = 56 + off
                            Else
                                debugMap.Value(New Point(2 + x, 2 + y)) = 54 + off
                            End If
                        End If
                    End If
                Next
            Next

        End If

    End Sub

    Public Sub MapEngine_geMouseUp(sender As Object, e As MouseEventArgs) Handles MapEngine.GE_MouseUp

        ' This is the geMouseUp event.
        ' This is triggered once per frame by the gameEngine and reports button releases on the mouse

        ' Get the current mouse position
        Dim pos = New Point(Int(e.Location.X / (8 * CLIENT_SCALE)), Int(e.Location.Y / (8 * CLIENT_SCALE)))

        ' Depending upon the mode indicated by the blockMode variable, a different outlining box is shown.
        Select Case blockMode

            Case Mode.addBlock

                ' If addBlock mode is active then check whether the tiles under the cursor are fixed
                ' If they are not then add a block
                If Not (mapMaze.MazeBlockFixed(pos.X, pos.Y) Or mapMaze.MazeBlockFixed(pos.X + 1, pos.Y) Or mapMaze.MazeBlockFixed(pos.X, pos.Y + 1) Or mapMaze.MazeBlockFixed(pos.X + 1, pos.Y + 1)) Then
                    mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.block
                    mapMaze.MazePathType(pos.X + 1, pos.Y) = Maze.PathType.block
                    mapMaze.MazePathType(pos.X, pos.Y + 1) = Maze.PathType.block
                    mapMaze.MazePathType(pos.X + 1, pos.Y + 1) = Maze.PathType.block
                End If

            Case Mode.deleteBlock

                ' If deleteBlock mode is active then check whether deletion is allowed at this position
                ' If it is then delete the block by replacing with dots
                If CheckDelete(pos.X, pos.Y) = True Then
                    mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.dot
                    mapMaze.MazePathType(pos.X + 1, pos.Y) = Maze.PathType.dot
                    mapMaze.MazePathType(pos.X, pos.Y + 1) = Maze.PathType.dot
                    mapMaze.MazePathType(pos.X + 1, pos.Y + 1) = Maze.PathType.dot
                End If

            Case Mode.addEnergizer

                ' If addEnergizer mode is active then check whether the tile under the cursor is fixed or a block
                ' If is is not then add an energizer
                If mapMaze.MazePathType(pos.X, pos.Y) <> Maze.PathType.block And mapMaze.MazeBlockFixed(pos.X, pos.Y) = False Then
                    mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.energizer
                End If

            Case Mode.addDot

                ' If addDot mode is active then check whether the tile under the cursor is fixed or a block
                ' If is is not then add a dot
                If mapMaze.MazePathType(pos.X, pos.Y) <> Maze.PathType.block And mapMaze.MazeBlockFixed(pos.X, pos.Y) = False Then
                    mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.dot
                End If

            Case Mode.deleteDot

                ' If deleteDot mode is active then check whether the tile under the cursor is fixed or not a dot
                ' If is is not then add a blank
                If mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.dot And mapMaze.MazeBlockFixed(pos.X, pos.Y) = False Then
                    mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.blank
                End If

            Case Mode.deleteEnergizer

                ' If deleteEnergizer mode is active then check whether the tile under the cursor is fixed or a not an energizer
                ' If is is then add a blank
                If mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.energizer And mapMaze.MazeBlockFixed(pos.X, pos.Y) = False Then
                    mapMaze.MazePathType(pos.X, pos.Y) = Maze.PathType.blank
                End If

        End Select

        ' Copy the changes made to the maze into the gameEngine
        CopyGameEngineMaze()

    End Sub

    Private Sub CopyGameEngineMaze()

        ' Convert maze data into path data
        mapMaze.PathToData()

        ' Copy each element within the mapMaze into the gameEngine
        For y = 0 To 30
            For x = 0 To 27
                MapEngine.MapByName("main").Value(New Point(x, y)) = mapMaze.Data(New Point(x, y))
            Next
        Next

    End Sub

    Public Sub Initialize()

        ClientSize = New Size((CLIENT_WIDTH * CLIENT_SCALE), CLIENT_HEIGHT * CLIENT_SCALE)
        BackColor = Color.Black

        ' Toolstrip defaults to rounded corners, so turn them off as it doesn't look great
        If TypeOf ToolStrip1.Renderer Is ToolStripProfessionalRenderer Then
            CType(ToolStrip1.Renderer, ToolStripProfessionalRenderer).RoundedEdges = False
        End If
        
        ' Initialize toolbar icons if not already done
        If Not initializedIcons Then
            LoadToolbarIcons()
            initializedIcons = True
        End If

        ' Creates a new instance of the game engine.
        MapEngine = New GameEngine(Me, CLIENT_WIDTH, CLIENT_HEIGHT - 40, CLIENT_SCALE, ToolStrip1.Height + MenuStrip1.Height)

        ' Adds a new tileset of size 8 x 8.
        MapEngine.AddTile("defaultMaze", New Size(8, 8))
        ' Intitalizes the map varialbe to use the defaul maze tileset and initializes the size.
        map = MapEngine.AddMap("main", MapEngine.TileIndexByName("defaultMaze"), New Size(32, 40))

        ' This tileset is used for debugging purposes.
        debugMap = MapEngine.AddMap("debug", MapEngine.TileIndexByName("defaultMaze"), New Size(6, 6))    ' DEBUG
        ' Intitalizes the positioin of the debugger.
        debugMap.Point = New Point(88, 94)

        ' Adds a new instance of the Pacman sprite to the map engine, and initializes required attributes.
        mapPacman = MapEngine.AddSprite("pacman", New Size(16, 16))
        mapPacman.AnimateMode = GameEngine.Sprite.GE_AnimationMode.geBoth
        mapPacman.AnimateOnFrame = 5
        mapPacman.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(3, 5)
        mapPacman.Enabled = True
        mapPacman.Point = New Point(13 * 8, (22 * 8) + 4)

        ' Adds a new sprite to the map engine or the map cursor, and initializes required attributes.
        mapCursor = MapEngine.AddSprite("newmapcur", New Size(16, 16))
        mapCursor.AnimateMode = GameEngine.Sprite.GE_AnimationMode.geNone
        mapCursor.AnimationRange = New GameEngine.Sprite.GE_AnimationRange(1, 1)
        mapCursor.Enabled = True

        ' Loads a blank maze.
        mapMaze.LoadMaze("")

        CopyGameEngineMaze()

        MapEngine.DrawMap()

        MapEngine.StartEngine()

    End Sub
    
    Private Sub LoadToolbarIcons()
        ' Set the image size for the ImageList
        iconImage.ImageSize = New Size(16, 16)
        
        ' Load icons from mapEditor.png file
        Try
            ' Load the mapEditor.png file which contains all toolbar icons
            Dim mapEditorImage As Image = Image.FromFile("Assets\mapEditor.png")
            
            ' The image is 96x48, which means it can contain 6x3 icons of size 16x16
            Dim iconSize As Integer = 16
            
            ' Extract all icons from the image
            For row = 0 To 2 ' 3 rows
                For col = 0 To 5 ' 6 columns
                    Dim iconIndex As Integer = row * 6 + col
                    Dim iconBitmap As New Bitmap(iconSize, iconSize)
                    
                    Using g As Graphics = Graphics.FromImage(iconBitmap)
                        ' Copy the icon from the mapEditorImage to the iconBitmap
                        g.DrawImage(mapEditorImage, New Rectangle(0, 0, iconSize, iconSize), _
                                   New Rectangle(col * iconSize, row * iconSize, iconSize, iconSize), _
                                   GraphicsUnit.Pixel)
                    End Using
                    
                    ' Add the extracted icon to the ImageList
                    iconImage.Images.Add(iconBitmap)
                Next
            Next
            
            ' Set initial button images
            ' Block tool uses icons 0, 1, 2
            ' Dot tool uses icons 3, 4, 5
            ' Energizer tool uses icons 6, 7, 8
            toolStripBlock.Image = iconImage.Images(1)
            toolStripDot.Image = iconImage.Images(3)
            toolStripEnergizer.Image = iconImage.Images(6)
        Catch ex As Exception
            MessageBox.Show("Failed to load toolbar icons: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub MapEditor_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        ' The form is closing so end the mapEngine
        MapEngine.EndEngine()

    End Sub

    Private Sub ToolStripBlock_Click(sender As Object, e As EventArgs) Handles toolStripBlock.Click

        If blockMode <> Mode.addBlock And blockMode <> Mode.deleteBlock Then
            toolStripBlock.Image = iconImage.Images(1)
            toolStripDot.Image = iconImage.Images(3)
            toolStripEnergizer.Image = iconImage.Images(6)
            toolStripBlock.ToolTipText = "Add Block"
            toolStripDot.ToolTipText = "Add Dot"
            toolStripEnergizer.ToolTipText = "Add Energizer"
            blockMode = Mode.addBlock
        Else
            If blockMode = Mode.addBlock Then
                toolStripBlock.Image = iconImage.Images(2)
                toolStripDot.Image = iconImage.Images(3)
                toolStripEnergizer.Image = iconImage.Images(6)
                toolStripBlock.ToolTipText = "Delete Block"
                blockMode = Mode.deleteBlock
            Else
                toolStripBlock.Image = iconImage.Images(1)
                toolStripDot.Image = iconImage.Images(3)
                toolStripEnergizer.Image = iconImage.Images(6)
                toolStripBlock.ToolTipText = "Add Block"
                blockMode = Mode.addBlock
            End If
        End If

        ToolStrip1.Refresh()

    End Sub

    Private Sub ToolStripDot_Click(sender As Object, e As EventArgs) Handles toolStripDot.Click

        If blockMode <> Mode.addDot And blockMode <> Mode.deleteBlock Then
            toolStripBlock.Image = iconImage.Images(0)
            toolStripDot.Image = iconImage.Images(4)
            toolStripEnergizer.Image = iconImage.Images(6)
            toolStripBlock.ToolTipText = "Add Block"
            toolStripDot.ToolTipText = "Add Dot"
            toolStripEnergizer.ToolTipText = "Add Energizer"
            blockMode = Mode.addDot
        Else
            If blockMode = Mode.addDot Then
                toolStripBlock.Image = iconImage.Images(0)
                toolStripDot.Image = iconImage.Images(5)
                toolStripEnergizer.Image = iconImage.Images(6)
                toolStripDot.ToolTipText = "Delete Dot"
                blockMode = Mode.deleteDot
            Else
                toolStripBlock.Image = iconImage.Images(0)
                toolStripDot.Image = iconImage.Images(4)
                toolStripEnergizer.Image = iconImage.Images(6)
                toolStripDot.ToolTipText = "Add Dot"
                blockMode = Mode.addDot
            End If
        End If

        ToolStrip1.Refresh()

    End Sub

    Private Sub ToolStripEnergizer_Click(sender As Object, e As EventArgs) Handles toolStripEnergizer.Click

        If blockMode <> Mode.addEnergizer And blockMode <> Mode.deleteEnergizer Then
            toolStripBlock.Image = iconImage.Images(0)
            toolStripDot.Image = iconImage.Images(3)
            toolStripEnergizer.Image = iconImage.Images(7)
            toolStripBlock.ToolTipText = "Add Block"
            toolStripDot.ToolTipText = "Add Dot"
            toolStripEnergizer.ToolTipText = "Add Energizer"
            blockMode = Mode.addEnergizer
        Else
            If blockMode = Mode.addEnergizer Then
                toolStripBlock.Image = iconImage.Images(0)
                toolStripDot.Image = iconImage.Images(3)
                toolStripEnergizer.Image = iconImage.Images(8)
                toolStripEnergizer.ToolTipText = "Delete Energizer"
                blockMode = Mode.deleteEnergizer
            Else
                toolStripBlock.Image = iconImage.Images(0)
                toolStripDot.Image = iconImage.Images(3)
                toolStripEnergizer.Image = iconImage.Images(7)
                toolStripEnergizer.ToolTipText = "Add Energizer"
                blockMode = Mode.addEnergizer
            End If
        End If

        ToolStrip1.Refresh()

    End Sub

    Private Function CheckDelete(curX As Integer, curY As Integer) As Boolean

        If mapMaze.MazeBlockFixed(curX, curY) = True Or
            mapMaze.MazeBlockFixed(curX + 1, curY) = True Or
            mapMaze.MazeBlockFixed(curX, curY + 1) = True Or
            mapMaze.MazeBlockFixed(curX + 1, curY + 1) = True Then
            Return False
        End If

        ' Iterate through all the maze elements
        For y = 0 To 29
            For x = 0 To 27

                ' Copy the maze to the temporary maze and simulate the deletion of the block
                ' at the current cursor position
                If ((x = curX And y = curY) Or (x = curX + 1 And y = curY) Or (x = curX And y = curY + 1) Or (x = curX + 1 And y = curY + 1)) Then
                    mapMazeDelete.MazePathType(x, y) = Maze.PathType.dot
                Else
                    mapMazeDelete.MazePathType(x, y) = mapMaze.MazePathType(x, y)
                End If

            Next
        Next y

        ' Iterate through all the maze elements and check for invalid blocks
        ' As soon as one if found exit the function with a false flag to indicate that
        ' removing the block results in an invalid maze state
        For y = 1 To 28
            For x = 1 To 26

                If mapMazeDelete.MazePathType(x, y) = Maze.PathType.block Then
                    If mapMazeDelete.MazePathType(x - 1, y) = Maze.PathType.block And mapMazeDelete.MazePathType(x, y - 1) = Maze.PathType.block And mapMazeDelete.MazePathType(x - 1, y - 1) = Maze.PathType.block Then
                    Else
                        If mapMazeDelete.MazePathType(x + 1, y) = Maze.PathType.block And mapMazeDelete.MazePathType(x, y - 1) = Maze.PathType.block And mapMazeDelete.MazePathType(x + 1, y - 1) = Maze.PathType.block Then
                        Else
                            If mapMazeDelete.MazePathType(x - 1, y) = Maze.PathType.block And mapMazeDelete.MazePathType(x, y + 1) = Maze.PathType.block And mapMazeDelete.MazePathType(x - 1, y + 1) = Maze.PathType.block Then
                            Else
                                If mapMazeDelete.MazePathType(x + 1, y) = Maze.PathType.block And mapMazeDelete.MazePathType(x, y + 1) = Maze.PathType.block And mapMazeDelete.MazePathType(x + 1, y + 1) = Maze.PathType.block Then
                                Else
                                    Return False
                                End If
                            End If
                        End If
                    End If
                End If

            Next x
        Next y

        ' All block are valid so exit the function and return true to indicate a valid maze state
        Return True

    End Function

    Private Sub ResetMazeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToolStrip_ResetMaze.Click

        Dim reply As DialogResult

        reply = MessageBox.Show("Are you sure you want to reset the maze?", "Reset Maze", MessageBoxButtons.YesNo)

        If reply = System.Windows.Forms.DialogResult.Yes Then

            mapMaze.LoadMaze("")
            CopyGameEngineMaze()

            toolStripBlock.Image = iconImage.Images(1)
            toolStripDot.Image = iconImage.Images(3)
            toolStripEnergizer.Image = iconImage.Images(6)
            toolStripBlock.ToolTipText = "Add Block"
            toolStripDot.ToolTipText = "Add Dot"
            toolStripEnergizer.ToolTipText = "Add Energizer"
            blockMode = Mode.addBlock

        End If

    End Sub

    Private Sub ToolStrip_LoadMaze_Click(sender As Object, e As EventArgs) Handles ToolStrip_LoadMaze.Click

        Dim fd As FileDialog

        fd = New OpenFileDialog With {
            .Title = "Load Pacman Maze",
            .Filter = "Pacman Mazes (*.pac)|*.pac",
            .FilterIndex = 1,
            .RestoreDirectory = True
        }

        If fd.ShowDialog() = DialogResult.OK Then

            mapMaze.LoadMaze(fd.FileName)
            CopyGameEngineMaze()

        End If

    End Sub

    Private Sub ToolStrip_SaveMaze_Click(sender As Object, e As EventArgs) Handles ToolStrip_SaveMaze.Click

        Dim fd As FileDialog

        fd = New SaveFileDialog With {
            .Title = "Save Pacman Maze",
            .Filter = "Pacman Mazes (*.pac)|*.pac",
            .FilterIndex = 1,
            .RestoreDirectory = True
        }

        If fd.ShowDialog() = DialogResult.OK Then

            mapMaze.SaveMaze(fd.FileName)

        End If

    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click

        Close()

    End Sub

    Private Sub ToolStrip_Debug_Click(sender As Object, e As EventArgs) Handles ToolStrip_Debug.Click

        If ToolStrip_Debug.Checked = True Then
            ToolStrip_Debug.Checked = False
            debugMap.Enabled = False
            CopyGameEngineMaze()
        Else
            ToolStrip_Debug.Checked = True
            debugMap.Enabled = True
        End If

    End Sub

    Private Sub ToolStrip_EditorInstructions_Click(sender As Object, e As EventArgs) Handles ToolStrip_EditorInstructions.Click

        MapEngine.EndEngine()
        Program.mazeEditorInstr.ShowDialog()
        MapEngine.StartEngine()

    End Sub
End Class