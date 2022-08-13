using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TMPro
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextMeshPlus : MonoBehaviour
    {
        TMP_Text tmp;

        private void Awake()
        {
            tmp = GetComponent<TMP_Text>();
        }

        public void SetText(string text)
        {
            tmp.text = text;
        }
    }
}