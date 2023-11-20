using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav
{
    using UnityEngine.InputSystem;

    public class seqTest : MonoBehaviour
    {
        public  InputActionReference UiClick;
        void Start()
        {
            UiClick.action.Enable();
            UiClick.action.started += LogClick;
        }

        private void LogClick(InputAction.CallbackContext obj)
        {
            Debug.Log("Click");
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
