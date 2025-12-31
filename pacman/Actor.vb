
' Actor Class
' This handles Pac-Man, the Ghosts and the Fruit

Public Class Actor

    ' Enumerate actor directions
    Public Enum ActorDirection As Integer
        None = 0
        Up = 1
        Down = 2
        Left = 3
        Right = 4
    End Enum

    ' Enumerate ghost modes
    Public Enum GhostMode As Integer
        GhostOutside = 0
        GhostEaten = 1
        GhostGoingHome = 2
        GhostEnteringHome = 3
        GhostPacingHome = 4
        GhostLeavingHome = 5
        GhostStart = 6
    End Enum

    ' Enumerate ghost state
    Public Enum GhostState As Integer
        Scatter = 0
        Chase = 1
    End Enum

    ' Enumerate release modes
    Public Enum ReleaserMode
        ModePersonal = 0
        ModeGlobal = 1
    End Enum

    ' Actors are stored internally as lists
    ' Initialize the lists for ghosts, Pac-Man and the fruit
    Private Shared ReadOnly _ghost As New List(Of Ghost)
    Private Shared ReadOnly _pacman As New List(Of PacMan)
    Private Shared ReadOnly _fruit As New List(Of Fruit)

    ' Initialize the ghost state and default it to "Chase"
    Private Shared _ghostState As GhostState = GhostState.Chase

    ' Create a new ghost releaser
    Public ghostReleaser As New Releaser

    ' The step size structure holds information related to the speed of the actors
    ' when they are in different states
    Private Structure StepSizeData
        Public pacmanNormal As String
        Public ghostsNormal As String
        Public pacmanFright As String
        Public ghostsFright As String
        Public ghostsTunnel As String
        Public ghostsPacing As String
        Public elroy1 As String
        Public elroy2 As String
    End Structure

    ' Step size and step counter
    Private Shared _stepSize As StepSizeData
    Private Shared _stepCounter As Integer

    ' Internal level counter
    Private Shared _level As Integer

    ' Energize and energize flash time in frames. One element per level
    Private Shared ReadOnly _energizeTime() = {6, 5, 4, 3, 2, 5, 2, 2, 1, 5, 2, 1, 1, 3, 1, 1, 0, 1}
    Private Shared ReadOnly _energizeFlashTime() = {5, 5, 5, 5, 5, 5, 5, 5, 3, 5, 5, 3, 3, 5, 3, 3, 0, 3}

    ' Current energized timer, energized flash timer, and energized score
    Private _energizedTimer As Integer
    Private _energizedFlashTimer As Integer
    Private _energizedScore As Integer

    ' Nothing to initilize at the actor class level
    Public Sub New()

    End Sub

    ' =============================================================================================================================
    '
    ' fruit Class
    '
    ' =============================================================================================================================

    Public Class Fruit

        Private _name As String
        Private _pixel As Point
        Private _number As Integer
        Private _active As Integer
        Private _tick As Integer
        Private _points As Integer
        Private _eaten As Boolean
        Private _eatenTick As Integer
        Private _list As String

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.name(as string)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Name As String
            Get
                Name = _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.pixel(as string)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Pixel As Point
            Get
                Pixel = _pixel
            End Get
            Set(value As Point)
                _pixel = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.tile() as Point
        ' -----------------------------------------------------------------------------------------------------------------------------

        ReadOnly Property Tile As Point
            Get

                Tile.X = Int(_pixel.X / 8)
                Tile.Y = Int(_pixel.Y / 8)

            End Get
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.number(n as number) 
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Number As Integer
            Get
                Number = _number
            End Get
            Set(value As Integer)
                If value < 8 Then
                    _number = value
                End If
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.active() as boolean
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Active As Boolean
            Get
                Active = _active
            End Get
            Set(value As Boolean)
                _active = value
                If _active = False Then
                    _tick = 0
                End If
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.tick() as integer
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Tick As Integer
            Get
                Tick = _tick
            End Get
            Set(value As Integer)
                _tick = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.points() as integer
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Points As Integer
            Get
                Points = _points
            End Get
            Set(value As Integer)
                _points = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.eaten() as boolean
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Eaten As Boolean
            Get
                Eaten = _eaten
            End Get
            Set(value As Boolean)
                _eaten = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.eatenTick() as integer
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property EatenTick As Integer
            Get
                EatenTick = _eatenTick
            End Get
            Set(value As Integer)
                _eatenTick = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.fruit.list() as string
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property List As String
            Get
                List = _list
            End Get
            Set(value As String)
                _list = value
            End Set
        End Property

    End Class

    ' =============================================================================================================================
    '
    ' ghost Class
    '
    ' =============================================================================================================================

    Public Class Ghost

        Private _name As String
        Private _mode As GhostMode
        Private _pixel As Point
        Private _direction As ActorDirection
        Private _nextDirection As ActorDirection
        Private _signalReverse As Boolean
        Private _signalLeaveHome As Boolean
        Private _scared As Boolean
        Private _flashing As Boolean
        Private _targetting As Boolean
        Private _targetTile As Point
        Private _startPixel As Point
        Private _cornerTile As Point
        Private _startDirection As ActorDirection
        Private _startMode As GhostMode
        Private _arriveHomeMode As GhostMode
        Private _personalDotLimit As Integer
        Private _globalDotLimit As Integer
        Private _dotsCounter As Integer
        Private _directionChanged As Boolean
        Private _scaredChanged As Boolean
        Private _flashingChanged As Boolean
        Private _eatenPixel As Point
        Private _eatenTimer As Integer
        Private _eatenScore As Integer

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.name(as string)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Name As String
            Get
                Name = _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.state(as ghostMode)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Mode As GhostMode
            Get
                Mode = _mode
            End Get
            Set(value As GhostMode)
                _mode = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.pixel(as Point)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Pixel As Point
            Get
                Pixel = _pixel
            End Get
            Set(value As Point)
                _pixel = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.direction(as actorDirection)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Direction As ActorDirection
            Get
                Direction = _direction
            End Get
            Set(value As ActorDirection)
                If _direction <> value Then
                    _directionChanged = True
                End If
                _direction = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.nextDirection(as actorDirection)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property NextDirection As ActorDirection
            Get
                NextDirection = _nextDirection
            End Get
            Set(value As ActorDirection)
                _nextDirection = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.signalReverse(as boolean)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property SignalReverse As Boolean
            Get
                SignalReverse = _signalReverse
            End Get
            Set(value As Boolean)
                _signalReverse = value
                _directionChanged = True
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.signalLeaveHome(as boolean)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property SignalLeaveHome As Boolean
            Get
                SignalLeaveHome = _signalLeaveHome
            End Get
            Set(value As Boolean)
                _signalLeaveHome = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.scared(as boolean)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property Scared As Boolean
            Get
                Scared = _scared
            End Get
            Set(value As Boolean)
                If _scared <> value Then
                    _scaredChanged = True
                End If
                _scared = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.flashing(as boolean)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property Flashing As Boolean
            Get
                Flashing = _flashing
            End Get
            Set(value As Boolean)
                If _flashing <> value Then
                    _flashingChanged = True
                End If
                _flashing = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.targetting(as boolean)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property Targetting As Boolean
            Get
                Targetting = _targetting
            End Get
            Set(value As Boolean)
                _targetting = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.targetTile(as Point)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property TargetTile As Point
            Get
                TargetTile = _targetTile
            End Get
            Set(value As Point)
                _targetTile = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.targetPixel(as Point)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property TargetPixel As Point
            Get
                TargetPixel = _targetTile
                TargetPixel.X *= 8
                TargetPixel.Y *= 8
            End Get
            Set(value As Point)
                _targetTile.X = Int(TargetPixel.X / 8)
                _targetTile.Y = Int(TargetPixel.Y / 8)
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.startPixel(as Point)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property StartPixel As Point
            Get
                StartPixel = _startPixel
            End Get
            Set(value As Point)
                _startPixel = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.cornerTile(as Point)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property CornerTile As Point
            Get
                CornerTile = _cornerTile
            End Get
            Set(value As Point)
                _cornerTile = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.startDirection as actorDirection
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property StartDirection As ActorDirection
            Get
                StartDirection = _startDirection
            End Get
            Set(value As ActorDirection)
                _startDirection = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.startMode as ghostMode
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property StartMode As GhostMode
            Get
                StartMode = _startMode
            End Get
            Set(value As GhostMode)
                _startMode = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.arriveHomeMode as ghostMode
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property ArriveHomeMode As GhostMode
            Get
                ArriveHomeMode = _arriveHomeMode
            End Get
            Set(value As GhostMode)
                _arriveHomeMode = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.personalDotLimit as integer
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property PersonalDotLimit As Integer
            Get
                PersonalDotLimit = _personalDotLimit
            End Get
            Set(value As Integer)
                _personalDotLimit = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.globalDotLimit as integer
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property GlobalDotLimit As Integer
            Get
                GlobalDotLimit = _globalDotLimit
            End Get
            Set(value As Integer)
                _globalDotLimit = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.globalCounter as integer
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property DotsCounter As Integer
            Get
                DotsCounter = _dotsCounter
            End Get
            Set(value As Integer)
                _dotsCounter = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.directionChanged as boolean
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property DirectionChanged As Integer
            Get
                DirectionChanged = _directionChanged
            End Get
            Set(value As Integer)
                _directionChanged = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.directionChanged as boolean
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property ScaredChanged As Integer
            Get
                ScaredChanged = _scaredChanged
            End Get
            Set(value As Integer)
                _scaredChanged = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.eatenPixel as point
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property EatenPixel As Point
            Get
                EatenPixel = _eatenPixel
            End Get
            Set(value As Point)
                _eatenPixel = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.eatenTimer as integer
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property EatenTimer As Integer
            Get
                EatenTimer = _eatenTimer
            End Get
            Set(value As Integer)
                _eatenTimer = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.eatenScore as integer
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property EatenScore As Integer
            Get
                EatenScore = _eatenScore
            End Get
            Set(value As Integer)
                _eatenScore = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.directionChanged as boolean
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property FlashingChanged As Integer
            Get
                FlashingChanged = _flashingChanged
            End Get
            Set(value As Integer)
                _flashingChanged = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.atTileCenter(as boolean)
        ' -----------------------------------------------------------------------------------------------------------------------------

        ReadOnly Property AtTileCenter As Boolean
            Get
                AtTileCenter = False
                If _pixel.X Mod 8 = 3 And _pixel.Y Mod 8 = 4 Then
                    AtTileCenter = True
                End If
            End Get
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.justPassedCenter(as boolean)
        ' -----------------------------------------------------------------------------------------------------------------------------

        ReadOnly Property JustPassedCenter As Boolean
            Get
                JustPassedCenter = False
                If (_direction = ActorDirection.Right And Pixel.X Mod 8 = 4) Or
                    (_direction = ActorDirection.Left And Pixel.X Mod 8 = 2) Or
                    (_direction = ActorDirection.Up And Pixel.Y Mod 8 = 3) Or
                    (_direction = ActorDirection.Down And Pixel.Y Mod 8 = 5) Then
                    JustPassedCenter = True
                End If

            End Get
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.reverseDirection(as boolean)
        ' -----------------------------------------------------------------------------------------------------------------------------

        WriteOnly Property ReverseDirection As Boolean
            Set(value As Boolean)
                If value = True Then

                    Select Case _direction
                        Case ActorDirection.Up
                            _nextDirection = ActorDirection.Down
                        Case ActorDirection.Down
                            _nextDirection = ActorDirection.Up
                        Case ActorDirection.Left
                            _nextDirection = ActorDirection.Right
                        Case ActorDirection.Right
                            _nextDirection = ActorDirection.Left
                    End Select

                End If
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.tile() as Point
        ' -----------------------------------------------------------------------------------------------------------------------------

        ReadOnly Property Tile As Point
            Get

                Tile.X = Int(_pixel.X / 8)
                Tile.Y = Int(_pixel.Y / 8)

            End Get
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.nextTile() as Point
        ' -----------------------------------------------------------------------------------------------------------------------------

        ReadOnly Property NextTile As Point
            Get

                Dim tile As Point

                tile.X = Int(_pixel.X / 8)
                tile.Y = Int(_pixel.Y / 8)

                Select Case _direction
                    Case ActorDirection.Left
                        tile.X -= 1
                    Case ActorDirection.Right
                        tile.X += 1
                    Case ActorDirection.Up
                        tile.Y -= 1
                    Case ActorDirection.Down
                        tile.Y += 1
                End Select

                NextTile = tile

            End Get
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.ghost.randomDirection() as actorDirection
        ' -----------------------------------------------------------------------------------------------------------------------------

        ReadOnly Property RandomDirection As ActorDirection
            Get

                Dim rndDirection As Integer
                RandomDirection = ActorDirection.None

                Randomize()
                rndDirection = CInt(Math.Ceiling(Rnd() * 4))

                Select Case rndDirection
                    Case 0
                        RandomDirection = ActorDirection.Up
                    Case 1
                        RandomDirection = ActorDirection.Down
                    Case 2
                        RandomDirection = ActorDirection.Left
                    Case 3
                        RandomDirection = ActorDirection.Right
                End Select

            End Get
        End Property

    End Class

    ' =============================================================================================================================
    '
    ' Pacman Class
    '
    ' =============================================================================================================================

    Public Class PacMan

        Private _name As String
        Private _pixel As Point
        Private _startPixel As Point
        Private _direction As ActorDirection
        Private _nextDirection As ActorDirection
        Private _facing As ActorDirection
        Private _directionChanged As Boolean
        Private _died As Boolean

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.name(as string)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Name As String
            Get
                Name = _name
            End Get
            Set(value As String)
                _name = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.pixel(as Point)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Pixel As Point
            Get
                Pixel = _pixel
            End Get
            Set(value As Point)
                _pixel = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.startPixel(as Point)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property StartPixel As Point
            Get
                StartPixel = _startPixel
            End Get
            Set(value As Point)
                _startPixel = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.direction(as actorDirection)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Direction As ActorDirection
            Get
                Direction = _direction
            End Get
            Set(value As ActorDirection)
                If _direction <> value Then
                    _directionChanged = True
                End If
                _direction = value
                If _direction <> ActorDirection.None Then
                    _facing = _direction
                End If
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.nextDirection(as actorDirection)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property NextDirection As ActorDirection
            Get
                NextDirection = _nextDirection
            End Get
            Set(value As ActorDirection)
                _nextDirection = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.facing(as actorDirection)
        ' -----------------------------------------------------------------------------------------------------------------------------

        Public Property Facing As ActorDirection
            Get
                Facing = _facing
            End Get
            Set(value As ActorDirection)
                _facing = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.directionChanged as boolean
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property DirectionChanged As Integer
            Get
                DirectionChanged = _directionChanged
            End Get
            Set(value As Integer)
                _directionChanged = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.died as boolean
        ' -----------------------------------------------------------------------------------------------------------------------------

        Property Died As Integer
            Get
                Died = _died
            End Get
            Set(value As Integer)
                _died = value
            End Set
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.tile() as Point
        ' -----------------------------------------------------------------------------------------------------------------------------

        ReadOnly Property Tile As Point
            Get

                Tile.X = Int(_pixel.X / 8)
                Tile.Y = Int(_pixel.Y / 8)

            End Get
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.getNextTile() as Point
        ' -----------------------------------------------------------------------------------------------------------------------------

        ReadOnly Property NextTile As Point
            Get

                Dim tile As Point

                tile.X = Int(_pixel.X / 8)
                tile.Y = Int(_pixel.Y / 8)

                Select Case _direction
                    Case ActorDirection.Left
                        tile.X -= 1
                    Case ActorDirection.Right
                        tile.X += 1
                    Case ActorDirection.Up
                        tile.Y -= 1
                    Case ActorDirection.Down
                        tile.Y += 1
                End Select

                NextTile = tile

            End Get
        End Property

        ' -----------------------------------------------------------------------------------------------------------------------------
        ' actor.pacman.getDistanceFromCenter() as Point
        ' -----------------------------------------------------------------------------------------------------------------------------

        ReadOnly Property DistanceFromCenter As Point
            Get

                Dim p As Point
                p = New Point(0, 0) With {
                    .X = ((Pixel.X Mod 8) - 3),
                    .Y = ((Pixel.Y Mod 8) - 4)
                }

                DistanceFromCenter = p

            End Get
        End Property

    End Class

    ' =============================================================================================================================
    '
    ' Releaser Class
    '
    ' =============================================================================================================================

    Public Class Releaser

        Private Enum Mode
            modePersonal = 0
            modeGlobal = 1
        End Enum

        Private _mode As Integer
        Private _framesSinceLastDot As Integer
        Private _globalCount As Integer

        Public Sub NewLevel(level As Integer)

            _level = level
            _mode = Mode.modePersonal
            _framesSinceLastDot = 0

            For n = 0 To _ghost.Count - 1
                With _ghost(n)
                    Select Case .Name
                        Case "blinky"
                            .PersonalDotLimit = 0
                            .GlobalDotLimit = 0
                        Case "pinky"
                            .PersonalDotLimit = 0
                            .GlobalDotLimit = 7
                        Case "inky"
                            If level = 1 Then
                                .PersonalDotLimit = 30
                            Else
                                .PersonalDotLimit = 0
                            End If
                            .GlobalDotLimit = 17
                        Case "clyde"
                            If level = 1 Then .PersonalDotLimit = 30
                            If level = 2 Then .PersonalDotLimit = 60
                            If level > 2 Then .PersonalDotLimit = 0
                            .GlobalDotLimit = 32
                        Case Else
                            .PersonalDotLimit = 0
                            .GlobalDotLimit = 0
                    End Select

                    .DotsCounter = 0

                End With
            Next

            ' Set step size for each actor & state
            ' The step sizes cover 16 frame of movement in total, and the current step is tracked with the StepCounter variable
            ' When the end of the step size list is reached, the StepCounter is reset to 1 to start at the beginning of the list

            ' A step size of 0 indicates that no steps are taken in this frame
            ' A step size of 1 indicates that 1 step is taken in this frame
            ' A step size of 2 indicates that 2 steps are taken in this frame

            Select Case level

                Case Is = 1

                    _stepSize.pacmanNormal = "1111111111111111"
                    _stepSize.ghostsNormal = "0111111111111111"
                    _stepSize.pacmanFright = "1111211111112111"
                    _stepSize.ghostsFright = "0110110101101101"
                    _stepSize.ghostsTunnel = "0101010101010101"
                    _stepSize.ghostsPacing = "0101010101010101"
                    _stepSize.elroy1 = "1111111111111111"
                    _stepSize.elroy2 = "1111111121111111"

                Case Is <= 4

                    _stepSize.pacmanNormal = "1111211111112111"
                    _stepSize.ghostsNormal = "1111111121111111"
                    _stepSize.pacmanFright = "1111211112111121"
                    _stepSize.ghostsFright = "0110110110110111"
                    _stepSize.ghostsTunnel = "0110101011010101"
                    _stepSize.ghostsPacing = "0101010101010101"
                    _stepSize.elroy1 = "1111211111112111"
                    _stepSize.elroy2 = "1111211112111121"

                Case Is <= 20

                    _stepSize.pacmanNormal = "1121112111211121"
                    _stepSize.ghostsNormal = "1111211112111121"
                    _stepSize.pacmanFright = "1121112111211121"
                    _stepSize.ghostsFright = "0111011101110111"
                    _stepSize.ghostsTunnel = "0110110101101101"
                    _stepSize.ghostsPacing = "0101010101010101"
                    _stepSize.elroy1 = "1121112111211121"
                    _stepSize.elroy2 = "1121121121121121"

                Case Is > 20

                    _stepSize.pacmanNormal = "1111211111112111"
                    _stepSize.ghostsNormal = "1111211112111121"
                    _stepSize.pacmanFright = "1121112111211121"
                    _stepSize.ghostsFright = "0111011101110111"
                    _stepSize.ghostsTunnel = "0110110101101101"
                    _stepSize.ghostsPacing = "0101010101010101"
                    _stepSize.elroy1 = "1121112111211121"
                    _stepSize.elroy2 = "1121121121121121"

            End Select

            _stepCounter = 1

        End Sub

        Public Sub RestartLevel()

            _mode = Mode.modeGlobal
            _framesSinceLastDot = 0
            _globalCount = 0

        End Sub

        Public Sub DotEat()

            _framesSinceLastDot = 0

            If _mode = Mode.modeGlobal Then
                _globalCount += 1
            Else
                For n = 0 To _ghost.Count - 1
                    With _ghost(n)
                        If .Mode = GhostMode.GhostPacingHome Then
                            .DotsCounter += 1
                            Exit For
                        End If
                    End With
                Next
            End If

        End Sub

        Public Sub Update()

            Dim _timeoutLimit As Integer

            If _mode = Mode.modePersonal Then

                For n = 0 To _ghost.Count - 1
                    With _ghost(n)
                        If .Mode = GhostMode.GhostPacingHome Then
                            If .DotsCounter >= .PersonalDotLimit Then
                                .SignalLeaveHome = True
                                Exit Sub
                            End If
                            Exit For
                        End If
                    End With
                Next

            Else

                If _mode = Mode.modeGlobal Then

                    For n = 0 To _ghost.Count - 1
                        With _ghost(n)

                            Select Case .Name

                                Case "pinky"
                                    If _globalCount > .GlobalDotLimit And .Mode = GhostMode.GhostPacingHome Then
                                        .SignalLeaveHome = True
                                        Exit Sub
                                    End If

                                Case "inky"
                                    If _globalCount > .GlobalDotLimit And .Mode = GhostMode.GhostPacingHome Then
                                        .SignalLeaveHome = True
                                        Exit Sub
                                    End If

                                Case "clyde"
                                    If _globalCount > .GlobalDotLimit And .Mode = GhostMode.GhostPacingHome Then
                                        _globalCount = 0
                                        _mode = Mode.modePersonal
                                        .SignalLeaveHome = True
                                        Exit Sub
                                    End If

                            End Select

                        End With
                    Next

                End If
            End If

            If _level < 5 Then
                _timeoutLimit = 4 * 60
            Else
                _timeoutLimit = 3 * 60
            End If

            If _framesSinceLastDot > _timeoutLimit Then

                _framesSinceLastDot = 0
                For n = 0 To _ghost.Count - 1
                    With _ghost(n)
                        If .Mode = GhostMode.GhostPacingHome Then
                            .SignalLeaveHome = True
                            Exit For
                        End If
                    End With
                Next
            Else

                _framesSinceLastDot += 1

            End If

        End Sub

    End Class

    ' =============================================================================================================================
    '
    ' actor Class
    '
    ' =============================================================================================================================

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.addGhost
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub AddGhost(name As String, startPixel As Point, cornerTile As Point, startDirection As ActorDirection, startMode As GhostMode, arriveHomeMode As GhostMode)
        Dim gh As New Ghost With {
            .Name = name,
            .Mode = GhostMode.GhostPacingHome,
            .Pixel = startPixel,
            .Direction = startDirection,
            .NextDirection = startDirection,
            .SignalReverse = False,
            .SignalLeaveHome = False,
            .Scared = False,
            .Flashing = False,
            .Targetting = False,
            .StartPixel = startPixel,
            .CornerTile = cornerTile,
            .StartDirection = startDirection,
            .StartMode = startMode,
            .ArriveHomeMode = arriveHomeMode,
            .PersonalDotLimit = 0,
            .GlobalDotLimit = 0,
            .DotsCounter = 0,
            .EatenPixel = New Point(0, 0),
            .EatenTimer = 0
        }

        _ghost.Add(gh)

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.ghostByName(name as String) as ghost
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function GhostByName(name As String) As Ghost

        Dim index
        index = _ghost.FindIndex((Function(f) f.Name = name))
        Return _ghost(index)

    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.ghostByIndex(index as integer) as ghost
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function GhostByIndex(index As Integer) As Ghost

        Return _ghost(index)

    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.ghostIndexByName(name as String) as integer
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function GhostIndexByName(name As String) As Integer

        Return _ghost.FindIndex((Function(f) f.Name = name))

    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.setGhostState() as ghostState
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Property State As GhostState
        Get
            State = _ghostState
        End Get
        Set(value As GhostState)
            _ghostState = value
        End Set
    End Property

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.addPacman(n as String)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub AddPacman(n As String, position As Point, startDirection As ActorDirection)
        Dim pm As New PacMan With {
            .Name = n,
            .Pixel = position,
            .StartPixel = position,
            .Direction = startDirection,
            .NextDirection = ActorDirection.None,
            .Facing = ActorDirection.Left,
            .DirectionChanged = False,
            .Died = False
        }

        _pacman.Add(pm)

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.pacmanByName(name as String) as pacman
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function PacmanByName(name As String) As PacMan

        Dim index
        index = _pacman.FindIndex((Function(f) f.Name = name))
        Return _pacman(index)

    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.pacmanByIndex(index as integer) as pacman
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function PacmanByIndex(index As Integer) As PacMan

        Return _pacman(index)

    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.getPacmanIndexByName(name as String) as integer
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function GetPacmanIndexByName(name As String) As Integer

        Return _pacman.FindIndex((Function(f) f.Name = name))

    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.resetPacman()
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub ResetPacman()

        For n = 0 To _pacman.Count - 1
            With _pacman(n)

                .Pixel = .StartPixel
                .Direction = ActorDirection.Left
                .NextDirection = ActorDirection.None
                .Facing = ActorDirection.Left
                .DirectionChanged = True
                .Died = False

            End With
        Next

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.resetGhosts()
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub ResetGhost()

        For n = 0 To _ghost.Count - 1
            With _ghost(n)

                .SignalReverse = False
                .SignalLeaveHome = False

                .Mode = .StartMode
                .Scared = False
                .Flashing = False

                .Direction = .StartDirection
                .Pixel = .StartPixel
                .Targetting = False

                .DirectionChanged = True
                .ScaredChanged = True
                .FlashingChanged = True

                .TargetTile = .CornerTile

            End With
        Next

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.setGhostState(timer as long)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Sub SetGhostState(timer As Long)

        If _level = 1 Then

            Select Case timer
                Case Is < (7 * 60)
                    State = Actor.GhostState.Scatter
                Case Is < (27 * 60)
                    State = Actor.GhostState.Chase
                Case Is < (34 * 60)
                    State = Actor.GhostState.Scatter
                Case Is < (54 * 60)
                    State = Actor.GhostState.Chase
                Case Is < (59 * 60)
                    State = Actor.GhostState.Scatter
                Case Is < (79 * 60)
                    State = Actor.GhostState.Chase
                Case Is < (84 * 60)
                    State = Actor.GhostState.Scatter
                Case Is >= (84 * 60)
                    State = Actor.GhostState.Chase
            End Select

        Else
            If _level > 2 And _level < 5 Then

                Select Case timer
                    Case Is < (7 * 60)
                        State = Actor.GhostState.Scatter
                    Case Is < (27 * 60)
                        State = Actor.GhostState.Chase
                    Case Is < (34 * 60)
                        State = Actor.GhostState.Scatter
                    Case Is < (54 * 60)
                        State = Actor.GhostState.Chase
                    Case Is < (59 * 60)
                        State = Actor.GhostState.Scatter
                    Case Is < (1092 * 60)
                        State = Actor.GhostState.Chase
                    Case Is < (1092 * 60) + 1
                        State = Actor.GhostState.Scatter
                    Case Is >= (1092 * 60) + 1
                        State = Actor.GhostState.Chase
                End Select

            Else

                Select Case timer
                    Case Is < (5 * 60)
                        State = Actor.GhostState.Scatter
                    Case Is < (25 * 60)
                        State = Actor.GhostState.Chase
                    Case Is < (30 * 60)
                        State = Actor.GhostState.Scatter
                    Case Is < (50 * 60)
                        State = Actor.GhostState.Chase
                    Case Is < (55 * 60)
                        State = Actor.GhostState.Scatter
                    Case Is < (1092 * 60)
                        State = Actor.GhostState.Chase
                    Case Is < (1092 * 60) + 1
                        State = Actor.GhostState.Scatter
                    Case Is >= (1092 * 60) + 1
                        State = Actor.GhostState.Chase
                End Select

            End If
        End If

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.update(ByRef as maze)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub Update(ByRef maze As Maze, invincible As Boolean)

        For f = 1 To 2
            UpdateGhost(maze, f, invincible)
            UpdatePacman(maze, f, invincible)
        Next

        ghostReleaser.Update()

        UpdateFruit()

        If _energizedTimer > 0 Then
            If _energizedTimer = _energizedFlashTimer Then
                For n = 0 To _ghost.Count - 1
                    If _ghost(n).Scared = True Then
                        _ghost(n).Flashing = True
                    End If
                Next
            End If
            _energizedTimer -= 1
        Else
            For n = 0 To _ghost.Count - 1
                _ghost(n).Scared = False
                _ghost(n).Flashing = False
            Next
        End If

        _stepCounter += 1
        If _stepCounter > 16 Then _stepCounter = 1

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.updateGhost(ByRef as maze)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub UpdateGhost(ByRef maze As Maze, f As Integer, invincible As Boolean)

        Dim steps As Integer

        ' Loop through all ghost items

        For n = 0 To _ghost.Count - 1

            ' Using the current ghost item...

            With _ghost(n)

                If .EatenTimer > 0 Then
                    .EatenTimer -= 1
                End If

                If .Mode = GhostMode.GhostEaten Then
                    .EatenPixel = .Pixel
                    .EatenTimer = (60 * 3)
                    .Mode = GhostMode.GhostGoingHome
                End If

                If .Mode = GhostMode.GhostGoingHome Or .Mode = GhostMode.GhostEnteringHome Then
                    steps = 2
                Else
                    If .Mode = GhostMode.GhostLeavingHome Or .Mode = GhostMode.GhostPacingHome Then
                        steps = Int(Mid(_stepSize.ghostsPacing, _stepCounter, 1))
                    Else
                        If .Tile.Y = 14 And (.Pixel.X < 40 Or .Pixel.X > 184) Then
                            steps = Int(Mid(_stepSize.ghostsTunnel, _stepCounter, 1))
                        Else
                            If .Scared Then
                                steps = Int(Mid(_stepSize.ghostsFright, _stepCounter, 1))
                            Else
                                ' ELROY TODO
                                steps = Int(Mid(_stepSize.ghostsNormal, _stepCounter, 1))
                            End If
                        End If
                    End If
                End If

                If f <= steps Then

                    ' Perform all home logic

                    Select Case .Mode
                        Case GhostMode.GhostGoingHome
                            If .Tile = maze.homeDoorTile Then
                                .Direction = ActorDirection.Down
                                .Targetting = False
                                If .Pixel.X = maze.homeDoorPixel.X Then
                                    .Mode = GhostMode.GhostEnteringHome
                                    .Direction = ActorDirection.Down
                                Else
                                    .Direction = ActorDirection.Right
                                End If
                            End If

                        Case GhostMode.GhostEnteringHome
                            If .Pixel.Y = maze.homeBottomPixel Then
                                If .Pixel.X = .StartPixel.X Then
                                    .Direction = ActorDirection.Up
                                    .Mode = .ArriveHomeMode
                                Else
                                    If .StartPixel.X < .Pixel.X Then
                                        .Direction = ActorDirection.Left
                                    Else
                                        .Direction = ActorDirection.Right
                                    End If
                                End If
                            End If

                        Case GhostMode.GhostPacingHome
                            If .SignalLeaveHome = True Then
                                .SignalLeaveHome = False
                                .Mode = GhostMode.GhostLeavingHome
                                If .Pixel.X = maze.homeDoorPixel.X Then
                                    .Direction = ActorDirection.Up
                                Else
                                    If .Pixel.X < maze.homeDoorPixel.X Then
                                        .Direction = ActorDirection.Right
                                    Else
                                        .Direction = ActorDirection.Left
                                    End If
                                End If
                            Else
                                If .Pixel.Y = maze.homeTopPixel Then
                                    .Direction = ActorDirection.Down
                                Else
                                    If .Pixel.Y = maze.homeBottomPixel Then
                                        .Direction = ActorDirection.Up
                                    End If
                                End If
                            End If

                        Case GhostMode.GhostLeavingHome
                            If .Pixel.X = maze.homeDoorPixel.X Then
                                If .Pixel.Y = maze.homeDoorPixel.Y Then
                                    .Mode = GhostMode.GhostOutside
                                    Randomize()
                                    If CInt(Math.Ceiling(Rnd() * 2 - 1)) = 0 Then
                                        .Direction = ActorDirection.Left
                                        .NextDirection = ActorDirection.Left
                                    Else
                                        .Direction = ActorDirection.Right
                                        .NextDirection = ActorDirection.Right
                                    End If
                                Else
                                    .Direction = ActorDirection.Up
                                End If
                            End If

                    End Select

                    ' If we are not pursuing a target tile then exit the subroutine as we are done

                    If (.Mode <> GhostMode.GhostOutside And .Mode <> GhostMode.GhostGoingHome) Then
                        .Targetting = False
                    Else

                        ' Are we are at the middle of a tile?

                        If .AtTileCenter = True Then

                            ' If reversal has been triggered then do so

                            If .SignalReverse = True Then
                                .ReverseDirection = True
                                .SignalReverse = False
                            End If

                            ' Commit the new direction

                            .Direction = .NextDirection

                        Else

                            ' Have we just passed the mid-tile?

                            If .JustPassedCenter = True Then

                                ' Get next tile

                                Dim tilePos As Point
                                tilePos = .NextTile

                                ' Get exits from next tile

                                'Dim ex As maze.exits
                                'ex = maze.getExits(tilePos)

                                Dim ex As List(Of Maze.MazeExits)
                                ex = maze.GetExits(tilePos)

                                ' With the standard Pac-man map there are always at least two exits from each tile,
                                ' however, a user created map may have a dead-end. For this reason, we must allow a
                                ' ghost to exit the way it came...but ONLY if there is a single exit.
                                ' Otherwise we prohibit a ghost choosing an exit that is in the opposite direction
                                ' of travel.

                                If ex.Count > 1 Then
                                    Select Case .Direction
                                        Case ActorDirection.Up
                                            For i = ex.Count - 1 To 0 Step -1
                                                If ex(i) = Maze.MazeExits.exitDown Then
                                                    ex.RemoveAt(i)
                                                End If
                                            Next
                                            'ex.down = False
                                        Case ActorDirection.Down
                                            For i = ex.Count - 1 To 0 Step -1
                                                If ex(i) = Maze.MazeExits.exitUp Then
                                                    ex.RemoveAt(i)
                                                End If
                                            Next
                                            'ex.up = False
                                        Case ActorDirection.Left
                                            For i = ex.Count - 1 To 0 Step -1
                                                If ex(i) = Maze.MazeExits.exitRight Then
                                                    ex.RemoveAt(i)
                                                End If
                                            Next
                                            'ex.right = False
                                        Case ActorDirection.Right
                                            For i = ex.Count - 1 To 0 Step -1
                                                If ex(i) = Maze.MazeExits.exitLeft Then
                                                    ex.RemoveAt(i)
                                                End If
                                            Next
                                            'ex.left = False
                                    End Select
                                End If

                                ' If a ghost is scared then its moves are random;

                                If .Scared = True Then

                                    ' Get Random Exit

                                    Randomize()

                                    Dim randomExit As Integer
                                    randomExit = CInt(Math.Ceiling(Rnd() * (ex.Count - 1)))
                                    .NextDirection = ex(randomExit)

                                Else

                                    If .Mode = GhostMode.GhostGoingHome Then

                                        .TargetTile = maze.homeDoorTile

                                    Else

                                        If State = GhostState.Scatter Then

                                            .TargetTile = .CornerTile
                                            .Targetting = True

                                        Else

                                            Select Case _ghost(n).Name
                                                Case "blinky"
                                                    .TargetTile = BlinkyGetTargetTile()
                                                Case "pinky"
                                                    .TargetTile = PinkyGetTargetTile()
                                                Case "inky"
                                                    .TargetTile = InkyGetTargetTile()
                                                Case "clyde"
                                                    .TargetTile = ClydeGetTargetTile()
                                            End Select

                                        End If


                                    End If

                                    ' Find shortest path to target tile

                                    Dim dist As Point
                                    Dim distance As Long
                                    Dim distanceSelected As Long

                                    distanceSelected = 1000000
                                    For i = 0 To ex.Count - 1
                                        Select Case ex(i)
                                            Case Maze.MazeExits.exitUp
                                                dist = Point.Subtract(Point.Add(.Tile(), New Point(0, -1)), .TargetTile)
                                            Case Maze.MazeExits.exitDown
                                                dist = Point.Subtract(Point.Add(.Tile(), New Point(0, 1)), .TargetTile)
                                            Case Maze.MazeExits.exitLeft
                                                dist = Point.Subtract(Point.Add(.Tile(), New Point(-1, 0)), .TargetTile)
                                            Case Maze.MazeExits.exitRight
                                                dist = Point.Subtract(Point.Add(.Tile(), New Point(1, 0)), .TargetTile)
                                        End Select

                                        distance = (dist.X * dist.X) + (dist.Y * dist.Y)
                                        If distance < distanceSelected Then
                                            distanceSelected = distance
                                            .NextDirection = ex(i)
                                        End If

                                    Next

                                End If

                            End If

                        End If
                    End If

                    Select Case .Direction
                        Case ActorDirection.Up
                            .Pixel = Point.Add(.Pixel, New Size(0, -1))
                        Case ActorDirection.Down
                            .Pixel = Point.Add(.Pixel, New Size(0, 1))
                        Case ActorDirection.Left
                            .Pixel = Point.Add(.Pixel, New Size(-1, 0))
                        Case ActorDirection.Right
                            .Pixel = Point.Add(.Pixel, New Size(1, 0))
                    End Select

                    ' Deal with tunnel

                    If .Tile.Y = 14 Then
                        If .Pixel.X < -16 Then
                            .Pixel = New Point(239, .Pixel.Y)
                        Else
                            If .Pixel.X > 239 Then
                                .Pixel = New Point(-16, .Pixel.Y)
                            End If
                        End If
                    End If

                    If Not invincible Then
                        For i = 0 To _pacman.Count - 1
                            If .Tile = _pacman(i).Tile Then
                                Collision(i, n)
                            End If
                        Next
                    End If


                End If

            End With

        Next

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.updatePacman(ByRef as maze)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub UpdatePacman(ByRef maze As Maze, f As Integer, invincible As Boolean)

        Dim steps As Integer

        steps = Int(Mid(_stepSize.pacmanNormal, _stepCounter, 1))
        If f > steps Then Exit Sub

        ' Loop through all pacman items

        For n = 0 To _pacman.Count - 1

            ' Using the current ghost item...

            With _pacman(n)

                ' Get next tile

                Dim tilePos As Point
                tilePos = .Tile

                Dim ex As List(Of Maze.MazeExits)
                ex = maze.GetExits(tilePos)

                Dim distFromCenter As Point
                distFromCenter = .DistanceFromCenter

                Select Case .NextDirection

                    Case ActorDirection.Left
                        If ex.IndexOf(ActorDirection.Left) < 0 Then
                            If distFromCenter.X = 0 Then
                                .NextDirection = ActorDirection.None
                            End If
                        End If

                    Case ActorDirection.Right
                        If ex.IndexOf(ActorDirection.Right) < 0 Then
                            If distFromCenter.X = 0 Then
                                .NextDirection = ActorDirection.None
                            End If
                        End If

                    Case ActorDirection.Up
                        If ex.IndexOf(ActorDirection.Up) < 0 Then
                            If distFromCenter.Y = 0 Then
                                .NextDirection = ActorDirection.None
                            End If
                        End If

                    Case ActorDirection.Down
                        If ex.IndexOf(ActorDirection.Down) < 0 Then
                            If distFromCenter.Y = 0 Then
                                .NextDirection = ActorDirection.None
                            End If
                        End If

                End Select

                ' Stop moving if the exit is blocked in the direction of travel

                If .Direction = ActorDirection.Left Or .Direction = ActorDirection.Right Then
                    If ex.IndexOf(.Direction) < 0 Then
                        If distFromCenter.X = 0 Then
                            .Direction = ActorDirection.None
                        End If
                    End If
                End If

                If .Direction = ActorDirection.Up Or .Direction = ActorDirection.Down Then
                    If ex.IndexOf(.Direction) < 0 Then
                        If distFromCenter.Y = 0 Then
                            .Direction = ActorDirection.None
                        End If
                    End If
                End If

                ' Update direction if it has changed

                If .NextDirection <> ActorDirection.None Then
                    .Direction = .NextDirection
                End If

                Select Case .Direction
                    Case ActorDirection.Up
                        .Pixel = Point.Add(.Pixel, New Size(0, -1))
                        If distFromCenter.X < 0 Then
                            .Pixel = Point.Add(.Pixel, New Size(1, 0))
                        Else
                            If distFromCenter.X > 0 Then
                                .Pixel = Point.Add(.Pixel, New Size(-1, 0))
                            End If
                        End If
                    Case ActorDirection.Down
                        .Pixel = Point.Add(.Pixel, New Size(0, 1))
                        If distFromCenter.X < 0 Then
                            .Pixel = Point.Add(.Pixel, New Size(1, 0))
                        Else
                            If distFromCenter.X > 0 Then
                                .Pixel = Point.Add(.Pixel, New Size(-1, 0))
                            End If
                        End If
                    Case ActorDirection.Left
                        .Pixel = Point.Add(.Pixel, New Size(-1, 0))
                        If distFromCenter.Y < 0 Then
                            .Pixel = Point.Add(.Pixel, New Size(0, 1))
                        Else
                            If distFromCenter.Y > 0 Then
                                .Pixel = Point.Add(.Pixel, New Size(0, -1))
                            End If
                        End If
                    Case ActorDirection.Right
                        .Pixel = Point.Add(.Pixel, New Size(1, 0))
                        If distFromCenter.Y < 0 Then
                            .Pixel = Point.Add(.Pixel, New Size(0, 1))
                        Else
                            If distFromCenter.Y > 0 Then
                                .Pixel = Point.Add(.Pixel, New Size(0, -1))
                            End If
                        End If
                End Select

                ' Deal with tunnel

                If .Tile.Y = 14 Then
                    If .Pixel.X < -16 Then
                        .Pixel = New Point(239, .Pixel.Y)
                    Else
                        If .Pixel.X > 239 Then
                            .Pixel = New Point(-16, .Pixel.Y)
                        End If
                    End If
                End If

                If Not invincible Then
                    For i = 0 To _ghost.Count - 1
                        If .Tile = _ghost(i).Tile Then
                            Collision(n, i)
                        End If
                    Next
                End If

                ' If Pac-Man has eaten a fruit...
                For i = 0 To _fruit.Count - 1
                    If _fruit(i).Active Then
                        If .Tile = _fruit(i).Tile Then
                            _fruit(i).Eaten = True
                            _fruit(i).EatenTick = (3 * 60)
                            _fruit(i).Active = False
                        End If
                    End If
                Next

            End With

        Next

    End Sub

    ' Blinky direction targets Pacman 

    Private Function BlinkyGetTargetTile() As Point

        Dim index As Integer
        Dim pos As Point

        index = GetPacmanIndexByName("pacman")

        If index <> -1 Then
            pos = _pacman(index).Tile()
        Else
            pos = New Point(1, 1)
        End If

        Return pos

    End Function

    ' Pinky 

    Private Function PinkyGetTargetTile() As Point

        Dim index As Integer
        Dim pos As Point

        index = GetPacmanIndexByName("pacman")

        If index <> -1 Then
            pos = _pacman(index).Tile()

            Select Case _pacman(index).Facing
                Case ActorDirection.Left
                    pos = Point.Add(pos, New Size(-4, 0))
                Case ActorDirection.Right
                    pos = Point.Add(pos, New Size(4, 0))
                Case ActorDirection.Up
                    pos = Point.Add(pos, New Size(-4, -4))
                Case ActorDirection.Down
                    pos = Point.Add(pos, New Size(0, 4))

            End Select
        Else
            pos = New Point(1, 1)
        End If

        Return pos

    End Function

    Private Function InkyGetTargetTile() As Point

        Dim index1 As Integer
        Dim index2 As Integer
        Dim pos1 As Point
        Dim pos2 As Point
        Dim pos3 As Point

        index1 = GetPacmanIndexByName("pacman")
        index2 = GhostIndexByName("blinky")

        If index1 <> -1 And index2 <> -1 Then

            pos1 = _pacman(index1).Tile()
            pos2 = _ghost(index2).Tile()

            Select Case _pacman(index1).Facing
                Case ActorDirection.Left
                    pos1 = Point.Add(pos1, New Size(-2, 0))
                Case ActorDirection.Right
                    pos1 = Point.Add(pos1, New Size(2, 0))
                Case ActorDirection.Up
                    pos1 = Point.Add(pos1, New Size(-2, -2))
                Case ActorDirection.Down
                    pos1 = Point.Add(pos1, New Size(0, 2))
            End Select

            pos3 = Point.Subtract(pos2, pos1)
            pos3 = Point.Add(pos3, pos3)
            pos1 = Point.Subtract(pos1, pos3)

        Else

            pos1 = New Point(1, 1)

        End If

        Return pos1

    End Function

    Private Function ClydeGetTargetTile() As Point

        Dim index1 As Integer
        Dim index2 As Integer
        Dim pos1 As Point
        Dim pos2 As Point
        Dim pos3 As Point
        Dim dist As Long

        index1 = GetPacmanIndexByName("pacman")
        index2 = GhostIndexByName("clyde")

        If index1 <> -1 And index2 <> -1 Then

            pos1 = _pacman(index1).Tile()
            pos2 = _ghost(index2).Tile()
            pos3 = Point.Subtract(pos1, pos2)

            dist = pos3.X * pos3.X + pos3.Y * pos3.Y
            If dist < 64 Then
                pos1 = _ghost(index2).CornerTile
            End If
        End If

        Return pos1

    End Function

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.collision(pacmanNumber as integer, ghostNumber as integer)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Private Sub Collision(pacmanNumber As Integer, ghostNumber As Integer)

        If _energizedTimer > 0 Then
            If _ghost(ghostNumber).Scared = True Then
                _ghost(ghostNumber).Mode = GhostMode.GhostEaten
                _ghost(ghostNumber).Scared = False
                _ghost(ghostNumber).Flashing = False
                _ghost(ghostNumber).EatenScore = _energizedScore
                _energizedScore += 1
            Else
                If _ghost(ghostNumber).Mode <> GhostMode.GhostEaten And _ghost(ghostNumber).Mode <> GhostMode.GhostGoingHome Then
                    _pacman(pacmanNumber).Died = True
                End If
            End If
        Else
            If _ghost(ghostNumber).Mode <> GhostMode.GhostEaten And _ghost(ghostNumber).Mode <> GhostMode.GhostGoingHome Then
                _pacman(pacmanNumber).Died = True
            End If
        End If

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.energize()
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub Energize()
        If _level < 19 Then
            _energizedFlashTimer = _energizeFlashTime(_level - 1) * 14 * 2
            _energizedTimer = (_energizeTime(_level - 1) * 60) + _energizedFlashTimer
        Else
            _energizedFlashTimer = _energizeFlashTime(17) * 14 * 2
            _energizedTimer = (_energizeTime(17) * 60) + _energizedFlashTimer
        End If
        For n = 0 To _ghost.Count - 1
            If _ghost(n).Mode <> GhostMode.GhostGoingHome Then
                _ghost(n).Scared = True
                _ghost(n).Flashing = False
                _ghost(n).ReverseDirection = True
            End If
        Next
        _energizedScore = 0
    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.addFruit(name as string, pixel as point)
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub AddFruit(name As String, position As Point)
        Dim fr As New Fruit With {
            .Name = name,
            .Pixel = position,
            .Number = 1,
            .active = False,
            .tick = 0,
            .eaten = False,
            .eatenTick = 0,
            .list = ""
        }

        _fruit.Add(fr)

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.resetFruit()
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub ResetFruit()

        Dim _fruitSequence() = {0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7}
        Dim _fruitPoints() = {100, 300, 500, 700, 1000, 2000, 3000, 5000}

        For n = 0 To _fruit.Count - 1
            With _fruit(n)
                If _level < 14 Then
                    .Number = _fruitSequence(_level - 1)
                Else
                    .Number = _fruitSequence(13)
                End If
                .points = _fruitPoints(.Number)

                .list = ""
                For l = 0 To 6
                    If _level - l - 1 >= 0 Then
                        If _level - l - 1 < 14 Then
                            .list = Trim(Str(_fruitSequence(_level - l - 1))) & .list
                        Else
                            .list = Trim(Str(_fruitSequence(13))) & .list
                        End If
                    Else
                        .list = " " & .list
                    End If
                Next

                .active = False
                .tick = 0
                .eatenTick = 0

            End With
        Next

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.updateFruit()
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Sub UpdateFruit()

        For n = 0 To _fruit.Count - 1
            With _fruit(n)
                If .active Then

                    If .tick > 0 Then
                        .tick -= 1
                    Else
                        .active = False
                    End If

                Else

                    If .eatenTick > 0 Then
                        .eatenTick -= 1
                    End If

                End If
            End With
        Next

    End Sub

    ' -----------------------------------------------------------------------------------------------------------------------------
    ' actor.fruitByName(name as String) as fruit
    ' -----------------------------------------------------------------------------------------------------------------------------

    Public Function FruitByName(name As String) As Fruit

        Dim index
        index = _fruit.FindIndex((Function(f) f.Name = name))
        Return _fruit(index)

    End Function

    'Public Shared Event pacmanChanged(n As Integer)
    'Public Shared Event ghostChanged(n As Integer)

End Class
