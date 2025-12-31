Option Strict On
Option Infer On
Imports NAudio.Wave

Public Class AudioPlayer
    Private ReadOnly reader As AudioFileReader
    Private ReadOnly waveOut As WaveOutEvent
    Private isLooping As Boolean = False
    Private ReadOnly _lock As New Object

    Public Sub New(filename As String)
        reader = New AudioFileReader(filename)
        waveOut = New WaveOutEvent
        waveOut.Init(reader)

        AddHandler waveOut.PlaybackStopped, AddressOf OnPlaybackStopped
    End Sub

    Public Sub PlayOnce()
        SyncLock _lock
            If waveOut IsNot Nothing Then
                isLooping = False
                reader.Position = 0
                waveOut.Play()
            End If
        End SyncLock
    End Sub

    Public Sub PlayLooping()
        SyncLock _lock
            If waveOut IsNot Nothing Then
                isLooping = True
                reader.Position = 0
                waveOut.Play()
            End If
        End SyncLock
    End Sub

    Public Sub [Stop]()
        SyncLock _lock
            If waveOut IsNot Nothing Then
                isLooping = False
                waveOut.Stop()
            End If
        End SyncLock
    End Sub

    Public Sub OnPlaybackStopped(sender As Object, e As StoppedEventArgs)
        SyncLock _lock
            If isLooping AndAlso waveOut IsNot Nothing Then
                reader.Position = 0
                waveOut.Play()
            End If
        End SyncLock
    End Sub
End Class