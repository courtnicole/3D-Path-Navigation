using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav
{
    public class Billboard : MonoBehaviour
    {
       [SerializeField] private Transform target;

       private Vector3 _currentRotation;
       private Vector3 Rotation => transform.rotation.eulerAngles;

       private void OnEnable()
       {
           if (target == null)
           {
               enabled = false;
           }
       }
       
         private void Update()
         {
             transform.LookAt(Vector3.zero - target.position);
         }
    }
}
