Imports System.Runtime.CompilerServices
Imports GTA
Imports GTA.Native

'Vehicle natives SHVDN v3 doesn't wrap. These are built in already, so use them instead:
'HasRocketBoost, IsRocketBoostActive, CanJump, HasParachute, IsParachuteDeployed,
'SpecialFlightModeTargetRatio, SpecialFlightModeCurrentRatio, SpecialFlightModeWingRatio,
'Model.IsAmphibiousCar, Model.IsAmphibiousQuadBike. Xenon colour is XenonHeadlightsColor in Helper.vb.
Module NewFunc

    'Retractable Wheels
    <Extension>
    Public Function HasRetractableWheels(vehicle As Vehicle) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.GET_HAS_RETRACTABLE_WHEELS, vehicle)
    End Function

    <Extension>
    Public Sub RaiseRetractableWheels(vehicle As Vehicle)
        Native.Function.Call(Hash.SET_WHEELS_EXTENDED_INSTANTLY, vehicle)
    End Sub

    <Extension>
    Public Sub LowerRetractableWheels(vehicle As Vehicle)
        Native.Function.Call(Hash.SET_WHEELS_RETRACTED_INSTANTLY, vehicle)
    End Sub

    'Rocket Boost
    <Extension>
    Public Sub SetRocketBoostRefillTime(vehicle As Vehicle, seconds As Single)
        Native.Function.Call(Hash.SET_SCRIPT_ROCKET_BOOST_RECHARGE_TIME, vehicle, seconds)
    End Sub

    <Extension>
    Public Sub SetRocketBoostPercentage(vehicle As Vehicle, percentage As Single)
        Native.Function.Call(Hash.SET_ROCKET_BOOST_FILL, vehicle, percentage)
    End Sub

    'Nitro Boost
    <Extension>
    Public Sub SetBoostActiveSound(vehicle As Vehicle, toggle As Boolean)
        Native.Function.Call(Hash.SET_VEHICLE_BOOST_ACTIVE, vehicle, toggle)
    End Sub

    <Extension>
    Public Function IsVehicleShuntBoostActive(vehicle As Vehicle) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.GET_IS_VEHICLE_SHUNTING, vehicle)
    End Function

    <Extension>
    Public Sub SetNitroEnabled(vehicle As Vehicle, toggle As Boolean, Optional level As Single = 2.5F, Optional power As Single = 1.1F, Optional rechargeTime As Single = 4.0F, Optional disableSound As Boolean = False)
        If Not Native.Function.Call(Of Boolean)(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "veh_xs_vehicle_mods") Then
            Native.Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "veh_xs_vehicle_mods")
        End If
        Native.Function.Call(Hash.SET_OVERRIDE_NITROUS_LEVEL, vehicle, toggle, level, power, rechargeTime, disableSound)
    End Sub

    Public Sub SetNitroHudActive(toggle As Boolean)
        Native.Function.Call(Hash.SET_PLAYER_IS_IN_DIRECTOR_MODE, toggle)
        If toggle Then Native.Function.Call(Hash.SET_ABILITY_BAR_VISIBILITY, toggle)
    End Sub

    'Dominator Tombstone
    <Extension>
    Public Function IsVehicleHaveTombstone(vehicle As Vehicle) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.GET_DOES_VEHICLE_HAVE_TOMBSTONE, vehicle)
    End Function

    <Extension>
    Public Sub HideTombstone(vehicle As Vehicle, toggle As Boolean)
        Native.Function.Call(Hash.HIDE_TOMBSTONE, vehicle, toggle)
    End Sub

    'Deluxo Flight
    <Extension>
    Public Function CanTransformFlightMode(vehicle As Vehicle) As Boolean
        Return vehicle.Bones.Contains("thrust")
    End Function

    <Extension>
    Public Sub SetHoverTransformActive(vehicle As Vehicle, toggle As Boolean)
        Native.Function.Call(Hash.SET_DISABLE_HOVER_MODE_FLIGHT, vehicle, toggle)
    End Sub

    'Car to Submarine
    <Extension>
    Public Sub TransformVehicleToSubmarine(vehicle As Vehicle, noAnimation As Boolean)
        Native.Function.Call(Hash.TRANSFORM_TO_SUBMARINE, vehicle, noAnimation)
    End Sub

    <Extension>
    Public Sub TransformSubmarineToVehicle(vehicle As Vehicle, noAnimation As Boolean)
        Native.Function.Call(Hash.TRANSFORM_TO_CAR, vehicle, noAnimation)
    End Sub

    <Extension>
    Public Function IsSubmarineVehicleTransformed(vehicle As Vehicle) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.IS_VEHICLE_IN_SUBMARINE_MODE, vehicle)
    End Function

    <Extension>
    Public Function CanTransformSubmarineMode(vehicle As Vehicle) As Boolean
        Return vehicle.Bones.Contains("turbine_hatch")
    End Function

    'Parachute
    <Extension>
    Public Sub SetParachuteModel(vehicle As Vehicle, modelHash As Integer)
        'parachute model = 230075693
        Native.Function.Call(Hash.VEHICLE_SET_PARACHUTE_MODEL_OVERRIDE, vehicle, modelHash)
    End Sub

    <Extension>
    Public Sub SetParachuteTextureVariation(vehicle As Vehicle, textureVariation As Integer)
        Native.Function.Call(Hash.VEHICLE_SET_PARACHUTE_MODEL_TINT_INDEX, vehicle, textureVariation)
    End Sub

    <Extension>
    Public Function CanActivateParachute(vehicle As Vehicle) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.GET_VEHICLE_CAN_DEPLOY_PARACHUTE, vehicle)
    End Function

    <Extension>
    Public Sub SetParachuteActive(vehicle As Vehicle, active As Boolean)
        Native.Function.Call(Hash.VEHICLE_START_PARACHUTING, vehicle, active)
    End Sub

    'Others
    <Extension>
    Public Function GetLastRammedVehicle(vehicle As Vehicle) As Vehicle
        Return Native.Function.Call(Of Vehicle)(Hash.GET_LAST_SHUNT_VEHICLE, vehicle)
    End Function

    <Extension>
    Public Function GetNumberOfVehicleDoors(vehicle As Vehicle) As Integer
        Return Native.Function.Call(Of Integer)(Hash.GET_NUMBER_OF_VEHICLE_DOORS, vehicle)
    End Function

    <Extension>
    Public Function HasRam(vehicle As Vehicle) As Boolean
        Return vehicle.Bones.Contains("ram_1mod")
    End Function

    <Extension>
    Public Function HasScoop(vehicle As Vehicle) As Boolean
        Return vehicle.Bones.Contains("scoop_1mod")
    End Function

    <Extension>
    Public Function HasSpike(vehicle As Vehicle) As Boolean
        Return vehicle.Bones.Contains("spike_1mod")
    End Function

End Module
