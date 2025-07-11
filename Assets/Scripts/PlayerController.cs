using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class infoEje
    {
        public WheelCollider ruedaIzquierda;
        public WheelCollider ruedaDerecha;
        public bool motor;
        public bool direccion;
    }

    public List<infoEje> infoEjes;
    public float maxMotorTorsion;
    public float maxAnguloDeGiro;


    void posRuedas(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform ruedaVisual = collider.transform.GetChild(0);

        Vector3 posicion;
        Quaternion rotacion;
        collider.GetWorldPose(out posicion, out rotacion);

        ruedaVisual.transform.position = posicion;
        ruedaVisual.transform.rotation = rotacion;
    }
    private void FixedUpdate()
    {
        float motor = maxMotorTorsion * Input.GetAxis("Vertical");
        float direccion = maxAnguloDeGiro * Input.GetAxis("Horizontal");

        foreach (infoEje ejeInfo in infoEjes)
        {
            if (ejeInfo.direccion)
            {
                ejeInfo.ruedaIzquierda.steerAngle = direccion;
                ejeInfo.ruedaDerecha.steerAngle = direccion;
            }
            if (ejeInfo.motor)
            {
                ejeInfo.ruedaIzquierda.motorTorque = motor;
                ejeInfo.ruedaDerecha.motorTorque = motor;
            }

            posRuedas(ejeInfo.ruedaIzquierda);
            posRuedas(ejeInfo.ruedaDerecha);

        }
    }
}
