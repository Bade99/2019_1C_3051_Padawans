using System;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Core.Mathematica;
using TGC.Core.Input;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using BulletSharp;

public class FollowingCamera
{
    private TGCVector3 cameraPosition;
    private CoordenadaEsferica coordenadaEsferica;
    private TGCVector3 lookAtCamera;
    public float fixedDistanceCamera = -20;
    private float minDistance = -15, maxDistance = -50;
    private Xwing xwing;
    private float velocidadAngular = 0.75f;
    private float alcanzarMaximaVelocidadAngularEn = FastMath.PI / 6;
    private readonly static float DESVIO_ANGULO_POLAR = 0.2f;
    private float restar = 0, sumar = 0;
    private float promedioActualizacionCamara = 0;
    private float framesCount = 0;
    private float actualizacionesCount = 0;

    /// <summary>
    ///     Es el encargado de modificar la camara siguiendo a la nave
    /// </summary>
    public FollowingCamera(Xwing xwing)//@debe ser creada dsps del xwing
    {
        this.xwing = xwing;
        this.coordenadaEsferica = xwing.GetCoordenadaEsferica();
    }
    public void Update(TGC.Core.Camara.TgcCamera Camara, TgcD3dInput Input, float ElapsedTime)
    {
        UpdateInterno(Camara, Input, ElapsedTime);

    }

    private void UpdateInterno(TGC.Core.Camara.TgcCamera Camara, TgcD3dInput Input, float ElapsedTime)
    {
        //ElapsedTime = 0.01f;
        CoordenadaEsferica anguloNave = new CoordenadaEsferica(xwing.GetCoordenadaEsferica().acimutal, xwing.GetCoordenadaEsferica().polar + DESVIO_ANGULO_POLAR);
        CalcularDeltaAcimutal(ElapsedTime, anguloNave);
        CalcularDeltaPolar(ElapsedTime, anguloNave);

        RueditaMouse(Input);
        cameraPosition = CommonHelper.SumarVectores(xwing.GetPosition(), GetDistancePoint());
        lookAtCamera = xwing.GetPosition();
        Camara.SetCamera(cameraPosition, lookAtCamera);
    }

    private TGCVector3 GetDistancePoint()
    {
        TGCVector3 distance_point =  this.coordenadaEsferica.GetXYZCoord() * fixedDistanceCamera;
        return distance_point;
    }

    private void RueditaMouse(TgcD3dInput Input)
    {
        if (Input.WheelPos == -1)//rueda para atras
        {
            if (fixedDistanceCamera > maxDistance)
            {
                restar += .1f;
                fixedDistanceCamera -= restar;
            }
        }
        else if (Input.WheelPos == 1)//rueda para adelante
        {
            if (fixedDistanceCamera < minDistance)
            {
                sumar += .1f;
                fixedDistanceCamera += sumar;
            }
        }
        else
        {
            if (restar > 0) restar -= .01f;
            if (sumar > 0) sumar -= .01f;
        }
    }

    private void CalcularDeltaAcimutal(float ElapsedTime, CoordenadaEsferica anguloNave)
    {
        float deltaAcimutal = anguloNave.acimutal - this.coordenadaEsferica.acimutal;
        if (Math.Abs(deltaAcimutal) > velocidadAngular * ElapsedTime)
        {
            if (deltaAcimutal < 0 && deltaAcimutal > -FastMath.PI || deltaAcimutal > FastMath.PI)
            {
                this.coordenadaEsferica = new CoordenadaEsferica(
                    this.coordenadaEsferica.acimutal - velocidadAngular * ElapsedTime, this.coordenadaEsferica.polar);
            }
            else
            {
                this.coordenadaEsferica = new CoordenadaEsferica(
                    this.coordenadaEsferica.acimutal + velocidadAngular * ElapsedTime, this.coordenadaEsferica.polar);
            }
        }
    }
    private void CalcularDeltaPolar(float ElapsedTime, CoordenadaEsferica anguloNave)
    {
        float deltaPolar = anguloNave.polar - this.coordenadaEsferica.polar;
        if (Math.Abs(deltaPolar) > velocidadAngular * ElapsedTime)
        {
            if (deltaPolar < 0 && deltaPolar > -FastMath.PI/2 || deltaPolar > FastMath.PI)
            {
                this.coordenadaEsferica = new CoordenadaEsferica(
                    this.coordenadaEsferica.acimutal, this.coordenadaEsferica.polar - velocidadAngular * ElapsedTime);
            }
            else
            {
                this.coordenadaEsferica = new CoordenadaEsferica(
                    this.coordenadaEsferica.acimutal, this.coordenadaEsferica.polar + velocidadAngular * ElapsedTime);
            }
        }
    }
}
