Imports System.IO

Public Class PacMan

    Public Const TILE_SIZE = 8
    Public Const SPRITE_SIZE = 16
    Public Const CLIENT_WIDTH = 224
    Public Const CLIENT_HEIGHT = 288
    Public Const CLIENT_SCALE = 2
    Public Const FPS = 60

    ' Create a new instance of the gameEngine and specify that we want events.
    ' GameEngine exposes two public events that get called on each game loop
    ' GeGameLogic (this should contain the game logic)
    ' GeRenderScene (this is used to perform additional rendering, or gameEngine logic)

    Dim WithEvents GameEngine As GameEngine
    Friend Shared maze As New Maze
    Dim WithEvents Actor As New Actor

    ' Determines whether the debugging logic is called. The default state is off

    Structure GhostData
        Public name As String
        Public startPixel As Point
        Public cornerTile As Point
        Public startDirection As Actor.ActorDirection
        Public startMode As Actor.ghostMode
        Public arriveHomeMode As Actor.ghostMode
    End Structure

    Structure PacManData
        Public name
        Public startPixel As Point
        Public startDirection As Actor.ActorDirection
    End Structure

    Dim ghosts(3) As GhostData
    Dim pacman(0) As PacManData

    Dim level As Integer
    Dim score As Integer
    Dim highScore As Integer
    Dim lives As Integer
    Dim stepCount As Integer
    Dim state As GameState = GameState.Reset

    Enum GameState
        Reset = 0
        Menu = 1
        GetReady = 2
        GameStarted = 3
        PlayerDied = 4
    End Enum

    Dim debug As Boolean = False
    Dim invincibile As Boolean = False
    Dim redrawStatus As Boolean = False
    ' -----------------------------------------------------------------------------------------------------------

    Structure KbInput
        Dim left As Boolean
        Dim right As Boolean
        Dim up As Boolean
        Dim down As Boolean
        Dim space As Boolean
    End Structure

    Dim input As KbInput

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializeGame()

    End Sub

    Private Sub TransitionTo(state As GameState)
        Select Case state
            Case GameState.GetReady
                bgmMainTheme.Stop()
                bgmGetReady.PlayOnce()
            Case GameState.GameStarted
                bgmMainTheme.PlayLooping()
            Case Else
                bgmGetReady.Stop()
                bgmMainTheme.Stop()
        End Select
        Me.state = state
    End Sub

    Public Sub GameEngine_GE_GameLogic() Handles GameEngine.GE_GameLogic
        Dim last_score As Integer
        last_score = score


        ' This is the geGameLogic event.
        ' This is triggered once per frame by the gameEngine.
        ' It is meant to handle game logic ONLY.

        Select Case state
            Case GameState.Reset

                ' We are in reset mode
                ' This mode lasts a since frame and its sole purpose is to reset the game variables,
                ' display the pacman logo, border and turn the pacman sprite off.
                ' Although pacman is not displayed it is still active as an actor and the ghosts still
                ' target based on his starting position.

                ResetGame()

                ' Turn on the Pac-Man Logo
                GameEngine.SpriteByName("pacmanlogo2").Enabled = True

                'Turn on the Border (a transparent window)
                GameEngine.SpriteByName("pacmanborder2").Enabled = True

                ' Disable the Pac-Man sprite
                GameEngine.SpriteByName("pacman").Enabled = False

                ' Don't move Pac-Man in any direction
                Actor.PacmanByName("pacman").Direction = Actor.ActorDirection.None

                ' Set the state to "Menu"
                TransitionTo(GameState.Menu)

            Case GameState.Menu
                ' We are in menu mode (attract mode)
                ' In this mode pacman is hidden and the ghosts wander the maze alternating
                ' between scatter and chase mode every 10 seconds

                ' Update the actors.
                ' Because Pac-Man is disabled, we only update the ghosts
                Actor.Update(maze, invincibile)

                ' If 1800 frames have passed (30 seconds), then reset the internal clock
                If GameEngine.Clock > 1800 Then
                    GameEngine.Clock = 0
                End If

                ' If 600 frames have passed (10 seconds), then the ghosts are in scatter mode.
                ' Otherwise, the ghosts are in chase mode.
                If GameEngine.Clock < 600 Then
                    Actor.State = Actor.GhostState.Scatter
                Else
                    Actor.State = Actor.GhostState.Chase
                End If

                ' If the Spacebar has been pressed then the player wants to start the game.
                If input.space = True Then

                    ' Turn off the Pac-Man logo
                    GameEngine.SpriteByName("pacmanlogo2").Enabled = False

                    ' Turn off the border
                    GameEngine.SpriteByName("pacmanborder2").Enabled = False

                    ' Enable the Pac-Man sprite
                    GameEngine.SpriteByName("pacman").Enabled = True

                    ' Set Pac-Man's initial direction to left
                    Actor.PacmanByName("pacman").Direction = Actor.ActorDirection.Left

                    ' Reset the level
                    ResetLevel()

                    ' Set the state to "getReady"
                    TransitionTo(GameState.GetReady)
                End If

            Case GameState.GetReady
                ' We are in getReady mode.
                ' In this mode the text "Get Ready!" is displayed for 120 frames (2 seconds)

                ' If 120 frames have passed (3.5 seconds) then we have displayed the 
                ' "Get Ready!" messages for long enough, so move the game state onwards.
                If GameEngine.Clock > CInt(60 * 3.5) Then

                    ' Set the state to "gameStarted"
                    TransitionTo(GameState.GameStarted)
                End If

            Case GameState.GameStarted
                ' We are in the gameStarted mode.
                ' In this mode the player is actually playing the game, so this handles the
                ' main game logic.

                ' Push input into actor Class
                ' The actor class will apply this to Pac-Man movement

                ' If the input resolves to up then set the Pac-Man actor's direction to up
                If input.up Then
                    Actor.PacmanByName("pacman").NextDirection = Actor.ActorDirection.Up
                End If

                ' If the input resolves to down then set the Pac-Man actor's direction to down
                If input.down Then
                    Actor.PacmanByName("pacman").NextDirection = Actor.ActorDirection.Down
                End If

                ' If the input resolves to left then set the Pac-Man actor's direction to left
                If input.left Then
                    Actor.PacmanByName("pacman").NextDirection = Actor.ActorDirection.Left
                End If

                ' If the input resolves to right then set the Pac-Man actor's direction to right
                If input.right Then
                    Actor.PacmanByName("pacman").NextDirection = Actor.ActorDirection.Right
                End If

                ' Update the actors
                Actor.Update(maze, invincibile)

                ' If Pacman has eaten a dot...
                If maze.Data(Actor.PacmanByName("pacman").Tile) = Maze.MazeObjects.dot Then
                    sndDotEaten.PlayOnce()
                    ' Increase the score by 10 points
                    score += 10

                    ' Update the maze, replacing the dot with a blank tile
                    maze.Data(Actor.PacmanByName("pacman").Tile) = Maze.MazeObjects.blank

                    ' Update the gameEngine map, replacing the dot with a blank tile
                    GameEngine.MapByName("main").Value(Point.Add(Actor.PacmanByName("pacman").Tile, New Point(0, 3))) = Maze.MazeObjects.blank

                    ' Inform the ghost releaser that a dot has been eaten
                    Actor.ghostReleaser.DotEat()

                    ' When 70 or 170 dots have been eaten, we should update the fruit state
                    If maze.DotEaten = 70 Or maze.DotEaten = 170 Then

                        ' If the fruit is not already active we can active it
                        If Actor.FruitByName("fruit").Active = False Then

                            ' Fruit stays active for 540 frames (9 seconds)
                            Actor.FruitByName("fruit").Tick = (9 * 60)

                            ' Enable the fruit
                            Actor.FruitByName("fruit").Active = True
                        End If
                    End If

                    ' If the number of dots eaten equal the total maze dot count then
                    ' the maze is complete and we need to start the next level
                    If maze.DotEaten = maze.DotCount Then

                        ' Increment the level counter
                        level += 1

                        ' Perform next level actions
                        NextLevel()

                        ' Set the state to "getReady"
                        TransitionTo(GameState.GetReady)
                    End If

                End If

                ' If Pacman has eaten an energizer...
                If maze.Data(Actor.PacmanByName("pacman").Tile) = Maze.MazeObjects.energizer Then
                    sndPowerEaten.PlayOnce()
                    ' Increase the score by 50 points
                    score += 50

                    ' Inform the actor class that an energizer is active
                    Actor.Energize()

                    ' Update the maze, replacing the energizer with a blank tile
                    maze.Data(Actor.PacmanByName("pacman").Tile) = Maze.MazeObjects.blank

                    ' Update the gameEngine map, replacing the energizer with a blank tile
                    GameEngine.MapByName("main").Value(Point.Add(Actor.PacmanByName("pacman").Tile, New Point(0, 3))) = Maze.MazeObjects.blank

                    ' If the number of dots eaten equal the total maze dot count then
                    ' the maze is complete and we need to start the next level
                    If maze.DotEaten = maze.DotCount Then

                        ' Increment the level counter
                        level += 1

                        ' Perform next level actions
                        NextLevel()

                        ' Set the state to "getReady"
                        TransitionTo(GameState.GetReady)
                    End If

                End If

                ' If Pacman has died...
                If Actor.PacmanByName("pacman").Died = True Then
                    sndLifeLost.PlayOnce()
                    ' Reset the game engine clock
                    GameEngine.Clock = 0

                    ' Set the state to "playerDied"
                    TransitionTo(GameState.PlayerDied)

                End If

                ' If Pac-Man has eaten a fruit...
                If Actor.FruitByName("fruit").Eaten = True Then
                    sndFruitEaten.PlayOnce()
                    ' Increase the score by whatever the fruit was worth
                    score += Actor.FruitByName("fruit").Points

                    ' Reset the fruit eaten state within the actor class
                    Actor.FruitByName("fruit").Eaten = False

                End If

                ' Set the ghost state based on the current gameEngine clock (current tick)
                Actor.SetGhostState(GameEngine.Clock)

            Case GameState.PlayerDied

                ' We are in the playerDied mode.
                ' In this mode, game play stops and an animation of Pac-Man dying is played

                ' If the game engine clock exceeds 70 then the animation has completed...
                If GameEngine.Clock > 70 Then

                    ' Decrement the lives counter
                    lives -= 1

                    ' If the lives are still greater than zero then we simply reset the level,
                    ' otherwise the game is over
                    If lives > 0 Then

                        ' Reset the current level
                        ResetLevel()

                        ' Inform the releaser class that the level has restarted
                        Actor.ghostReleaser.RestartLevel()

                        ' Set the state to "getReady"
                        TransitionTo(GameState.GetReady)

                    Else

                        ' Save the current highscore
                        ' Open the highscore stream file
                        Dim stream As New StreamWriter("HighScore.txt")

                        ' Write the new highscore
                        stream.Write(highScore)

                        ' Close the highscore stream file
                        stream.Close()

                        ' Set the state to "Reset"
                        TransitionTo(GameState.Reset)
                    End If

                End If

        End Select

        If score >= 10000 And last_score < 10000 Then
            sndLifeGained.PlayOnce()
            redrawStatus = True
            lives += 1
        End If

    End Sub

    Public Sub GameEngine_RenderScene() Handles GameEngine.GE_RenderScene

        Dim last_score As Integer

        last_score = score

        ' This is the geRenderScene event.
        ' This is triggered once per frame by the gameEngine.
        ' It is meant to handle rendering type logic.

        ' If the state is not "playerDied"...
        If state <> GameState.PlayerDied Then

            ' Iterate through the ghosts and update their positions
            For n = 0 To ghosts.Count - 1

                ' For each ghost actor...
                With Actor.GhostByIndex(n)

                    GameEngine.SpriteByName(ghosts(n).name).Enabled = True

                    ' Update the ghost sprite based on whether it's scared...
                    ' If the ghost is scared and the ghost has either only just become scared, or is flashing...
                    If .Scared And (.ScaredChanged Or .FlashingChanged) Then

                        ' If the ghost is flashing and only just changed to a flashing state then
                        ' set it's animation range to alternate between white and blue,
                        ' otherwise the ghost is simply blue
                        If .Flashing And .FlashingChanged = True Then
                            GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(8, 11)
                        Else
                            GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(8, 9)
                        End If
                    End If

                    ' Update the ghost sprite based on whether it's not scared...
                    ' If the ghost is not scared and the ghost has either changed direction or is just changing back from scared...
                    If Not .Scared And (.DirectionChanged Or .ScaredChanged) Then

                        ' If the ghost is going home or entering home...
                        If .Mode = Actor.GhostMode.GhostGoingHome Or .Mode = Actor.GhostMode.GhostEnteringHome Then

                            ' Change the animation based on the direction that the ghost is facing
                            Select Case .Direction
                                Case Actor.ActorDirection.Right
                                    GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(12, 12)
                                Case Actor.ActorDirection.Left
                                    GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(13, 13)
                                Case Actor.ActorDirection.Up
                                    GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(14, 14)
                                Case Actor.ActorDirection.Down
                                    GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(15, 15)
                            End Select

                        Else

                            ' If the ghost is not going home and not entering home...

                            ' Change the animation based on the direction that the ghost is facing
                            Select Case .Direction
                                Case Actor.ActorDirection.Right
                                    GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(0, 1)
                                Case Actor.ActorDirection.Left
                                    GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(2, 3)
                                Case Actor.ActorDirection.Up
                                    GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(4, 5)
                                Case Actor.ActorDirection.Down
                                    GameEngine.SpriteByName(ghosts(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(6, 7)
                            End Select

                        End If
                    End If

                    ' If eaten timer has started...
                    If .EatenTimer > 0 Then

                        ' If 3 seconds (180 frames) have passed since the ghost has been eaten then
                        ' update the score.
                        ' 200 for the 1st ghost, 400 for the 2nd, 800 for the 3rd, and 1600 for the 4th
                        ' The simple calculation for this is;
                        ' 2 ^ [Sequence Eaten] * 100

                        If .EatenTimer = (60 * 3) Then
                            sndGhostEaten.PlayOnce()
                            score += (2 ^ (.EatenScore + 1)) * 100
                        End If

                        ' Display the eaten score in the appropriate location
                        GameEngine.SpriteByName(.Name & "score").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(.EatenScore, .EatenScore)
                        GameEngine.SpriteByName(.Name & "score").Point = Point.Add(.EatenPixel, New Point(-7, 16))
                        GameEngine.SpriteByName(.Name & "score").Enabled = True
                    Else

                        ' The eaten timer has ran out so remove the score
                        GameEngine.SpriteByName(.Name & "score").Enabled = False

                    End If

                    ' Reset the direction changed, scared changed and flashing changed flags as we
                    ' have processed them now
                    .DirectionChanged = False
                    .ScaredChanged = False
                    .FlashingChanged = False

                End With

                ' Calculate and set the ghost position within the game surface.
                ' This is done by offsetting the ghost by -7 in the x direction and +16 in the y direction.
                GameEngine.SpriteByName(ghosts(n).name).Point = Point.Add(Actor.GhostByName(ghosts(n).name).Pixel, New Size(-7, 16))

                ' If debugging is turned on then calculate the debug squares position.
                ' This is done by offsetting the debug square by -3 in the x direction and 20 in the y direction.
                If debug = True Then
                    GameEngine.SpriteByName(ghosts(n).name & "debug").Point = Point.Add(Actor.GhostByName(ghosts(n).name).TargetPixel, New Size(-7 + 4, 16 + 4))
                End If
            Next

            ' Iterate through the Pac-Man actors.
            ' Although the game is played with only a single Pac-Man, this logic supports
            ' multiple Pac-Man Sprites. Potentially, the game could be extended to have multiple
            ' players with seperate Pac-Man sprites, all playing simultaneously.

            For n = 0 To pacman.Count - 1

                ' For each Pac-Man actor...
                With Actor.PacmanByIndex(n)

                    ' If the Pac-Man direction has changed...
                    If .DirectionChanged = True Then

                        ' If Pac-Man is no longer moving then stop the Pac-Man animation,
                        ' otherwise the animation mode is geBoth which means the animation plays
                        ' forwards to the last frame and then backwards to the first frame, then repeats.
                        If .Direction = Actor.ActorDirection.None Then
                            GameEngine.SpriteByName(pacman(n).name).AnimateMode = GameEngine.Sprite.GE_AnimationMode.geNone
                        Else
                            GameEngine.SpriteByName(pacman(n).name).AnimateMode = GameEngine.Sprite.GE_AnimationMode.geBoth
                        End If

                        ' Update the animation based on the direction that Pac-Man is facing.
                        Select Case .Direction
                            Case Actor.ActorDirection.Right
                                GameEngine.SpriteByName(pacman(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(0, 2)
                            Case Actor.ActorDirection.Left
                                GameEngine.SpriteByName(pacman(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(3, 5)
                            Case Actor.ActorDirection.Up
                                GameEngine.SpriteByName(pacman(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(6, 8)
                            Case Actor.ActorDirection.Down
                                GameEngine.SpriteByName(pacman(n).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(9, 11)
                        End Select

                        ' Reset the direction changed flag as we have processed it now
                        .DirectionChanged = False

                    End If
                End With

                ' Calculate and set the Pac-Man position within the game surface.
                ' This is done by offsetting Pac-Man by -7 in the x direction and +16 in the y direction.
                GameEngine.SpriteByName(pacman(n).name).Point = Point.Add(Actor.PacmanByName(pacman(n).name).Pixel, New Size(-7, 16))

            Next

            ' Set the fruit animation...
            With Actor.FruitByName("fruit")

                ' If a fruit is active then set the fruit type and enable it...
                If .Active And .Tick > 0 Then
                    GameEngine.SpriteByName("fruit").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(.Number, .Number)
                    GameEngine.SpriteByName("fruit").Enabled = True
                Else

                    ' If a fruit is not active then check the eatenTick to determine whether we
                    ' should be displaying a fruit score or not...
                    If Not .Active And .EatenTick > 0 Then

                        ' If we are displaying a fruit score then show the appropriate tile
                        Select Case .Points
                            Case 100
                                GameEngine.SpriteByName("fruit").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(8, 8)
                            Case 300
                                GameEngine.SpriteByName("fruit").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(9, 9)
                            Case 500
                                GameEngine.SpriteByName("fruit").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(10, 10)
                            Case 700
                                GameEngine.SpriteByName("fruit").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(11, 11)
                            Case 1000
                                GameEngine.SpriteByName("fruit").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(12, 12)
                            Case 2000
                                GameEngine.SpriteByName("fruit").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(13, 13)
                            Case 3000
                                GameEngine.SpriteByName("fruit").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(14, 14)
                            Case 5000
                                GameEngine.SpriteByName("fruit").AnimationRange = New GameEngine.Sprite.GE_AnimationRange(15, 15)
                                GameEngine.SpriteByName("fruit").Enabled = True
                        End Select
                    Else
                        ' If the fruit is not active and we are not showing a fruit score then
                        ' disable the fruit
                        GameEngine.SpriteByName("fruit").Enabled = False
                    End If
                End If

            End With

            ' If the score is greater than the current highscore then update the highscore
            If score > highScore Then
                highScore = score
            End If

        Else

            ' The state is "playerDied"...

            ' If the gameEngine clock is zero...
            If GameEngine.Clock = 0 Then

                ' Play Pac-Man animation forwards
                GameEngine.SpriteByName(pacman(0).name).AnimateMode = GameEngine.Sprite.GE_AnimationMode.geForward

                ' Play the death animation
                GameEngine.SpriteByName(pacman(0).name).AnimationRange = New GameEngine.Sprite.GE_AnimationRange(11, 22)

                ' Turn all the ghosts off
                For n = 0 To ghosts.Count - 1
                    GameEngine.SpriteByName(ghosts(n).name).Enabled = False
                Next
            End If

        End If

        ' The text at the top of the screen is not part of the map, it is rendered seperately.
        ' Render the "1UP" and "HIGHSCORE" text
        GameEngine.DrawTextbyName("font", "1UP", New Point(24, 0))
        GameEngine.DrawTextbyName("font", "HIGH SCORE", New Point(72, 0))

        ' Render the current score
        GameEngine.DrawTextbyName("font", String.Format("{0, 6}", score.ToString("####00")), New Point((1 * TILE_SIZE), (1 * TILE_SIZE)))

        ' Render current highscore
        GameEngine.DrawTextbyName("font", String.Format("{0, 6}", highScore.ToString("####00")), New Point((11 * TILE_SIZE), (1 * TILE_SIZE)))

        ' If the game state is "menu" then we should render the opening credits and instructions
        ' about how to start the game...
        If state = GameState.Menu Then
            GameEngine.DrawTextbyName("font", "MODIFIED BY", New Point((8 * TILE_SIZE) + 4, (24 * TILE_SIZE)))
            GameEngine.DrawTextbyName("font", "PAC DESSERT1436", New Point((6 * TILE_SIZE) + 4, (25 * TILE_SIZE)))
            GameEngine.DrawTextbyName("font", "PRESS SPACE", New Point((8 * TILE_SIZE) + 4, (27 * TILE_SIZE)))
            GameEngine.DrawTextbyName("font", "TO START GAME", New Point((7 * TILE_SIZE) + 4, (28 * TILE_SIZE)))
        End If

        ' If the game state is "getReady" then we should render the "GET READY" text
        If state = GameState.GetReady Then
            GameEngine.DrawTextbyName("font", "GET READY", New Point((9 * TILE_SIZE) + 4, (20 * TILE_SIZE)))
        End If

        ' If debugging is enabled then we should show the current FPS and game engine clock,
        ' otherwise just display the current FPS
        If debug = True Then
            statusText.Text = "FPS: " & Format(GameEngine.Fps, "000.0") & " CLOCK: " & Format(GameEngine.Clock / 60, "000000") & "." & Format(GameEngine.Clock Mod 60, "00") & " DOTS: " & Format(maze.DotEaten, "000") & " / " & Format(maze.DotCount, "000")
        Else
            statusText.Text = "FPS: " & Format(GameEngine.Fps, "000.0")
        End If

        If score >= 10000 And last_score < 10000 Then
            redrawStatus = True
            lives += 1
        End If

        If redrawStatus = True Then
            UpdateStatus()
            redrawStatus = False
        End If

    End Sub

    Function InitializeGame()

        ' Set Form to use double buffering
        ' This prevents flickering as it uses two surfaces and flicks between each
        Me.DoubleBuffered = True

        ' Form is not maximized
        Me.MaximizeBox = False

        ' Set border style to fixed
        Me.FormBorderStyle = FormBorderStyle.FixedSingle

        ' Set the client width and height
        Me.ClientSize = New Size((CLIENT_WIDTH) * CLIENT_SCALE, CLIENT_HEIGHT * CLIENT_SCALE + MenuStrip2.Height + StatusStrip1.Height)

        ' Center Form to screen
        Me.CenterToScreen()

        ' We keep the ghost starting states in an array as this allows us to reset them quicker.
        ' Initialize those arrays...
        With ghosts(0)
            .name = "blinky"
            .cornerTile = New Point(25, -3)
            .startPixel = New Point((14 * TILE_SIZE) - 1, (11 * TILE_SIZE) + 4)
            .startDirection = Actor.ActorDirection.Left
            .startMode = Actor.GhostMode.GhostOutside
            .arriveHomeMode = Actor.GhostMode.GhostLeavingHome
        End With

        With ghosts(1)
            .name = "pinky"
            .cornerTile = New Point(2, -3)
            .startPixel = New Point((14 * TILE_SIZE) - 1, (14 * TILE_SIZE) + 4)
            .startDirection = Actor.ActorDirection.Down
            .startMode = Actor.GhostMode.GhostPacingHome
            .arriveHomeMode = Actor.GhostMode.GhostPacingHome
        End With

        With ghosts(2)
            .name = "inky"
            .cornerTile = New Point(27, 31)
            .startPixel = New Point((12 * TILE_SIZE) - 1, (14 * TILE_SIZE) + 4)
            .startDirection = Actor.ActorDirection.Up
            .startMode = Actor.GhostMode.GhostPacingHome
            .arriveHomeMode = Actor.GhostMode.GhostPacingHome
        End With

        With ghosts(3)
            .name = "clyde"
            .cornerTile = New Point(0, 31)
            .startPixel = New Point((16 * TILE_SIZE) - 1, (14 * TILE_SIZE) + 4)
            .startDirection = Actor.ActorDirection.Up
            .startMode = Actor.GhostMode.GhostPacingHome
            .arriveHomeMode = Actor.GhostMode.GhostPacingHome
        End With

        ' We keep pacmans starting state in an array
        ' Initialize those arrays...
        With pacman(0)
            .name = "pacman"
            .startPixel = New Point((14 * TILE_SIZE), (23 * TILE_SIZE) + 4)
            .startDirection = Actor.ActorDirection.Left
        End With

        ' --------------------------------------------------------------------------------------------------
        ' Create Actors
        ' --------------------------------------------------------------------------------------------------

        ' Create a ghost actor with starting positions from the ghost arrays
        For n = 0 To ghosts.Count - 1
            Actor.AddGhost(ghosts(n).name, ghosts(n).startPixel, ghosts(n).cornerTile, ghosts(n).startDirection, ghosts(n).startMode, ghosts(n).arriveHomeMode)
        Next

        ' Create a pacman actor with starting positions from the pacman arrays
        For n = 0 To pacman.Count - 1
            Actor.AddPacman(pacman(n).name, pacman(n).startPixel, pacman(n).startDirection)
        Next

        ' Create a fruit actor
        Actor.AddFruit("fruit", New Point((13 * TILE_SIZE), (17 * TILE_SIZE)))

        ' --------------------------------------------------------------------------------------------------
        ' Initialize GameEngine
        ' --------------------------------------------------------------------------------------------------

        ' Create a new gameEngine instance, pointing it to our main form
        ' The gameEngine initializes a surface to render to, so we need to define its size
        GameEngine = New GameEngine(Me, CLIENT_WIDTH, CLIENT_HEIGHT, CLIENT_SCALE, MenuStrip2.Height)

        ' Add a new tile set that we will use to render the maze
        GameEngine.AddTile("defaultMaze", New Size(TILE_SIZE, TILE_SIZE))

        ' Add a new tile set that we will use to display the status information
        GameEngine.AddTile("status", New Size(16, 16))

        ' Add a new map called "main" and initialize it to 32 x 40 tiles, and set it to use our "maze" tile map
        GameEngine.AddMap("main", GameEngine.TileIndexByName("defaultMaze"), New Size(32, 34))

        ' Add a new map called "status" and initialize it to 16 x 1 tiles, and set it to use our "status" tile map
        GameEngine.AddMap("status", GameEngine.TileIndexByName("status"), New Size(16, 1))

        ' Initialize a blank status area (map "status") and enable this map
        GameEngine.MapByName("status").Point = New Point(0, (34 * TILE_SIZE))
        For n = 0 To 15
            GameEngine.MapByName("status").Value(New Point(n, 0)) = 0
        Next
        GameEngine.MapByName("status").Enabled = True

        ' Add the ghost sprites to gameEngine
        For n = 0 To ghosts.Count - 1
            With ghosts(n)

                ' Add a new ghost sprite and initialize it
                GameEngine.AddSprite(.name, New Size(16, 16))
                With GameEngine.SpriteByName(.name)
                    .Point = Actor.GhostByName(.Name).Pixel
                    .AnimateMode = GameEngine.Sprite.GE_AnimationMode.geForward
                    .AnimateOnFrame = 7
                    .AnimationRange = New GameEngine.Sprite.GE_AnimationRange(4, 5)
                    .Enabled = True
                End With

                ' Add a ghost debug sprite and initialize it
                GameEngine.AddSprite(.name & "debug", New Size(16, 16))
                If debug = True Then
                    GameEngine.SpriteByName(.name & "debug").Enabled = True
                Else
                    GameEngine.SpriteByName(.name & "debug").Enabled = False
                End If

                ' Add a ghost score sprite and initialize it
                GameEngine.AddSprite(.name & "score", New Size(16, 16))
                GameEngine.SpriteByName(.name & "score").Point = New Point(50 + (n * 20), 50)
                GameEngine.SpriteByName(.name & "score").Enabled = True

            End With
        Next

        ' Add pacman sprites to gameEngine
        For n = 0 To pacman.Count - 1
            With pacman(n)
                GameEngine.AddSprite(.name, New Size(16, 16))
                With GameEngine.SpriteByName(.name)
                    .Point = Actor.PacmanByName(.Name).Pixel
                    .AnimateMode = GameEngine.Sprite.GE_AnimationMode.geNone
                    .AnimateOnFrame = 5
                    .AnimationRange = New GameEngine.Sprite.GE_AnimationRange(3, 5)
                    .Enabled = True
                End With
            End With
        Next

        ' Add pacman logo sprite to the gameEngine
        GameEngine.AddSprite("pacmanlogo2", New Size(132, 42))
        With GameEngine.SpriteByName("pacmanlogo2")
            .Point = New Point(46, 50)
            .AnimateMode = GameEngine.Sprite.GE_AnimationMode.geNone
            .Enabled = False
            .ZIndex = 2
        End With

        ' Add pacman border sprite to the gameEngine
        GameEngine.AddSprite("pacmanborder2", New Size(132, 66))
        With GameEngine.SpriteByName("pacmanborder2")
            .Point = New Point(46, 180)
            .AnimateMode = GameEngine.Sprite.GE_AnimationMode.geNone
            .Enabled = False
            .ZIndex = 2
        End With

        ' Add fruit sprite to gameEngine
        GameEngine.AddSprite("fruit", New Size(16, 16))
        With GameEngine.SpriteByName("fruit")
            .Point = New Point((13 * TILE_SIZE), (19 * TILE_SIZE) + 4)
            .AnimationRange = New GameEngine.Sprite.GE_AnimationRange(0, 0)
            .AnimateMode = GameEngine.Sprite.GE_AnimationMode.geNone
            .Enabled = False
            .ZIndex = 2
        End With

        ' Add fruit score sprite to gameEngine
        GameEngine.AddSprite("fruitscore", New Size(16, 16))

        ' Add a new font to the gameEngine
        GameEngine.AddFont("font", New Size(TILE_SIZE, TILE_SIZE), 48)

        ' Load the the default maze
        maze.LoadMaze("PacmanMaze.pac")

        ' Reset the game
        ResetGame()

        ' Load the highscore
        Try
            Dim stream As New StreamReader("HighScore.txt")
            highScore = stream.ReadLine()
            stream.Close()
        Catch
            highScore = 0
        End Try

        ' Set the gameEngine FPS, reset the clock and (finally, phew!) start the gameEngine process
        GameEngine.Fps = FPS
        GameEngine.Clock = 0

        GameEngine.StartEngine()

        Return True

    End Function

    Public Sub ResetGame()

        ' This function resets the entire game.
        ' It us usually called when a new game is started

        ' Reset level, score and lives
        level = 1
        score = 0
        lives = 3

        ' Reset ghost actors
        Actor.ResetGhost()

        ' Reset pacman actor
        Actor.ResetPacman()

        ' Reset ghost releaser
        Actor.ghostReleaser.NewLevel(level)

        ' Reset fruit
        Actor.ResetFruit()

        ' Reset the maze
        maze.ResetMaze()

        ' Copy the maze to the gameEngine class.
        ' We do this because the game engine keeps all of it's resources internally
        ' because it runs in a seperate thread. Accessing variables outside of this
        ' thread is a big no-no as it can cause protection faults, so we need to 
        ' provide a copy.
        CopyGameEngineMaze()

        ' Update the status bar (fruits)
        UpdateStatus()

        ' Reset the gameEngine tick
        GameEngine.Clock = 0

    End Sub

    Public Sub ResetLevel()

        ' This function resets the level.
        ' It is usually called when Pac-Man dies but has more lives.

        ' Reset ghost actors
        Actor.ResetGhost()

        ' Reset pacman actor
        Actor.ResetPacman()

        ' Reset ghost releaser
        Actor.ghostReleaser.NewLevel(level)

        ' Update the status bar (fruits)
        UpdateStatus()

        ' Reset the gameEngine tick
        GameEngine.Clock = 0

    End Sub

    Public Sub NextLevel()

        ' This function moves to the next level.
        ' It is usually called when Pac-Man has eaten all of the dots on a level.

        ' Reset ghost actors
        Actor.ResetGhost()

        ' Reset pacman actor
        Actor.ResetPacman()

        ' Reset ghost releaser
        Actor.ghostReleaser.NewLevel(level)

        ' Reset fruit
        Actor.ResetFruit()

        ' Reset the maze
        maze.ResetMaze()

        ' Copy the maze to the gameEngine class
        CopyGameEngineMaze()

        ' Update the status bar
        UpdateStatus()

        ' Reset the gameEngine tick
        GameEngine.Clock = 0

    End Sub

    Public Sub UpdateStatus()

        ' This function updates the status bar at the bottom of the gameEngine screen
        ' This displays the number of lives (represented by pacman sprites), and the current
        ' level (represented by the last 7 fruit bonuses)

        Dim FruitChar

        ' Process the list of last fruit bonuses. The list is passed as a string of numbers, where
        ' each number represents the fruit that needs rendering. A blank space indicates a blank tile.
        For l = 0 To 6

            ' Get a character from the fruit list
            FruitChar = Mid(Actor.FruitByName("fruit").List, l + 1, 1)

            ' If the fruit character is not blank then render the fruit
            If FruitChar <> " " Then
                GameEngine.MapByName("status").Value(New Point(l + 6, 0)) = Int(FruitChar + 1)
            Else
                GameEngine.MapByName("status").Value(New Point(l + 6, 0)) = 0
            End If

        Next

        ' Display up to four lives. Each life is represented by a Pac-Man sprite.
        ' If the player has more than four lives then only 4 are rendered.
        For l = 1 To 4
            If lives >= l Then
                GameEngine.MapByName("status").Value(New Point(l, 0)) = 9
            Else
                GameEngine.MapByName("status").Value(New Point(l, 0)) = 0
            End If
        Next

    End Sub

    Public Sub CopyGameEngineMaze()

        ' Initialize the first three lines of the gameEngine map with blank tiles.
        ' This is the area that displays the score and high score.
        For y = 0 To 2
            For x = 0 To 30
                GameEngine.MapByName("main").Value(New Point(x, y)) = Maze.MazeObjects.blank
            Next
        Next

        ' Copy the actual maze data into the gameEngine map.
        For y = 3 To 33
            For x = 0 To 30
                GameEngine.MapByName("main").Value(New Point(x, y)) = maze.Data(New Point(x, y - 3))
            Next
        Next

    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        ' End the gameEngine since the main form is closing
        GameEngine.EndEngine()

    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown

        ' Based on the keyCode pressed, we can pass data into the input class...
        Select Case e.KeyCode

            Case Keys.A, Keys.Left
                input.left = True
                input.right = False
            Case Keys.D, Keys.Right
                input.right = True
                input.left = False
            Case Keys.W, Keys.Up
                input.up = True
                input.down = False
            Case Keys.S, Keys.Down
                input.down = True
                input.up = False
            Case Keys.Space
                input.space = True
        End Select

    End Sub

    Private Sub Form1_KeyUp(sender As Object, e As KeyEventArgs) Handles MyBase.KeyUp

        ' Based on the keyCode released, we can pass data into the input class...
        Select Case e.KeyCode

            Case Keys.A, Keys.Left
                input.left = False
            Case Keys.D, Keys.Right
                input.right = False
            Case Keys.W, Keys.Up
                input.up = False
            Case Keys.S, Keys.Down
                input.down = False
            Case Keys.Space
                input.space = False

        End Select

    End Sub

    Private Sub ToolStrip_FPS5_Click(sender As Object, e As EventArgs) Handles ToolStrip_FPS5.Click

        ' This function is called when the FPS is set to 5 from the tool strip menu.

        ' End the game engine
        GameEngine.EndEngine()

        ' Set the FPS to 5
        GameEngine.Fps = 5

        ' Restart the game engine
        GameEngine.StartEngine()

        ToolStrip_FPS5.Checked = True
        ToolStrip_FPS50.Checked = False
        ToolStrip_FPS60.Checked = False
        ToolStrip_FPS200.Checked = False
        ToolStrip_FPS100.Checked = False

    End Sub

    Private Sub ToolStrip_FPS50_Click(sender As Object, e As EventArgs) Handles ToolStrip_FPS50.Click

        ' This function is called when the FPS is set to 200 from the tool strip menu.

        GameEngine.EndEngine()
        GameEngine.Fps = 50
        GameEngine.StartEngine()

        ToolStrip_FPS5.Checked = False
        ToolStrip_FPS50.Checked = True
        ToolStrip_FPS60.Checked = False
        ToolStrip_FPS200.Checked = False
        ToolStrip_FPS100.Checked = False

    End Sub

    Private Sub ToolStrip_FPS60_Click(sender As Object, e As EventArgs) Handles ToolStrip_FPS60.Click

        ' This function is called when the FPS is set to 60 from the tool strip menu.
        GameEngine.EndEngine()
        GameEngine.Fps = 60
        GameEngine.StartEngine()

        ToolStrip_FPS5.Checked = False
        ToolStrip_FPS50.Checked = False
        ToolStrip_FPS60.Checked = True
        ToolStrip_FPS200.Checked = False
        ToolStrip_FPS100.Checked = False

    End Sub

    Private Sub ToolStrip_FPS100_Click(sender As Object, e As EventArgs) Handles ToolStrip_FPS100.Click

        ' This function is called when the FPS is set to 100 from the tool strip menu.

        GameEngine.EndEngine()
        GameEngine.Fps = 100
        GameEngine.StartEngine()

        ToolStrip_FPS5.Checked = False
        ToolStrip_FPS50.Checked = False
        ToolStrip_FPS60.Checked = False
        ToolStrip_FPS100.Checked = True
        ToolStrip_FPS200.Checked = False

    End Sub

    Private Sub ToolStrip_FPS200_Click(sender As Object, e As EventArgs) Handles ToolStrip_FPS200.Click

        ' This function is called when the FPS is set to 200 from the tool strip menu.

        GameEngine.EndEngine()
        GameEngine.Fps = 200
        GameEngine.StartEngine()

        ToolStrip_FPS5.Checked = False
        ToolStrip_FPS50.Checked = False
        ToolStrip_FPS60.Checked = False
        ToolStrip_FPS100.Checked = False
        ToolStrip_FPS200.Checked = True

    End Sub

    Private Sub ToolStrip_Debug_Click(sender As Object, e As EventArgs) Handles ToolStrip_Debug.Click

        ' This function is called when the debugging is enable or disabled in the tool strip menu.

        If ToolStrip_Debug.Checked = True Then
            debug = False
            ToolStrip_Debug.Checked = False
            For n = 0 To ghosts.Count - 1
                GameEngine.SpriteByName(ghosts(n).name & "debug").Enabled = False
            Next
        Else
            debug = True
            ToolStrip_Debug.Checked = True
            For n = 0 To 3
                GameEngine.SpriteByName(ghosts(n).name & "debug").Enabled = True
            Next
        End If

    End Sub

    Private Sub ToolStrip_Instructions_Click(sender As Object, e As EventArgs) Handles ToolStrip_Instructions.Click

        GameEngine.EndEngine()
        Program.Instructions.ShowDialog()
        GameEngine.StartEngine()

    End Sub

    Private Sub ToolStrip_LoadMaze_Click(sender As Object, e As EventArgs) Handles ToolStrip_LoadMaze.Click

        Dim fd As FileDialog
        Dim fileName As String
        Dim ans As MsgBoxResult

        GameEngine.EndEngine()

        fd = New OpenFileDialog With {
            .Title = "Load Pacman Maze",
            .Filter = "Pacman Mazes (*.pac)|*.pac",
            .FilterIndex = 1,
            .RestoreDirectory = True
        }

        If state = GameState.GameStarted Or state = GameState.GetReady Or state = GameState.PlayerDied Then
            ans = MsgBox("Loading a maze will end the current game." & Chr(13) & "Are you sure that you want to continue?", MsgBoxStyle.YesNo, "Load Maze")
        Else
            ans = vbYes
        End If

        If ans = vbYes Then

            If fd.ShowDialog() = DialogResult.OK Then

                fileName = fd.FileName

                maze.LoadMaze(fileName)

                state = GameState.Reset

            End If

        End If

        GameEngine.StartEngine()

    End Sub

    Private Sub ToolStrip_MapEditor_Click(sender As Object, e As EventArgs) Handles ToolStrip_MapEditor.Click

        Program.MapEditor.Show()

    End Sub

    Private Sub ToolStrip_NewGame_Click(sender As Object, e As EventArgs) Handles ToolStrip_NewGame.Click

        GameEngine.EndEngine()
        If MsgBox("Are you sure that you want to start a new game?", MsgBoxStyle.YesNo, "New Game") = MsgBoxResult.Yes Then
            state = GameState.Reset
        End If
        GameEngine.StartEngine()

    End Sub

    Private Sub ToolStrip_ResetHighscore_Click(sender As Object, e As EventArgs) Handles ToolStrip_ResetHighscore.Click

        If MsgBox("Are you sure that you want to reset the highscore?", MsgBoxStyle.YesNo, "Reset Highscore") = MsgBoxResult.Yes Then

            ' Save the current highscore
            ' Open the highscore stream file
            Dim stream As New StreamWriter("HighScore.txt")

            ' Write the new highscore
            stream.Write(0)

            ' Close the highscore stream file
            stream.Close()

            ' Reset highscore
            highScore = 0

        End If

    End Sub

    Private Sub ToolStrip_EnableInvincibility_Click(sender As Object, e As EventArgs) Handles ToolStrip_EnableInvincibility.Click

        If ToolStrip_EnableInvincibility.Checked = False Then
            ToolStrip_EnableInvincibility.Checked = True
            invincibile = True
        Else
            ToolStrip_EnableInvincibility.Checked = False
            invincibile = False
        End If

    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click

        Me.Close()

    End Sub
End Class