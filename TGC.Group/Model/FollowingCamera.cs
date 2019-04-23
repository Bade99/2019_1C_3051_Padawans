using System;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Core.Input;


public class FollowingCamera
{
    private TGCVector3 cameraPosition;
    private TGCVector3 lookAtCamera;
    private readonly float fixedDistanceCamera = 40;
    private TGCVector3 minimumDistance;
    private Xwing xwing;


    /// <summary>
    ///     Es el encargado de modificar la camara siguiendo a la nave
    /// </summary>
    public FollowingCamera(Xwing xwing)
    {
        cameraPosition = new TGCVector3();
        lookAtCamera = new TGCVector3();
        this.xwing = xwing;
    }
/*
    private void CheckZoom()
    {
        if (fixedDistanceCamera.Y < minimumDistance.Y || fixedDistanceCamera.Z < minimumDistance.Z)
        {
            //no se hace con la x xq puede haber giros de camara
            fixedDistanceCamera.Y = minimumDistance.Y;
            fixedDistanceCamera.Z = minimumDistance.Z;
        }
    }
    */
    public void Update(TGC.Core.Camara.TgcCamera Camara,TgcD3dInput Input)
    {
        //CheckZoom();
        cameraPosition = CommonHelper.SumarVectores(xwing.GetPosition(), GetDistancePoint());

        if (Input.WheelPos == 0)
        {
            lookAtCamera = xwing.GetPosition();
            Camara.SetCamera(cameraPosition, lookAtCamera);
        }
        /*
        //Ruedita para alejar/acercar camara
        else if (Input.WheelPos == -1)//rueda para atras
        {
            fixedDistanceCamera.Add(new TGCVector3(0, 1, 2));
            //Camara.SetCamera(cameraPosition + new TGCVector3(0, 2, 2), Camara.LookAt);
        }
        else if (Input.WheelPos == 1)//rueda para adelante
        {
            fixedDistanceCamera.Subtract(new TGCVector3(0, 1, 2));
            //Camara.SetCamera(cameraPosition + new TGCVector3(0, -2, -2), Camara.LookAt);
        }
        */
    }

    private TGCVector3 GetDistancePoint()
    {
        float x = fixedDistanceCamera * FastMath.Cos(xwing.GetAcimutal()) * FastMath.Sin(xwing.GetPolar());
        float y = -fixedDistanceCamera * FastMath.Cos(xwing.GetPolar());
        float z = -fixedDistanceCamera * FastMath.Sin(xwing.GetAcimutal()) * FastMath.Sin(xwing.GetPolar());
        return new TGCVector3(x,y,z);
    }
}
