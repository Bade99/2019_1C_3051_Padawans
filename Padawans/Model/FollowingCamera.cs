﻿using System;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Core.Input;
using Microsoft.DirectX.DirectInput;

public class FollowingCamera : InteractiveElement
{
    private TGCVector3 cameraPosition;
    private TGCVector3 velocidadVectorial;
    private TGCVector3 lookAtCamera;
    private readonly float fixedDistanceCamera = -20;
    private Xwing xwing;
    private float aceleracion = 1f;
    private TGCVector3 cameraDestination;

    /// <summary>
    ///     Es el encargado de modificar la camara siguiendo a la nave
    /// </summary>
    public FollowingCamera(Xwing xwing)
    {
        this.xwing = xwing;
        velocidadVectorial = new TGCVector3();
        cameraPosition = CommonHelper.SumarVectores(xwing.GetPosition(), GetDistancePoint());
    }
    public void Update(TGC.Core.Camara.TgcCamera Camara, TgcD3dInput Input, float ElapsedTime)
    {
        cameraDestination = CommonHelper.SumarVectores(xwing.GetPosition(), GetDistancePoint());
        TGCVector3 delta = CommonHelper.RestarVectores(cameraDestination, cameraPosition);
        aceleracion = xwing.GetVelocidadGeneral() / 25;

        if (delta.X > velocidadVectorial.X / 5)
        {
            velocidadVectorial.X += aceleracion;
        }
        else
        {
            velocidadVectorial.X -= aceleracion;
        }
        if (delta.Y > velocidadVectorial.Y / 5)
        {
            velocidadVectorial.Y += aceleracion;
        }
        else
        {
            velocidadVectorial.Y -= aceleracion;
        }
        if (delta.Z > velocidadVectorial.Z / 5)
        {
            velocidadVectorial.Z += aceleracion;
        } else
        {
            velocidadVectorial.Z -= aceleracion;
        }

        cameraPosition.X += velocidadVectorial.X * ElapsedTime;
        cameraPosition.Y += velocidadVectorial.Y * ElapsedTime;
        cameraPosition.Z += velocidadVectorial.Z * ElapsedTime;

        //cameraPosition = CommonHelper.SumarVectores(xwing.GetPosition(), GetDistancePoint());

        if (Input.WheelPos == 0)
        {
            lookAtCamera = xwing.GetPosition();
            Camara.SetCamera(cameraPosition, lookAtCamera);
        }
    }

    private TGCVector3 GetDistancePoint()
    {
        float x = fixedDistanceCamera * xwing.GetCoordenadaEsferica().GetXCoord();
        float y = fixedDistanceCamera * xwing.GetCoordenadaEsferica().GetYCoord();
        float z = fixedDistanceCamera * xwing.GetCoordenadaEsferica().GetZCoord();
        return new TGCVector3(x,y,z);
    }

    public void UpdateInput(TgcD3dInput input, float ElapsedTime)
    {

    }
}
