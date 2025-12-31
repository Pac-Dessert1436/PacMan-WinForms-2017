Public Class GameEngine

    Declare Function QueryPerformanceCounter Lib "Kernel32" (ByRef X As Long) As Short
    Declare Function QueryPerformanceFrequency Lib "Kernel32" (ByRef X As Long) As Short

    Private Const MARGIN_X = 0
    Private Const MARGIN_Y = 0

    Private ReadOnly _bw As New ComponentModel.BackgroundWorker

    Dim _cTimer As Long
    Dim _cTimer2 As Long
    Dim _freq As Long
    Dim _interval As Double
    Dim _fpsActual As Double

    ' This structure holds the information relating to the surface we're targetting

    Private Structure GE_Surface
        Public _client As PictureBox
        Public _mouseEventArgs As MouseEventArgs
        Public _image As Image
        Public _scale As Integer
        Public _fps As Double
        Public _clock As Integer
        Public _surfaceLocked As Boolean
    End Structure

    ' -----------------------------------------------------------------------------------------------------------------------------

    Private _geSurface As New GE_Surface
    Private ReadOnly _geTile As New List(Of Tile)
    Private ReadOnly _geSprite As New List(Of Sprite)
    Private ReadOnly _geMap As New List(Of Map)
    Private ReadOnly _geFont As New List(Of Font)

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' geSurface
    ' -----------------------------------------------------------------------------------------------------------------------------

    ' Create a new surface

    Public Sub New(client As Form, x As Integer, y As Integer, Optional scale As Integer = 1, Optional offset As Integer = 24)

        ' Create a new picturebox to hold the gameEngine surface

        _geSurface._client = New PictureBox
        With _geSurface._client
            .Size = New Size(x * scale, y * scale)
            .Location = New Point(0, offset)
        End With
        AddHandler _geSurface._client.MouseMove, AddressOf MouseMove
        AddHandler _geSurface._client.MouseDown, AddressOf MouseDown
        AddHandler _geSurface._client.MouseUp, AddressOf MouseUp

        ' Add the picturebox to the client (form)

        client.Controls.Add(_geSurface._client)

        ' Create a surface into which the gameEngine will render

        _geSurface._image = New Bitmap((x + (MARGIN_X * 2)) * scale, (y + (MARGIN_Y * 2)) * scale)

        ' Set the scaling
        ' Game engine pre-scales everything in advance rather than at render time
        ' This increases performance drastically

        _geSurface._scale = scale

        ' Set the default frames per second

        _geSurface._fps = 60

        ' Set up the background worker
        ' The background worker supports cancellation and reports progress (we do the rendering in the progress event)

        _bw.WorkerSupportsCancellation = True
        _bw.WorkerReportsProgress = True
        AddHandler _bw.DoWork, AddressOf RunEngine
        AddHandler _bw.ProgressChanged, AddressOf RunEngineProcess
        AddHandler _bw.RunWorkerCompleted, AddressOf RunEngineComplete

    End Sub

    Public Sub StartEngine()

        ' Wait until the background worker is ended

        While _bw.IsBusy = True
            Application.DoEvents()
        End While

        ' Start the background worker

        _bw.RunWorkerAsync()

    End Sub

    Public Sub EndEngine()

        ' Cancel the background worker

        _bw.CancelAsync()

    End Sub

    Private Sub RunEngineProcess(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs)

        _geSurface._surfaceLocked = True
        RaiseEvent GE_GameLogic()
        DrawMap()
        DrawSprites()
        RaiseEvent GE_RenderScene()

        _geSurface._client.Image = _geSurface._image
        _geSurface._clock += 1
        _geSurface._surfaceLocked = False

    End Sub

    Private Sub RunEngine(sender As Object, e As System.ComponentModel.DoWorkEventArgs)
        ' Get the performance frequency

        QueryPerformanceFrequency(_freq)

        ' Calculate the clock interval at the given frames per second

        _interval = _freq / _geSurface._fps

        Do

            ' If the background worker has been cancelled (by gameEngine.endEngine) then cancel and exit

            If _bw.CancellationPending = True Then
                e.Cancel = True
                Exit Do
            End If

            ' Get current time
            QueryPerformanceCounter(_cTimer2)

            ' Compare the current time with the previous time to see whether enough time has elapsed

            If _cTimer2 >= _cTimer + _interval Then

                ' Get the actual frames per second

                _fpsActual = Math.Round(_freq / (_cTimer2 - _cTimer), 1)

                ' Get the current time

                QueryPerformanceCounter(_cTimer)

                ' Ensure that the previous ReportProgress has completed
                ' We do this be checking that the surfaceLocked state

                '_bw.ReportProgress(1, _geSurface._image)

                If _geSurface._surfaceLocked = False Then
                    _bw.ReportProgress(1, _geSurface._image)
                End If

            End If

        Loop

    End Sub

    Private Sub RunEngineComplete(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs)

    End Sub

    Private Sub MouseMove(sender As Object, e As MouseEventArgs)

        RaiseEvent GE_MouseMove(sender, e)

    End Sub

    Private Sub MouseDown(sender As Object, e As MouseEventArgs)

        RaiseEvent GE_MouseDown(sender, e)

    End Sub

    Private Sub MouseUp(sender As Object, e As MouseEventArgs)

        RaiseEvent GE_MouseUp(sender, e)

    End Sub



    Public Event GE_GameLogic()
    Public Event GE_RenderScene()

    Public Event GE_MouseMove(sender As Object, e As MouseEventArgs)
    Public Event GE_MouseDown(sender As Object, e As MouseEventArgs)
    Public Event GE_MouseUp(sender As Object, e As MouseEventArgs)

    Public Property Scale
        Get
            Scale = _geSurface._scale
        End Get
        Set(value)
            _geSurface._scale = Scale
        End Set
    End Property

    Public Property Fps As Double
        Get
            Fps = _fpsActual
        End Get
        Set(value As Double)
            _geSurface._fps = value
        End Set
    End Property

    Public ReadOnly Property Running As Boolean
        Get
            If _bw.IsBusy Then
                Running = True
            Else
                Running = False
            End If
        End Get
    End Property

    Public Property Clock As Integer
        Get
            Clock = _geSurface._clock '/ _geSurface._fps
        End Get
        Set(value As Integer)
            _geSurface._clock = value '* _geSurface._fps
        End Set
    End Property

    Public ReadOnly Property GetMouse As MouseEventArgs
        Get
            GetMouse = _geSurface._mouseEventArgs
        End Get
    End Property

    Public ReadOnly Property GetSurface As PictureBox
        Get
            GetSurface = _geSurface._client
        End Get
    End Property

    Public ReadOnly Property GetImage
        Get
            GetImage = _geSurface._image
        End Get
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.addSprite(as gameEngine.geSprite)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Function AddSprite(filename As String, spriteSize As Size) As Sprite
        _geSprite.Add(New Sprite(Me, filename, spriteSize))
        Return _geSprite(_geSprite.Count - 1)
    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.spriteByIndex(as integer)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Property SpriteByIndex(index As Integer) As Sprite
        Get
            Return _geSprite(index)
        End Get
        Set(value As Sprite)
            _geSprite(index) = value
        End Set
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.spriteByName(as string)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Property SpriteByName(name As String) As Sprite
        Get
            Dim index As Integer
            index = _geSprite.FindIndex((Function(f) f.Name = name))
            Return _geSprite(index)
        End Get
        Set(value As Sprite)
            Dim index As Integer
            index = _geSprite.FindIndex((Function(f) f.Name = name))
            _geSprite(index) = value
        End Set
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.spriteIndexByName(as string) as integer
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public ReadOnly Property SpriteIndexByName(name As String) As Integer
        Get
            Dim index As Integer
            index = _geSprite.FindIndex((Function(f) f.Name = name))
            Return index
        End Get
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.addTile(as gameEngine.geTile)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function AddTile(filename As String, tileSize As Size) As Tile

        _geTile.Add(New Tile(Me, filename, tileSize))
        Return _geTile(_geTile.Count - 1)

    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.tileByIndex(as integer) as tile
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Property TileByIndex(index As Integer) As Tile
        Get
            Return _geTile(index)
        End Get
        Set(value As Tile)
            _geTile(index) = value
        End Set
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.tileByName(as string) as tile
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Property TileByName(name As String) As Tile
        Get
            Dim index As Integer
            index = _geTile.FindIndex((Function(f) f.Name = name))
            Return _geTile(index)
        End Get
        Set(value As Tile)
            Dim index As Integer
            index = _geTile.FindIndex((Function(f) f.Name = name))
            _geTile(index) = value
        End Set
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.tileIndexByName(as string) as integer
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public ReadOnly Property TileIndexByName(name As String) As Integer
        Get
            Dim index As Integer
            index = _geTile.FindIndex((Function(f) f.Name = name))
            Return index
        End Get
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.addMap(as gameEngine.geMap)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function AddMap(name As String, tilesetIndex As Integer, mapSize As Size) As Map
        _geMap.Add(New Map(Me, name, tilesetIndex, mapSize))
        Return _geMap(_geMap.Count - 1)
    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.mapByIndex(as integer)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Property MapByIndex(index As Integer) As Map
        Get
            Return _geMap(index)
        End Get
        Set(value As Map)
            _geMap(index) = value
        End Set
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.mapByName(as string) as map
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Property MapByName(name As String) As Map
        Get
            Dim index As Integer
            index = _geMap.FindIndex((Function(f) f.Name = name))
            Return _geMap(index)
        End Get
        Set(value As Map)
            Dim index As Integer
            index = _geMap.FindIndex((Function(f) f.Name = name))
            _geMap(index) = value
        End Set
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.mapIndexByName(as string) as integer
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public ReadOnly Property MapIndexByName(name As String) As Integer
        Get
            Dim index As Integer
            index = _geMap.FindIndex((Function(f) f.Name = name))
            Return index
        End Get
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' gameEngine.addFont(as gameEngine.geFont)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function AddFont(filename As String, tileSize As Size, asciiStart As Integer) As Font
        _geFont.Add(New Font(Me, filename, tileSize, asciiStart))
        Return _geFont(_geFont.Count - 1)
    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' drawSprites()
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub DrawSprites()

        _geSprite.Sort(Function(x, y) x.ZIndex.CompareTo(y.ZIndex))

        For n = 0 To _geSprite.Count - 1
            SpriteByIndex(n).DrawSprite()
        Next

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' drawTileByName(name as string, t as integer, p as point)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub DrawTileByName(name As String, t As Integer, p As Point)

        TileByName(name).DrawTile(t, p)

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' drawTileByIndex(index as integer, t as integer, p as point)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub DrawTileByIndex(index As Integer, t As Integer, p As Point)

        TileByIndex(index).DrawTile(t, p)

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' drawMap()
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub DrawMap()

        For m = 0 To _geMap.Count - 1

            If _geMap(m).Enabled = True Then

                For y1 = _geMap(m).MapClip.Y To _geMap(m).MapClip.Bottom - 1
                    For x1 = _geMap(m).MapClip.X To _geMap(m).MapClip.Right - 1

                        Dim tileSize As Size
                        tileSize = _geTile(_geMap(m).TilesetIndex).Size

                        If _geMap(m).Invalidated(New Point(x1, y1)) = True Then
                            If _geMap(m).Value(New Point(x1, y1)) <> -1 Then
                                DrawTileByIndex(_geMap(m).TilesetIndex, _geMap(m).Value(New Point(x1, y1)), New Point(_geMap(m).Point.X + ((x1 - _geMap(m).MapClip.X) * tileSize.Width), _geMap(m).Point.Y + ((y1 - _geMap(m).MapClip.Y) * tileSize.Height)))
                            End If
                            _geMap(m).Invalidated(New Point(x1, y1)) = False
                        End If

                    Next
                Next

            End If

        Next

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' drawTextByName(name as string, text as string, p as point)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub DrawTextbyName(name As String, text As String, p As Point)

        Dim index As Integer
        index = _geFont.FindIndex((Function(f) f.Name = name))

        For s = 1 To Len(text)

            _geFont(index).DrawTile(Asc(Mid(text, s, 1)), New Point(p.X + ((s - 1) * _geFont(index).Size.Width), p.Y))

        Next

        ' Invalidate tiles that are overlapped by the sprites

        For n = 0 To _geMap.Count - 1

            Dim tileSize As Size
            tileSize = _geTile(_geMap(n).TilesetIndex).Size

            Dim tileOffset As Point
            tileOffset = _geMap(n).Point

            Dim tilePos As Point
            tilePos = New Point(Int((p.X - tileOffset.X) / tileSize.Width), Int((p.Y - tileOffset.Y) / tileSize.Height))

            For yy = tilePos.Y To tilePos.Y + (_geFont(index).Size.Height / tileSize.Height) + 1
                For xx = tilePos.X To tilePos.X + ((_geFont(index).Size.Width * Len(text)) / tileSize.Width) + 1
                    _geMap(n).Invalidated(New Point(xx, yy)) = True
                Next
            Next
        Next

    End Sub

    ' =============================================================================================================================
    '
    ' geTile Class
    '
    ' =============================================================================================================================

    Public Class Tile

        Private ReadOnly _parent As GameEngine               ' Parent class
        Private _name As String                     ' Name of tileset
        Private ReadOnly _geTileset As List(Of Image)        ' List of tile images
        Private ReadOnly _count As Integer                   ' Number of tiles in the tileset
        Private _size As Size                       ' Tile size

        ' New subroutine
        ' This handles the creation of the geTile item

        Public Sub New(parent As GameEngine, filename As String, tileSize As Size)

            _parent = parent
            _name = filename
            _geTileset = New List(Of Image)
            _count = 0
            _size = tileSize

            ' Define a new picturebox to hold the unformatted tileset

            Dim pbTileSet As New PictureBox

            ' Allocate an empty bitmap the same size as an individual tile

            Dim bm As New Bitmap(_size.Width * _parent._geSurface._scale, _size.Height * _parent._geSurface._scale, Imaging.PixelFormat.Format32bppPArgb)

            ' Define two variables to hold the number of tiles in the X and Y direction

            Dim numberTilesX As Integer
            Dim numberTilesY As Integer

            ' Load image from file

            pbTileSet.Image = Image.FromFile("Assets\" & filename & ".png")

            ' Get number of tiles in X and Y direction

            numberTilesX = pbTileSet.Image.Width / _size.Width
            numberTilesY = pbTileSet.Image.Height / _size.Height

            ' Loop through each row and column per individual tile

            For y1 = 0 To numberTilesY - 1
                For x1 = 0 To numberTilesX - 1

                    ' Allocate a graphics surface to the empty bitmap

                    Using gr As Graphics = Graphics.FromImage(bm)

                        ' Set the source rectangle size

                        Dim srcRect As New Rectangle(x1 * _size.Width, y1 * _size.Height, _size.Width, _size.Height)

                        ' Set the destination rectangle size

                        Dim dstRect As New Rectangle(0, 0, _size.Width * _parent._geSurface._scale, _size.Height * _parent._geSurface._scale)

                        ' Copy from the source rectangle co-ordinates in the tileset bitmap
                        ' into the destination rectangle co-ordinates in the empty bitmap

                        gr.CompositingQuality = Drawing2D.CompositingQuality.HighSpeed
                        gr.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
                        gr.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                        gr.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                        gr.CompositingMode = Drawing2D.CompositingMode.SourceCopy
                        gr.DrawImage(pbTileSet.Image, dstRect, srcRect, GraphicsUnit.Pixel)

                    End Using

                    _geTileset.Add(bm.Clone)
                    _count += 1

                Next
            Next

        End Sub

        ' geTile.drawTile

        Public Sub DrawTile(t As Integer, p As Point)

            If t < _count Then
                Using gr As Graphics = Graphics.FromImage(_parent._geSurface._image)
                    gr.DrawImage(_geTileset(t), p.X * _parent._geSurface._scale, p.Y * _parent._geSurface._scale)
                End Using
            End If

        End Sub

        ' geTile.name

        Public Property Name As String
            Get
                Name = _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        ' geTile.count

        Public ReadOnly Property Count As Integer
            Get
                Count = _count
            End Get
        End Property

        ' geTile.size

        Public ReadOnly Property Size As Size
            Get
                Size = _size
            End Get
        End Property

    End Class

    ' =============================================================================================================================
    '
    ' geMap Class
    '
    ' =============================================================================================================================

    Public Class Map

        Private ReadOnly _parent As GameEngine
        Private _name As String
        Private _tilesetIndex As Integer
        Private _geMap(,) As Integer
        Private ReadOnly _gemapInvalidated(,) As Boolean
        Private _geMapSize As Size
        Private _geMapClip As Rectangle
        Private _point As Point
        Private _enabled As Boolean

        Public Sub New(parent As GameEngine, name As String, tilesetIndex As Integer, mapSize As Size)

            _parent = parent
            _name = name
            _tilesetIndex = tilesetIndex
            _geMapSize = mapSize
            _geMapClip = New Rectangle(0, 0, _geMapSize.Width, _geMapSize.Height)
            _point = New Point(0, 0)
            _enabled = True

            ReDim _geMap(_geMapSize.Width, _geMapSize.Height)
            ReDim _gemapInvalidated(_geMapSize.Width, _geMapSize.Height)
            For y1 = 0 To _geMapSize.Height
                For x1 = 0 To _geMapSize.Width
                    _geMap(x1, y1) = -1
                    _gemapInvalidated(x1, y1) = True
                Next
            Next

        End Sub

        Property Name As String
            Get
                Name = _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        Property TilesetIndex As Integer
            Get
                TilesetIndex = _tilesetIndex
            End Get
            Set(value As Integer)
                _tilesetIndex = value
            End Set
        End Property

        Property MapSize As Size
            Get
                MapSize = _geMapSize
            End Get
            Set(value As Size)
                _geMapSize = value
                ReDim Preserve _geMap(_geMapSize.Width - 1, _geMapSize.Height - 1)
            End Set
        End Property

        Property MapClip As Rectangle
            Get
                MapClip = _geMapClip
            End Get
            Set(value As Rectangle)
                If value.X < 0 Then value.X = 0
                If value.X > _geMapSize.Width Then value.X = _geMapSize.Width
                If value.Y < 0 Then value.Y = 0
                If value.Y > _geMapSize.Height Then value.Y = _geMapSize.Height
                _geMapClip = value
            End Set
        End Property

        Public Property Point As Point
            Get
                Point = _point
            End Get
            Set(value As Point)
                _point = value
            End Set
        End Property

        Public Property Enabled As Boolean
            Get
                Enabled = _enabled
            End Get
            Set(value As Boolean)
                _enabled = value
            End Set
        End Property

        Public Property Value(p As Point) As Integer
            Get
                If p.X < 0 Or p.X >= _geMapSize.Width Or p.Y < 0 Or p.Y >= _geMapSize.Height Then
                    Value = -1
                Else
                    Value = _geMap(p.X, p.Y)
                End If
            End Get
            Set(value As Integer)
                If p.X >= 0 And p.X < _geMapSize.Width And p.Y >= 0 And p.Y < _geMapSize.Height Then
                    _geMap(p.X, p.Y) = value
                    _gemapInvalidated(p.X, p.Y) = True
                End If
            End Set
        End Property

        Public Property Invalidated(p As Point) As Boolean
            Get
                If p.X < 0 Or p.X >= _geMapSize.Width Or p.Y < 0 Or p.Y >= _geMapSize.Height Then
                    Invalidated = False
                Else
                    Invalidated = _gemapInvalidated(p.X, p.Y)
                End If
            End Get
            Set(value As Boolean)
                If p.X >= 0 And p.X < _geMapSize.Width And p.Y >= 0 And p.Y < _geMapSize.Height Then
                    _gemapInvalidated(p.X, p.Y) = value
                End If
            End Set
        End Property

    End Class

    ' =============================================================================================================================
    '
    ' geSprite Class
    '
    ' =============================================================================================================================

    Public Class Sprite

        Public Enum GE_AnimationMode As Integer
            geNone = 0
            geForward = 1
            geBackwards = 2
            geBoth = 3
        End Enum

        Public Class GE_AnimationRange
            Private _min As Integer
            Private _max As Integer
            Sub New(min As Integer, max As Integer)
                _min = min
                _max = max
            End Sub
            Public Property Min As Integer
                Get
                    Min = _min
                End Get
                Set(value As Integer)
                    _min = value
                End Set
            End Property
            Public Property Max As Integer
                Get
                    Max = _max
                End Get
                Set(value As Integer)
                    _max = value
                End Set
            End Property
        End Class

        Private ReadOnly _parent As GameEngine               ' Parent Class
        Private _name As String                     ' Name of sprite
        Private ReadOnly _geTileset As List(Of Image)        ' List of sprite images
        Private _point As Point                     ' X,Y position
        Private _size As Size                       ' size in pixels
        Private _enabled As Boolean                 ' Enabled/disabled
        Private ReadOnly _totalframes As Integer             ' Total number of frames available
        Private _animateMode As GE_AnimationMode     ' Animation mode
        Private _animateRange As GE_AnimationRange   ' Animation minimum and maximum frames
        Private _frameCount As Integer              ' Global frame counter
        Private _animateOnFrame As Integer          ' Increment animation frame when the global frame counter hits this value
        Private _animateFrame As Integer            ' Current animation frame
        Private _animateDirection As Integer        ' Current animation direction
        Private _zindex As Integer                  ' Z-Order when rendering

        ' New subroutine
        ' This handles the creation of the geSprite item

        Public Sub New(parent As GameEngine, filename As String, spriteSize As Size)

            _parent = parent
            _name = filename
            _geTileset = New List(Of Image)
            _point = New Point(0, 0)
            _size = spriteSize
            _enabled = False
            _totalframes = 0
            _animateMode = GE_AnimationMode.geNone
            _animateRange = New GE_AnimationRange(0, 0)
            _frameCount = 0
            _animateOnFrame = 0
            _animateFrame = 0
            _animateDirection = 1
            _zindex = 1

            ' We need to load the tileset from the file specified

            ' Define a new picturebox to hold the unformatted tileset

            Dim pbTileSet As New PictureBox

            ' Allocation an empty bitmap the same size as an individual tile

            Dim bm As New Bitmap(_size.Width * _parent._geSurface._scale, _size.Height * _parent._geSurface._scale, Imaging.PixelFormat.Format32bppPArgb)

            ' Define two variables to hold the number of tiles in the X and Y direction

            Dim numberTilesX As Integer
            Dim numberTilesY As Integer

            ' Load image from file

            pbTileSet.Image = Image.FromFile("Assets\" & filename & ".png")

            ' Get number of tiles in X and Y direction

            numberTilesX = pbTileSet.Image.Width / _size.Width
            numberTilesY = pbTileSet.Image.Height / _size.Height

            ' Loop through each row and column per individual tile

            Dim x As Integer
            Dim y As Integer

            For y = 0 To numberTilesY - 1
                For x = 0 To numberTilesX - 1

                    ' Allocate a graphics surface to the empty bitmap

                    Using gr As Graphics = Graphics.FromImage(bm)

                        ' Set the source rectangle size

                        Dim srcRect As New Rectangle(x * _size.Width, y * _size.Height, _size.Width, _size.Height)

                        ' Set the destination rectangle size

                        Dim dstRect As New Rectangle(0, 0, _size.Width * _parent._geSurface._scale, _size.Height * _parent._geSurface._scale)

                        ' Copy from the source rectangle co-ordinates in the tileset bitmap
                        ' into the destination rectangle co-ordinates in the empty bitmap

                        gr.CompositingQuality = Drawing2D.CompositingQuality.HighSpeed
                        gr.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
                        gr.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                        gr.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                        gr.CompositingMode = Drawing2D.CompositingMode.SourceCopy
                        gr.DrawImage(pbTileSet.Image, dstRect, srcRect, GraphicsUnit.Pixel)

                    End Using

                    ' Add the current tile into our list of tiles and increment the frame count

                    _geTileset.Add(bm.Clone)
                    _totalframes += 1

                Next
            Next
        End Sub

        ' DrawSprite subroutine
        ' This handle the drawing of the sprite instance

        Public Sub DrawSprite()

            If _enabled = True Then
                Using gr As Graphics = Graphics.FromImage(_parent._geSurface._image)
                    gr.DrawImage(_geTileset(_animateFrame), _point.X * _parent._geSurface._scale, _point.Y * _parent._geSurface._scale)
                End Using

                If _animateMode <> GE_AnimationMode.geNone Then
                    If _frameCount >= _animateOnFrame Then
                        _animateFrame += _animateDirection
                        If _animateFrame > _animateRange.Max Then
                            If _animateMode = GE_AnimationMode.geBoth Then
                                _animateDirection *= -1
                                _animateFrame = _animateRange.Max + _animateDirection
                            Else
                                _animateFrame = _animateRange.Min
                            End If
                        End If
                        If _animateFrame < _animateRange.Min Then
                            If _animateMode = GE_AnimationMode.geBoth Then
                                _animateDirection *= -1
                                _animateFrame = _animateRange.Min + _animateDirection
                            Else
                                _animateFrame = _animateRange.Max
                            End If
                        End If
                        _frameCount = 0
                    Else
                        _frameCount += 1
                    End If
                End If

                ' Invalidate tiles that are overlapped by the sprites

                For n = 0 To _parent._geMap.Count - 1

                    Dim tileSize As Size
                    tileSize = _parent._geTile(_parent._geMap(n).TilesetIndex).Size

                    Dim tileOffset As Point
                    tileOffset = _parent._geMap(n).Point

                    Dim tilePos As Point
                    tilePos = New Point(Int((Point.X - tileOffset.X) / tileSize.Width), Int((Point.Y - tileOffset.Y) / tileSize.Height))

                    For yy = tilePos.Y To tilePos.Y + (_size.Height / tileSize.Height) + 1
                        For xx = tilePos.X To tilePos.X + (_size.Width / tileSize.Width) + 1
                            _parent._geMap(n).Invalidated(New Point(xx, yy)) = True
                        Next
                    Next
                Next

            End If

        End Sub

        ' geSprite.name

        Public Property Name As String
            Get
                Name = _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        ' geSprite.point 

        Public Property Point As Point
            Get
                Point = _point
            End Get
            Set(value As Point)
                _point = value
            End Set
        End Property

        ' geSprite.enabled

        Public Property Enabled As Boolean
            Get
                Enabled = _enabled
            End Get
            Set(value As Boolean)
                _enabled = value
            End Set
        End Property

        ' geSprite.totalFrames

        Public ReadOnly Property TotalFrames As Integer
            Get
                TotalFrames = _totalframes
            End Get
        End Property

        ' geSprite.animationMode

        Public Property AnimateMode As GE_AnimationMode
            Get
                AnimateMode = _animateMode
            End Get
            Set(value As GE_AnimationMode)
                _animateMode = value
                If _animateMode = GE_AnimationMode.geForward Or _animateMode = GE_AnimationMode.geBoth Then
                    _animateDirection = 1
                Else
                    If _animateMode = GE_AnimationMode.geBackwards Then
                        _animateDirection = -1
                    End If
                End If

            End Set
        End Property

        ' geSprite.animationRange

        Public Property AnimationRange As GE_AnimationRange
            Get
                AnimationRange = _animateRange
            End Get
            Set(value As GE_AnimationRange)
                _animateRange = value
                _animateFrame = value.Min
                _frameCount = 0
            End Set
        End Property

        ' geSprite.animateOnFrame

        Public Property AnimateOnFrame As Integer
            Get
                AnimateOnFrame = _animateOnFrame
            End Get
            Set(value As Integer)
                _animateOnFrame = value
                _frameCount = 0
            End Set
        End Property

        ' geSprite.zindex

        Public Property ZIndex As Integer
            Get
                ZIndex = _zindex
            End Get
            Set(value As Integer)
                _zindex = value
            End Set
        End Property

    End Class


    ' =============================================================================================================================
    '
    ' geFont Class
    '
    ' =============================================================================================================================

    Public Class Font

        Private ReadOnly _parent As GameEngine               ' Parent class
        Private _name As String                     ' Name of tileset
        Private ReadOnly _geTileset As List(Of Image)        ' List of tile images
        Private ReadOnly _count As Integer                   ' Number of tiles in the tileset
        Private _asciiStart As Integer              ' ASCII start position
        Private _tileSize As Size                   ' Tile Size

        ' New subroutine
        ' This handles the creation of the geFont item

        Public Sub New(parent As GameEngine, filename As String, tileSize As Size, asciiStart As Integer)

            _parent = parent
            _name = filename
            _geTileset = New List(Of Image)
            _count = 0
            _asciiStart = asciiStart
            _tileSize = tileSize

            ' Define a new picturebox to hold the unformatted tileset

            Dim pbTileSet As New PictureBox

            ' Allocation an empty bitmap the same size as an individual tile

            Dim bm As New Bitmap(_tileSize.Width * _parent._geSurface._scale, _tileSize.Height * _parent._geSurface._scale, Imaging.PixelFormat.Format32bppPArgb)

            ' Define two variables to hold the number of tiles in the X and Y direction

            Dim numberTilesX As Integer
            Dim numberTilesY As Integer

            ' Load image from file

            pbTileSet.Image = Image.FromFile("Assets\" & filename & ".png")

            ' Get number of tiles in X and Y direction

            numberTilesX = pbTileSet.Image.Width / tileSize.Width
            numberTilesY = pbTileSet.Image.Height / tileSize.Height

            ' Loop through each row and column per individual tile

            For y1 = 0 To numberTilesY - 1
                For x1 = 0 To numberTilesX - 1

                    ' Allocate a graphics surface to the empty bitmap

                    Using gr As Graphics = Graphics.FromImage(bm)

                        ' Set the source rectangle size

                        Dim srcRect As New Rectangle(x1 * tileSize.Width, y1 * tileSize.Height, tileSize.Width, tileSize.Height)

                        ' Set the destination rectangle size

                        Dim dstRect As New Rectangle(0, 0, tileSize.Width * _parent._geSurface._scale, tileSize.Height * _parent._geSurface._scale)

                        ' Copy from the source rectangle co-ordinates in the tileset bitmap
                        ' into the destination rectangle co-ordinates in the empty bitmap

                        gr.CompositingQuality = Drawing2D.CompositingQuality.HighSpeed
                        gr.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
                        gr.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                        gr.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                        gr.CompositingMode = Drawing2D.CompositingMode.SourceCopy
                        gr.DrawImage(pbTileSet.Image, dstRect, srcRect, GraphicsUnit.Pixel)

                    End Using

                    _geTileset.Add(bm.Clone)
                    _count += 1

                Next
            Next

        End Sub

        Public Sub DrawTile(t As Integer, p As Point)

            If t - _asciiStart < _count And t - AsciiStart >= 0 Then
                Using gr As Graphics = Graphics.FromImage(_parent._geSurface._image)
                    gr.DrawImage(_geTileset(t - _asciiStart), p.X * _parent._geSurface._scale, p.Y * _parent._geSurface._scale)
                End Using
            End If

        End Sub

        ' geFont.name

        Public Property Name As String
            Get
                Name = _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        ' geFont.count

        Public ReadOnly Property Count As Integer
            Get
                Count = _count
            End Get
        End Property

        ' geFount.asciiStart

        Public Property AsciiStart As Integer
            Get
                AsciiStart = _asciiStart
            End Get
            Set(value As Integer)
                If AsciiStart > 0 Then
                    If AsciiStart > (128 - _count) Then
                        _asciiStart = 1
                    End If
                    _asciiStart = value
                Else
                    _asciiStart = 1
                End If
            End Set
        End Property

        ' geFont.getSize

        Public ReadOnly Property Size As Size
            Get
                Size = _tileSize
            End Get
        End Property

    End Class

End Class