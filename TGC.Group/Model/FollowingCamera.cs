using System;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Core.Input;


public class FollowingCamera
{
    private TGCVector3 cameraPosition;
    private TGCVector3 lookAtCamera;
    private TGCVector3 fixedDistanceCamera;
    private TGCVector3 minimumDistance;
    private Xwing xwing;

    /// <summary>
    ///     Es el encargado de modificar la camara siguiendo a la nave
    /// </summary>
    public FollowingCamera(Xwing xwing)
    {
        cameraPosition = new TGCVector3();
        //Esta es la distancia fija que hay desde la posicion de la camara hacia el punto donde se ve
        fixedDistanceCamera = new TGCVector3(0, 5, 100);//def: 0,50,125
        minimumDistance = fixedDistanceCamera - new TGCVector3(0,10f,10f);
        lookAtCamera = new TGCVector3();
        this.xwing = xwing;
    }

    private void CheckZoom()
    {
        if (fixedDistanceCamera.Y < minimumDistance.Y || fixedDistanceCamera.Z < minimumDistance.Z)
        {
            //no se hace con la x xq puede haber giros de camara
            fixedDistanceCamera.Y = minimumDistance.Y;
            fixedDistanceCamera.Z = minimumDistance.Z;
        }
    }

    public void Update(TGC.Core.Camara.TgcCamera Camara,TgcD3dInput Input)
    {
        CheckZoom();
        cameraPosition = CommonHelper.SumarVectores(xwing.GetPosition(), fixedDistanceCamera);

        if (Input.WheelPos == 0)
        {
            lookAtCamera = xwing.GetPosition();
            Camara.SetCamera(cameraPosition, lookAtCamera);

        }
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
    }

}
