using System;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Core.Input;


public class FollowingCamera
{
    private TGCVector3 cameraDestination;
    private TGCVector3 cameraPosition;
    private TGCVector3 velocidadVectorial;
    private TGCVector3 lookAtCamera;
    private readonly float fixedDistanceCamera = -20;
    private TGCVector3 minimumDistance;
    private Xwing xwing;
    private float aceleracion;


    /// <summary>
    ///     Es el encargado de modificar la camara siguiendo a la nave
    /// </summary>
    public FollowingCamera(Xwing xwing)
    {
        this.xwing = xwing;
        velocidadVectorial = new TGCVector3();
        cameraPosition = CommonHelper.SumarVectores(xwing.GetPosition(), GetDistancePoint());
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
    public void Update(TGC.Core.Camara.TgcCamera Camara,TgcD3dInput Input, float ElapsedTime)
    {
        //CheckZoom();
        cameraDestination = CommonHelper.SumarVectores(xwing.GetPosition(), GetDistancePoint());
        TGCVector3 delta = CommonHelper.RestarVectores(cameraDestination, cameraPosition);
        //aceleracion = FastMath.Pow(xwing.GetVelocidadGeneral(), 2) / 625;
        aceleracion = xwing.GetVelocidadGeneral() / 25;
        if (delta.X > velocidadVectorial.X)
        {
            velocidadVectorial.X += aceleracion;
        }
        else if (delta.X < velocidadVectorial.X)
        {
            velocidadVectorial.X -= aceleracion;
        }
        else
        {
            velocidadVectorial.X = 0;
        }
        if (delta.Y > velocidadVectorial.Y)
        {
            velocidadVectorial.Y += aceleracion;
        }
        else if (delta.Y < velocidadVectorial.Y)
        {
            velocidadVectorial.Y -= aceleracion;
        }
        else
        {
            velocidadVectorial.Y = 0;
        }
        if (delta.Z > velocidadVectorial.Z)
        {
            velocidadVectorial.Z += aceleracion;
        }
        else if (delta.Z < velocidadVectorial.Z)
        {
            velocidadVectorial.Z -= aceleracion;
        }
        else
        {
            velocidadVectorial.Z = 0;
        }

        cameraPosition.X += velocidadVectorial.X * ElapsedTime;
        cameraPosition.Y += velocidadVectorial.Y * ElapsedTime;
        cameraPosition.Z += velocidadVectorial.Z * ElapsedTime;

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
        float y = fixedDistanceCamera * FastMath.Cos(xwing.GetPolar());
        float z = fixedDistanceCamera * FastMath.Sin(xwing.GetAcimutal()) * FastMath.Sin(xwing.GetPolar());
        return new TGCVector3(x,y,z);
    }
}
