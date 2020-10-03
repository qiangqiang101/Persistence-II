Imports System.IO
Imports System.Threading

Module Module1

    Public xmlPath As String = ".\Vehicles\"
    Public gotError As Boolean = False

    Sub Main()
        Console.WriteLine("Searching for xml files...")
        Console.WriteLine("")
        Thread.Sleep(1000)

        If Directory.Exists(xmlPath) Then
            Try
                For Each xml As String In Directory.GetFiles(xmlPath, "*.xml")
                    My.Computer.FileSystem.WriteAllText(xml, My.Computer.FileSystem.ReadAllText(xml).Replace("<HasNitro>false</HasNitro>", "<HasNitro>0</HasNitro>"), False)
                    My.Computer.FileSystem.WriteAllText(xml, My.Computer.FileSystem.ReadAllText(xml).Replace("<HasNitro>true</HasNitro>", "<HasNitro>3</HasNitro>"), False)
                    Console.WriteLine($"Patching {xml}...")
                Next
            Catch ex As Exception
                gotError = True
                Console.WriteLine("Game is running, process terminated.")
                Console.WriteLine("Press any key to exit...")
                Console.ReadKey()
            Finally
                If Not gotError Then
                    Console.WriteLine("")
                    Console.WriteLine("Patch completed. You can remove this file after closing.")
                    Console.WriteLine("Press any key to exit...")
                    Console.ReadKey()
                End If
            End Try
        Else
            Console.WriteLine($"Could not locate {xmlPath}.")
            Console.WriteLine("Press any key to exit...")
            Console.ReadKey()
        End If


    End Sub

End Module
