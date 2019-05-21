using System;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Core.Input;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;

public class FollowingCamera
{
    private TGCVector3 cameraPosition;
    private CoordenadaEsferica coordenadaEsferica;
    private TGCVector3 lookAtCamera;
    private readonly float fixedDistanceCamera = -20;
    private Xwing xwing;
    private float velocidadAngular = 0.7f;
    private float alcanzarMaximaVelocidadAngularEn = FastMath.PI / 8;

    /// <summary>
    ///     Es el encargado de modificar la camara siguiendo a la nave
    /// </summary>
    public FollowingCamera(Xwing xwing)
    {
        this.xwing = xwing;
        this.coordenadaEsferica = xwing.GetCoordenadaEsferica();
    }
    public void Update(TGC.Core.Camara.TgcCamera Camara, TgcD3dInput Input, float ElapsedTime)
    {
        ElapsedTime = 0.01f;//Hardcodeo hasta que sepamos como usarlo
        CalcularDeltaAcimutal(ElapsedTime);
        CalcularDeltaPolar(ElapsedTime);

        if (Input.WheelPos == 0)
        {
            cameraPosition = CommonHelper.SumarVectores(xwing.GetPosition(), GetDistancePoint());
            lookAtCamera = xwing.GetPosition();
            Camara.SetCamera(cameraPosition, lookAtCamera);
        }
    }

    private TGCVector3 GetDistancePoint()
    {
        float x = fixedDistanceCamera * this.coordenadaEsferica.GetXCoord();
        float y = fixedDistanceCamera * this.coordenadaEsferica.GetYCoord();
        float z = fixedDistanceCamera * this.coordenadaEsferica.GetZCoord();
        return new TGCVector3(x,y,z);
    }

    private void CalcularDeltaAcimutal(float ElapsedTime)
    {
        float deltaAcimutal = xwing.GetCoordenadaEsferica().acimutal - this.coordenadaEsferica.acimutal;
        if (Math.Abs(deltaAcimutal) > velocidadAngular * ElapsedTime)
        {
            if (deltaAcimutal < 0 && deltaAcimutal > -FastMath.PI || deltaAcimutal > FastMath.PI)
            {
                if (deltaAcimutal > -alcanzarMaximaVelocidadAngularEn)
                {
                    this.coordenadaEsferica = new CoordenadaEsferica(
                        this.coordenadaEsferica.acimutal - velocidadAngular * ElapsedTime, this.coordenadaEsferica.polar);
                } else
                {
                    this.coordenadaEsferica = new CoordenadaEsferica(
                        this.coordenadaEsferica.acimutal - ElapsedTime, this.coordenadaEsferica.polar);
                }
            }
            else
            {
                if (deltaAcimutal < alcanzarMaximaVelocidadAngularEn)
                {
                    this.coordenadaEsferica = new CoordenadaEsferica(
                       this.coordenadaEsferica.acimutal + velocidadAngular * ElapsedTime, this.coordenadaEsferica.polar);
                } else
                {
                    this.coordenadaEsferica = new CoordenadaEsferica(
                       this.coordenadaEsferica.acimutal + ElapsedTime, this.coordenadaEsferica.polar);

                }
            }
        }
    }
    private void CalcularDeltaPolar(float ElapsedTime)
    {
        float deltaPolar = xwing.GetCoordenadaEsferica().polar - this.coordenadaEsferica.polar;
        if (Math.Abs(deltaPolar) > velocidadAngular * ElapsedTime)
        {
            if (deltaPolar < 0 && deltaPolar > -FastMath.PI/2 || deltaPolar > FastMath.PI)
            {
                if (deltaPolar > -alcanzarMaximaVelocidadAngularEn)
                {
                    this.coordenadaEsferica = new CoordenadaEsferica(
                        this.coordenadaEsferica.acimutal, this.coordenadaEsferica.polar - velocidadAngular * ElapsedTime);
                }
                else
                {
                    this.coordenadaEsferica = new CoordenadaEsferica(
                        this.coordenadaEsferica.acimutal, this.coordenadaEsferica.polar - ElapsedTime);
                }
            }
            else
            {
                if (deltaPolar < alcanzarMaximaVelocidadAngularEn)
                {
                    this.coordenadaEsferica = new CoordenadaEsferica(
                       this.coordenadaEsferica.acimutal, this.coordenadaEsferica.polar + velocidadAngular * ElapsedTime);
                }
                else
                {
                    this.coordenadaEsferica = new CoordenadaEsferica(
                       this.coordenadaEsferica.acimutal, this.coordenadaEsferica.polar + ElapsedTime);

                }
            }
        }
    }
}
