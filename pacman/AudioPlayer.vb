Option Strict On
Option Infer On
Imports NAudio.Wave

Public Class AudioPlayer
    Implements IDisposable

    Private ReadOnly reader As AudioFileReader
    Private ReadOnly waveOut As WaveOutEvent
    Private isLooping As Boolean = False
    Private disposedValue As Boolean
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

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                If waveOut IsNot Nothing Then
                    RemoveHandler waveOut.PlaybackStopped, AddressOf OnPlaybackStopped
                End If
                waveOut?.Dispose()
                reader?.Dispose()
            End If

            disposedValue = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub
End Class