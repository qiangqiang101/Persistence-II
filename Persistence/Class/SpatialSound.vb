Imports System.IO
Imports System.Media
Imports GTA
Imports GTA.Math

'Plays a 16-bit PCM wav as if it came from a point in the world: it gets quieter with distance
'and is panned left/right by where the point is relative to the gameplay camera.
Public Module SpatialSound

    'Win32 can only play one SoundPlayer sound at a time; keep a reference so it isn't collected mid-play.
    Private lastPlayer As SoundPlayer

    Public Sub PlayWaveAt(waveFile As String, position As Vector3, Optional volume As Integer = 50, Optional maxDistance As Single = 30.0F, Optional minDistance As Single = 2.0F)
        Try
            Dim offset As Vector3 = position - GameplayCamera.Position
            Dim distance As Single = offset.Length()
            Dim gain As Single = DistanceGain(distance, minDistance, maxDistance) * System.Math.Max(0, System.Math.Min(100, volume)) / 100.0F
            If gain <= 0.0F Then Return

            'Pan: -1 = fully left, 1 = fully right. Scaled down a little so the far ear still hears it.
            Dim pan As Single = 0.0F
            If distance > 0.01F Then pan = Vector3.Dot(offset / distance, GameplayCamera.RightVector) * 0.8F

            Dim wav As Byte() = Spatialize(File.ReadAllBytes(waveFile), gain, pan)
            lastPlayer = New SoundPlayer(New MemoryStream(wav))
            lastPlayer.Play()
        Catch ex As Exception
            Logger.Log(ex)
        End Try
    End Sub

    'Full volume inside minDistance, fading to silent at maxDistance.
    Private Function DistanceGain(distance As Single, minDistance As Single, maxDistance As Single) As Single
        If distance <= minDistance Then Return 1.0F
        If distance >= maxDistance Then Return 0.0F
        Dim t As Single = (maxDistance - distance) / (maxDistance - minDistance)
        Return t * t
    End Function

    'Rewrites a PCM (8/16/24/32-bit) or 32-bit float wav as 16-bit stereo with the given gain and pan applied.
    'Files with more than two channels use their first two.
    Private Function Spatialize(source As Byte(), gain As Single, pan As Single) As Byte()
        If source.Length < 12 OrElse Text.Encoding.ASCII.GetString(source, 0, 4) <> "RIFF" OrElse Text.Encoding.ASCII.GetString(source, 8, 4) <> "WAVE" Then
            Throw New InvalidDataException("Not a wav file.")
        End If

        Dim format As Integer = 0, channels As Integer = 0, sampleRate As Integer = 0, bits As Integer = 0
        Dim dataOffset As Integer = -1, dataLength As Integer = 0
        Dim pos As Integer = 12
        While pos + 8 <= source.Length
            Dim id As String = Text.Encoding.ASCII.GetString(source, pos, 4)
            Dim size As Integer = BitConverter.ToInt32(source, pos + 4)
            If id = "fmt " Then
                format = BitConverter.ToUInt16(source, pos + 8)
                channels = BitConverter.ToInt16(source, pos + 10)
                sampleRate = BitConverter.ToInt32(source, pos + 12)
                bits = BitConverter.ToInt16(source, pos + 22)
                'WAVE_FORMAT_EXTENSIBLE keeps the real format in the first two bytes of its sub-format GUID.
                If format = &HFFFE AndAlso size >= 26 Then format = BitConverter.ToUInt16(source, pos + 8 + 24)
            ElseIf id = "data" Then
                dataOffset = pos + 8
                dataLength = System.Math.Min(size, source.Length - dataOffset)
                Exit While
            End If
            pos += 8 + size + (size And 1)
        End While
        Dim supported As Boolean = (format = 1 AndAlso (bits = 8 OrElse bits = 16 OrElse bits = 24 OrElse bits = 32)) OrElse (format = 3 AndAlso bits = 32)
        If Not supported OrElse channels < 1 OrElse dataOffset < 0 Then
            Throw New InvalidDataException($"Unsupported wav format (format {format}, {bits}-bit, {channels} channels). Save it as 16-bit PCM.")
        End If
        Dim bytesPerSample As Integer = bits \ 8

        'Constant-power pan, normalised so the centre stays at full volume.
        Dim angle As Double = (pan + 1.0) * System.Math.PI / 4.0
        Dim leftGain As Single = CSng(System.Math.Min(1.0, System.Math.Cos(angle) * System.Math.Sqrt(2.0))) * gain
        Dim rightGain As Single = CSng(System.Math.Min(1.0, System.Math.Sin(angle) * System.Math.Sqrt(2.0))) * gain

        Dim frames As Integer = dataLength \ (bytesPerSample * channels)
        Dim outData As Integer = frames * 4
        Using ms As New MemoryStream(44 + outData), w As New BinaryWriter(ms)
            w.Write(Text.Encoding.ASCII.GetBytes("RIFF")) : w.Write(36 + outData) : w.Write(Text.Encoding.ASCII.GetBytes("WAVE"))
            w.Write(Text.Encoding.ASCII.GetBytes("fmt ")) : w.Write(16) : w.Write(CShort(1)) : w.Write(CShort(2))
            w.Write(sampleRate) : w.Write(sampleRate * 4) : w.Write(CShort(4)) : w.Write(CShort(16))
            w.Write(Text.Encoding.ASCII.GetBytes("data")) : w.Write(outData)
            For f As Integer = 0 To frames - 1
                Dim i As Integer = dataOffset + f * bytesPerSample * channels
                Dim l As Single = ReadSample(source, i, format, bits)
                Dim r As Single = If(channels >= 2, ReadSample(source, i + bytesPerSample, format, bits), l)
                w.Write(Clip(l * leftGain))
                w.Write(Clip(r * rightGain))
            Next
            Return ms.ToArray()
        End Using
    End Function

    'Returns one sample scaled to the 16-bit range.
    Private Function ReadSample(data As Byte(), i As Integer, format As Integer, bits As Integer) As Single
        If format = 3 Then Return BitConverter.ToSingle(data, i) * 32767.0F
        Select Case bits
            Case 8 : Return (CInt(data(i)) - 128) * 256.0F
            Case 16 : Return BitConverter.ToInt16(data, i)
            Case 24 : Return ((CInt(data(i + 2)) << 24) Or (CInt(data(i + 1)) << 16) Or (CInt(data(i)) << 8)) / 65536.0F
            Case Else : Return BitConverter.ToInt32(data, i) / 65536.0F
        End Select
    End Function

    Private Function Clip(value As Single) As Short
        Return CShort(System.Math.Max(Short.MinValue, System.Math.Min(Short.MaxValue, System.Math.Round(value))))
    End Function

End Module
