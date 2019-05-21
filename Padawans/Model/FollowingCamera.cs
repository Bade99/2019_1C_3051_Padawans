using System;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Core.Input;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;

public class FollowingCamera
{
    private TGCVector3 cameraPosition;
    private float velocidadGeneral;
    private CoordenadaEsferica coordenadaEsferica;
    private TGCVector3 lookAtCamera;
    private readonly float fixedDistanceCamera = -20;
    private Xwing xwing;
    private Queue<CoordenadaEsferica> bufferAngulos;
    private int tamanioBuffer = 50;

    /// <summary>
    ///     Es el encargado de modificar la camara siguiendo a la nave
    /// </summary>
    public FollowingCamera(Xwing xwing)
    {
        this.xwing = xwing;
        this.bufferAngulos = new Queue<CoordenadaEsferica>();
        this.coordenadaEsferica = xwing.GetCoordenadaEsferica();
    }
    public void Update(TGC.Core.Camara.TgcCamera Camara, TgcD3dInput Input, float ElapsedTime)
    {
        ElapsedTime = 0.01f;//Hardcodeo hasta que sepamos como usarlo
        agregarCoordenadaEsferica();

        if (Input.WheelPos == 0)
        {
            cameraPosition = CommonHelper.SumarVectores(xwing.GetPosition(), GetDistancePoint());
            lookAtCamera = xwing.GetPosition();
            Camara.SetCamera(cameraPosition, lookAtCamera);
        }
    }

    private void agregarCoordenadaEsferica()
    {
        if (bufferAngulos.Count > tamanioBuffer)
        {
            this.coordenadaEsferica = bufferAngulos.Dequeue();
            bufferAngulos.Enqueue(xwing.GetCoordenadaEsferica());
        }
        else
        {
            bufferAngulos.Enqueue(coordenadaEsferica);
        }
    }

    private TGCVector3 GetDistancePoint()
    {
        float x = fixedDistanceCamera * this.coordenadaEsferica.GetXCoord();
        float y = fixedDistanceCamera * this.coordenadaEsferica.GetYCoord();
        float z = fixedDistanceCamera * this.coordenadaEsferica.GetZCoord();
        return new TGCVector3(x,y,z);
    }
}
