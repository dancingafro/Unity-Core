using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    public class Billboard : MonoBehaviour
    {
        Transform cameraTransform = null;
        // Start is called before the first frame update
        void Start()
        {
            cameraTransform = Camera.main.transform;
            transform.forward = Vector3.up;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 cameraPos = cameraTransform.position;
            Vector3 pos = transform.position;
            cameraPos.y = 0;
            pos.y = 0;

            Vector3 relative = (cameraPos - pos).normalized;
            Vector3 temp = Vector3.Cross(relative, transform.right);
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.z = Mathf.Atan2(temp.z, temp.x) * Mathf.Rad2Deg;
            transform.eulerAngles = eulerAngles;
        }
    }
}