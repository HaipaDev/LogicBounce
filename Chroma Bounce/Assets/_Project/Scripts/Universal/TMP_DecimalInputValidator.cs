using UnityEngine;
using System;

namespace TMPro{
    [Serializable]
    [CreateAssetMenu(menuName = "TextMeshPro/Input Validators/Decimal")]
    public class TMP_DecimalInputValidator : TMP_InputValidator{
        [SerializeField] float minValue = 0f;
        [SerializeField] float maxValue = 100f;
        public override char Validate(ref string text, ref int pos, char ch){
            if(ch >= '0' && ch <= '9'){
                if(text == "0"){
                    text = ch.ToString(); // Replace the leading zero with the input digit
                }else{
                    text = text.Insert(pos, ch.ToString());
                    pos += 1;
                }
                Clamp(ref text);
                return ch;
            }else if((ch == '.' || ch == ',') && !text.Contains(".") && !text.Contains(",")){
                if(pos == 0){
                    text = "0" + ch; // Convert leading decimal separator to "0."
                    pos = 2; // Set the position to after the "0."
                }else{
                    text += '.'; // Insert the decimal separator
                    pos += 1;
                }
                Clamp(ref text);
                return ch;
            }else if(ch == '-' && !text.Contains("-")){
                if(pos == 0){
                    text = "-" + text; // Add the negative sign at the beginning
                    pos += 1;
                }
                Clamp(ref text);
                return ch;
            }

            return (char)0; // Disallow any other characters
        }
        void Clamp(ref string text){
            if(float.TryParse(text, out float value)){
                value = Mathf.Clamp(value, minValue, maxValue);
                text = value.ToString();
            }
        }
    }
}