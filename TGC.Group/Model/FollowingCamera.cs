using System;
using TGC.Core.Mathematica;
using TGC.Group.Model;

public class FollowingCamera
{
    private TGCVector3 cameraPosition;
    private TGCVector3 lookAtCamera;
    private TGCVector3 fixedDistanceCamera;
    private Xwing xwing;

    /// <summary>
    ///     Es el encargado de modificar la camara siguiendo a la nave
    /// </summary>
    public FollowingCamera(Xwing xwing)
    {
        cameraPosition = new TGCVector3();
        //Esta es la distancia fija que hay desde la posicion de la camara hacia el punto donde se ve
        fixedDistanceCamera = new TGCVector3(0, 50, 125);
        lookAtCamera = new TGCVector3();
        this.xwing = xwing;
    }

    public void Update(TGC.Core.Camara.TgcCamera camara)
    {
        cameraPosition = CommonHelper.SumarVectores(xwing.GetPosition(), fixedDistanceCamera);
        lookAtCamera = xwing.GetPosition();
        camara.SetCamera(cameraPosition, lookAtCamera);
    }

}
