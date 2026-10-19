Imports System.Drawing
Imports GTA
Imports GTA.Math

Public Class Language
    Public Property name As String
    Public Property data As New List(Of LanguageData)

    Public Shared Function Defaults() As List(Of Language)
        Return New List(Of Language) From {
            New Language With {
            .name = "american",
            .data = New List(Of LanguageData) From {
                New LanguageData("loaded", "{0} vehicles loaded."),
                New LanguageData("lock", "Press {0} to lock {1}."),
                New LanguageData("unlock", "Press {0} to unlock {1}.")
            }
        }}
    End Function
End Class

Public Class LanguageData
    Public Property name As String
    Public Property value As String

    Public Sub New()

    End Sub

    Public Sub New(name As String, value As String)
        Me.name = name
        Me.value = value
    End Sub

End Class

Public Class VehicleData

    ' Basic Info
    Public Property brand As String
    Public Property name As String
    Public Property [class] As String
    Public Property hash As Integer
    Public Property position As Vector3
    Public Property rotation As Vector3
    Public Property owner As Integer

    ' Mods
    Public Property mods As List(Of inm_VehicleMod)
    Public Property toggleMods As List(Of inm_VehicleToggleMod)
    Public Property extras As List(Of inm_VehicleExtra)
    Public Property wheelsVariation As Boolean

    ' Colors
    Public Property windowTint As VehicleWindowTint
    Public Property primaryColor As VehicleColor
    Public Property secondaryColor As VehicleColor
    Public Property pearlescentColor As VehicleColor
    Public Property rimColor As VehicleColor
    Public Property dashboardColor As VehicleColor
    Public Property trimColor As VehicleColor
    Public Property neonLightsColor As Color
    Public Property tireSmokeColor As Color
    Public Property customPrimaryColor As Color
    Public Property customSecondaryColor As Color
    Public Property isPrimaryColorCustom As Boolean
    Public Property isSecondaryColorCustom As Boolean
    Public Property XenonHeadlightsColor As Integer
    Public Property livery As Integer
    Public Property livery2 As Integer

    ' Other mods
    Public Property canTireBurst As Boolean
    Public Property driftTires As Boolean

    ' More stuffs
    Public Property dirtLevel As Single
    Public Property roofState As VehicleRoofState
    Public Property steeringScale As Single
    Public Property hasTrailer As Boolean
    Public Property hasTowing As Boolean

    ' Addon stuffs
    Public Property hasNitro As Boolean

    Public Sub New()

    End Sub

    Public Sub New(veh As Vehicle, owner As Integer)
        brand = Game.GetLocalizedString(Vehicle.GetModelMakeName(veh.Model))
        name = veh.LocalizedName
        [class] = veh.ClassLocalizedName
        hash = veh.Model.Hash
        position = veh.Position
        rotation = veh.Rotation
        Me.owner = owner

        'Dim modList As New List(Of inm_VehicleMod)
        'For i As VehicleModType = VehicleModType.None To VehicleModType.Livery
        '    Dim vmod = veh.Mods(i)
        '    Dim vehMod As New inm_VehicleMod() With {
        '        .modType = i,
        '        .typeName = vmod.LocalizedTypeName,
        '        .index = vmod.Index,
        '        .variation = vmod.Variation,
        '        .name = vmod.LocalizedName
        '    }
        '    modList.Add(vehMod)
        'Next
        'mods = modList

        mods = veh.Mods.ToArray().Select(Function(x) New inm_VehicleMod() With {.modType = x.Type, .typeName = x.LocalizedTypeName, .index = x.Index, .variation = x.Variation, .name = x.LocalizedName}).ToList()

        Dim tmods As New List(Of inm_VehicleToggleMod)
        For i As VehicleToggleModType = VehicleToggleModType.Nitrous To VehicleToggleModType.XenonHeadlights
            'Dim vmod = veh.
            ' TODO
        Next

    End Sub

End Class

Public Class inm_VehicleMod

    Public Property modType As VehicleModType
    Public Property typeName As String
    Public Property index As Integer
    Public Property variation As Boolean
    Public Property name As String

End Class

Public Class inm_VehicleToggleMod

    Public Property modType As VehicleToggleModType
    Public Property isInstalled As Boolean

End Class

Public Class inm_VehicleExtra

    Public Property extraIndex As VehicleExtraIndex
    Public Property isInstalled As Boolean

End Class